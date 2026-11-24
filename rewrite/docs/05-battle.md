# 05. Battle Engine

`CardWars.Battle` is a generic engine for turn-based, server-authoritative battles. It has no card game rules. The vanilla card game (06-vanilla-battle.md) is built on top of it. The engine references `CardWars.Core` and `CardWars.Script` only.

## 1. What the engine keeps from legacy, and what changes

Kept, because they fit a server-authoritative card game well:

- State changes only through small, serializable mutations (legacy "blocks"), so the client can replay them.
- An event queue that behaviours react to, and that can be paused to wait for a player.
- A clean split between "a player asks for something" (input) and "something happens" (event).

Changed:

| Legacy | Rewrite | Why |
|---|---|---|
| ~30 game-specific block types, each with a handler on both server and client | 4 generic changes (section 4) | The client replica needs no game code to apply state, and visibility can be computed generically |
| Entities are fixed classes; extra data goes into a string-keyed `Data` compound | Entities are an id, a type, and typed components | Mods can attach data to any entity, with a codec and a schema |
| Relationships stored twice (card has `OwnerUnitSlotId`, slot has `HoldingCardId`) and kept in sync by paired attach/detach blocks | Each relationship stored once, on the child (06-vanilla-battle.md, section 3) | One source of truth; no "detach" without "attach" bugs |
| Behaviours are created for every entity on every event, and data-driven ones are re-parsed each time | Behaviours are derived from state when it changes and indexed by event type | One allocation per entity per change, not per event |
| Input rejections are `Log.Warn` | Actions return a typed rejection to the caller | The client learns why an action failed |
| Every client receives every change | Changes are projected per viewer (section 10) | Hidden information stays hidden |
| `Guid` entity ids, `Guid.NewGuid()` | Sequential `int` ids per battle; seeded random generator in state | Deterministic replays, smaller packets |

## 2. Lifecycle

```
BattleSetup (who plays, with what, under which rules)
   -> Battle.Create(environment, setup, seed)
   -> loop: host submits actions; engine resolves; host replicates change sets
   -> Status = Ended(outcome)
```

```csharp
public sealed class Battle
{
	public static Battle Create(BattleEnvironment environment, BattleSetup setup, ulong seed);

	public BattleView View { get; }                    // read-only access to state
	public BattleStatus Status { get; }                // Running, AwaitingPrompt(seat), Ended(outcome)

	public SubmitResult Submit(SeatId seat, IPlayerAction action);
	public LegalActionSet LegalActions(SeatId seat);

	public event Action<ChangeSet>? ChangeSetApplied;  // the host's replicator listens (section 10)
}
```

- `BattleEnvironment` holds the frozen battle registries from the mod environment (entity types, component types, event types, action types, behaviour providers, setup handler, visibility rule) plus an `ILogger`. Built by the host, never by the engine.
- `BattleSetup` is game-defined data. The engine passes it to the game's registered setup handler, which creates the initial entities. The legacy "join battle" input disappears: who is in a battle is decided before it starts. Joining mid-battle, if a mode wants it, is a game-defined system action (section 8.4).
- The engine is single-threaded. The host calls it from one thread.

## 3. State

### 3.1 Entities

```csharp
public readonly record struct EntityId(int Value);

public sealed class Entity
{
	public EntityId Id { get; }
	public Key<EntityType> Type { get; }
	public ComponentMap Components { get; }    // immutable map, replaced on change
}
```

- `EntityType` is a registered content of the engine registry `battle:entity_type`. It declares which components are required at creation, so creating an entity without them fails immediately.
- Component types belong to the family `battle:entity_component` (01-data.md, section 6). Their values are immutable.
- Ids are assigned from a counter in the state, starting at 1. They are unique within one battle and never reused.

### 3.2 `BattleState`

```csharp
public sealed class BattleState
{
	// Ordered by id, so iteration order is deterministic.
	internal OrderedEntityTable Entities { get; }
	internal int NextEntityId { get; }
	internal RandomState Random { get; }        // seeded generator state (10-infrastructure.md)
	public EntityId Root { get; }               // a battle-wide entity for global components (turn state, rules)
	public BattleStatus Status { get; }
}
```

- `BattleView` is the read-only facade over state that handlers, behaviours, scripts, and visibility rules use. It offers lookups by id, `All<T>(ComponentType<T>)` queries backed by a per-component-type index, and nothing that mutates.
- Global battle data (turn order, round number, chosen rules) lives in components on the root entity, so it replicates and persists through the same mechanism as everything else.

## 4. Changes

The only way state changes:

```csharp
public abstract record Change;
public sealed record CreateEntity(EntityId Id, Key<EntityType> Type, ComponentMap Components) : Change;
public sealed record DestroyEntity(EntityId Id) : Change;
public sealed record SetComponent(EntityId Id, IComponentType Type, object Value) : Change;   // typed builders hide the object
public sealed record RemoveComponent(EntityId Id, IComponentType Type) : Change;

public sealed record ChangeSet(ImmutableArray<Change> Changes, Cue? Cue);
public sealed record Cue(Id Type, DataTag Arguments);
```

- The set of change kinds is closed. Mods do not add new ones. This is what lets the client replica and the visibility projection work with no game knowledge.
- Applying an invalid change (setting a component on a missing entity, creating an existing id, creating without required components) throws. Changes come from handlers in code, so an invalid change is a bug.
- A change set applies atomically: all changes are validated against the state first, then applied.
- `Cue` is a presentation hint. Its type is an id the game registers (for example `vanilla:attack`), and the client maps cue types to animations. The engine never interprets cues. Cue arguments may only contain entity ids and public values, because cues are not projected (section 10.4).

Handlers and behaviours build change sets through a small builder:

```csharp
context.Apply(changes => changes
	.Set(card, VanillaComponents.Stats, stats with { Hp = stats.Hp - amount })
	.WithCue(VanillaCues.TakeDamage, new TakeDamageCue(card)));
```

Changes apply immediately, so code that runs after `Apply` sees the new state. The legacy "stage then commit" step is removed.

## 5. Events

### 5.1 Declaring events

```csharp
public interface IBattleEvent;

public sealed class UnitDamagedEvent : IBattleEvent
{
	public required EntityId Target { get; init; }
	public required EntityId? Source { get; init; }
	public int Amount { get; set; }                   // modifiable by "before" listeners
}
```

Event types are registered (`battle:event_type`) with an id, the CLR type, and a script object type so scripts can read the event's fields. Fields that listeners may modify have setters; everything else is `init`.

### 5.2 Resolution pipeline

Events are queued and resolved one at a time, first in, first out. For each event:

1. **Before listeners**, in order. A listener can modify settable fields, cancel the event, or prompt.
2. **Apply**, if not cancelled. Each event type has exactly one apply handler, registered by the mod that defines the event. The handler applies changes and may queue more events.
3. **After listeners**, in order. They react to the result and may queue events or prompt.

Events queued during resolution run after the current event finishes, in the order they were queued.

### 5.3 Ordering

Listeners for one event run in a deterministic order: listener priority (lower first), then entity id, then the order the entity's behaviours were provided. No dictionary iteration order is involved anywhere.

### 5.4 Limits

A single submission may resolve at most 10,000 events (configurable per battle). Exceeding it means content created an infinite trigger loop. The battle stops with `Ended(Aborted)`, logs the last events in the queue, and the host reports it (section 12).

## 6. Behaviours

### 6.1 Providers

Behaviours are not stored in state. They are derived from it:

```csharp
public interface IBehaviourProvider
{
	ImmutableArray<IComponentType> DependsOn { get; }                // recompute when one of these changes on an entity
	IEnumerable<IBehaviour> Provide(BattleView view, Entity entity);
}
```

For example, vanilla registers a provider that depends on the card definition and location components and returns the compiled behaviours of the card's abilities, but only while the card is on the battlefield. A status-effect mod registers a provider on its own `mymod:statuses` component.

The engine keeps an index from event type and phase to listeners. When an entity is created, destroyed, or has a `DependsOn` component changed, only that entity's entries are recomputed.

### 6.2 Behaviours

```csharp
public interface IBehaviour
{
	ImmutableArray<Subscription> Subscriptions { get; }
}

public sealed record Subscription(Type EventType, EventPhase Phase, int Priority, ListenerFunc Listener);

public delegate ListenerResult ListenerFunc(ListenerContext context);
// ListenerResult: Continue | Prompt(prompt, continuation)
```

- Behaviour definitions in content are a dispatch family (`battle:behaviour`). The engine provides the `script` kind: an `on` event type, an optional `when` condition, and `do` statements, compiled against the engine's `battle:behaviour_script` signature (bindings `self` and `event`; effects and prompts allowed). Game data such as the owner is reached through members of `self`. Mods register C# kinds with their own codecs.
- Behaviour objects are compiled once during content loading and shared by every entity and every battle that uses that definition. They hold no per-battle state.
- `ListenerContext` gives the event, the entity that owns the behaviour, the `BattleView`, `Apply` for changes, `Raise` for new events, `Cancel` in the before phase, and the random generator.

## 7. Prompts

A prompt pauses resolution until one seat answers.

```csharp
public sealed record Prompt(SeatId Seat, Id Kind, ImmutableArray<PromptOption> Options, int MinPicks, int MaxPicks);
```

1. A listener returns `ListenerResult.Prompt(prompt, continuation)`. The engine sets `Status = AwaitingPrompt(seat)` and returns from `Submit`.
2. While awaiting, `LegalActions(seat)` returns only the prompt's options, and every other action from every seat is rejected with `awaiting_prompt`.
3. The seat answers with the core action `battle:answer_prompt`. The engine validates the picks against the options and calls the continuation, then resumes the event pipeline where it stopped.
4. Script behaviours prompt with the `choose` statement; their continuation is the suspended script execution (04-script.md, section 7). C# behaviours pass a continuation object.

Timeouts are the host's business. On timeout, the host submits the prompt's default answer if the prompt has one, or the game's forfeit action.

## 8. Player actions

### 8.1 Seats

A battle has seats. A seat is an entity created by the setup handler with the engine component `battle:seat`, which holds an opaque `ParticipantId` the host supplies. The host maps its players to seats; the engine never sees accounts.

### 8.2 Action types

```csharp
public interface IPlayerAction;

public interface IActionHandler<TAction> where TAction : IPlayerAction
{
	ActionCheck Check(BattleView view, SeatId seat, TAction action);       // Ok, or Rejected(reason id, details)
	void Execute(ActionContext context, SeatId seat, TAction action);      // raises events, applies changes
	IEnumerable<ILegalAction> Enumerate(BattleView view, SeatId seat);     // what this seat could do with this action type now
}
```

Action types are registered (`battle:action_type`) with an id, CLR type, codec (actions arrive from the network), and handler.

### 8.3 Submit

```
Submit(seat, action):
  if Status is Ended                        -> Rejected(battle_ended)
  if Status is AwaitingPrompt(other seat)   -> Rejected(awaiting_prompt)
  check = handler.Check(view, seat, action)
  if not ok                                 -> Rejected(check.reason)
  handler.Execute(...)
  resolve events until the queue is empty, a prompt is raised, or the battle ends
  -> Accepted
```

`Check` is the single place rules for "may this seat do this now" live. It is also what `Enumerate` must agree with; a test helper checks that every enumerated legal action passes `Check` in randomized battles.

### 8.4 System actions

Things the host does that are not a seat's choice (a player disconnecting, a forfeit on timeout, a spectator joining) are system actions with their own handler interface, submitted with `SubmitSystem`. There is no "player id None" convention.

### 8.5 Legal actions

```csharp
public sealed record LegalActionSet(ImmutableArray<ILegalAction> Actions);
```

`ILegalAction` is a dispatch family, so each game describes options compactly. For example vanilla's play-card option is `{ card, targets: [...] }` and its attack option is `{ attacker, targets: [...] }`, instead of one entry per combination. The server sends each seat its set after every resolution (08-server.md). The client builds actions only from this set. The server still runs `Check` on everything it receives.

## 9. Determinism

- One seeded generator in state (10-infrastructure.md). Handlers and behaviours take randomness from the context only.
- Entity ids from a counter.
- All collections in state and indexes iterate in a defined order.
- No time, no threads, no `Guid.NewGuid()`.

A replay test runs a battle from its setup, seed, and action log twice and compares a hash of the final state. This test exists from the first battle milestone.

## 10. Replication and visibility

### 10.1 Rule

The game registers one visibility rule:

```csharp
public interface IVisibilityRule
{
	bool CanSeeEntity(Entity entity, Viewer viewer);
	bool CanSeeComponent(Entity entity, IComponentType component, Viewer viewer);
}
// Viewer: a seat, or a spectator
```

**Constraint: the rule may only look at the entity itself.** For example vanilla decides from the card's own location and owner components. This keeps projection incremental: only entities touched by a change set can change visibility.

### 10.2 Projection by diff

`BattleReplicator` (in the engine, used by the host) keeps, per viewer, the projected state that viewer last received. After each applied change set:

1. For each entity the change set touched, compute what the viewer can now see: nothing, or the entity with the visible subset of its components.
2. Diff against the viewer's previous projection for that entity and emit `CreateEntity`, `DestroyEntity`, `SetComponent`, or `RemoveComponent` changes.
3. Send the resulting change set, with the original cue, if it is not empty or has a cue the viewer may see.

This handles reveals automatically. When a card moves from deck to hand, the owner's projection gains the card's definition component, and the opponent sees a card entity with only public components (location and owner).

### 10.3 Snapshots

A joining viewer gets the projection of the full state. A reconnecting player gets the same and replaces their replica.

### 10.4 Cues and identity

- Cues pass through unprojected, which is why cue arguments may only contain entity ids and public values. A cue that references an entity the viewer cannot see is still sent; the client ignores ids it does not know.
- Tracking a hidden card by its id across moves can leak information (for example which card went back into a shuffled deck). Games handle this by recreating entities when they enter a hidden, shuffled zone. Vanilla does this for its deck (06-vanilla-battle.md).

## 11. Persistence

`BattleState` has a codec. A battle can be saved whenever `Status` is `Running` (between submissions) or `Ended`. Saving while `AwaitingPrompt` is not supported in v1, because C# continuations cannot be serialized; script continuations could be, which leaves the door open.

## 12. Errors

| Situation | Handling |
|---|---|
| Action fails `Check` | `Rejected(reason)` returned to the host, sent to the client |
| Script runtime failure in a behaviour | Logged once with file and path; that listener is skipped; resolution continues (04-script.md, section 7.2) |
| Event limit exceeded | `Ended(Aborted)`; logged with the tail of the event queue |
| Exception from C# handler code, or an invalid change | Bug. `Submit` lets it propagate. The host's battle instance catches it at its boundary, logs it with the action that caused it, and ends the battle as aborted |

No other catch blocks exist in the engine.

## 13. Testing

- Engine unit tests with a tiny test game (two entity types, a counter component, one action, one event) so engine behaviour is tested without vanilla.
- Change application and validation tests.
- Event pipeline order, cancellation, modification, and limit tests.
- Prompt suspend and resume tests.
- Projection tests: reveal, hide, destroy, snapshot equals the sum of projected change sets.
- Determinism replay test.
- Legal-action agreement test helper.

## Open questions

1. Event order is first in, first out (same as legacy). Some card games resolve triggered effects last in, first out (a stack). Keep FIFO?

# 02. Identifiers and Registries

## 1. `Id`

```csharp
public readonly record struct Id
{
	public string Namespace { get; }
	public string Path { get; }

	public Id(string @namespace, string path);              // validates, throws on invalid input (a bug in code)
	public static bool TryParse(string text, out Id id);     // requires "namespace:path"
	public static DataResult<Id> Parse(string text, string? defaultNamespace);  // used by the Id codec
	public override string ToString();                       // "namespace:path"
}
```

Rules:

- `Namespace`: `[a-z0-9_]+`, at most 64 characters. It is always a mod id, or one of the framework namespaces `core`, `script`, `battle`, `network`, `server`, `client`, which mods cannot use as their id.
- `Path`: `[a-z0-9_./-]+`, segments separated by `/`, no empty segments, no leading or trailing `/`.
- **No implicit namespace in code.** `new Id("vanilla", "cross")` or `Id.TryParse("vanilla:cross")`. There is no `Id.Vanilla(...)` helper in the framework; the vanilla mod keeps its own `VanillaIds` constants.
- **Short ids only in content files.** In a content file, `"cross"` means `<namespace of the file>:cross` (03-modding.md, section 7). This is done by the `Id` codec using `DecodeContext.DefaultNamespace`.
- **No implicit conversions** to or from `string`. Parsing is always visible at the call site.
- Ids are compared ordinally. They are case-sensitive, and validation already forbids uppercase.

`ModId` is a separate validated string type with the namespace rules, used in manifests and registration so a mod id is never confused with a full `Id`.

## 2. Registries

### 2.1 Keys

```csharp
public sealed class RegistryKey<T> where T : notnull
{
	public Id Id { get; }                 // for example "vanilla:card"
}
```

Registry keys are `static readonly` fields owned by the mod that defines the concept:

```csharp
public static class VanillaRegistries
{
	public static readonly RegistryKey<CardDefinition> Cards = new(new Id("vanilla", "card"));
	public static readonly RegistryKey<AttackDefinition> Attacks = new(new Id("vanilla", "attack"));
}
```

### 2.2 `Registry<T>`

```csharp
public sealed class Registry<T> where T : notnull
{
	public RegistryKey<T> Key { get; }
	public RegistryOptions Options { get; }     // Synced, AllowReplace (2.4)
	public bool IsFrozen { get; }
	public int Count { get; }

	public void Register(Id id, T value, ModId owner);   // error on duplicate, error after freeze
	public void Replace(Id id, T value, ModId owner);    // explicit override of another mod's entry
	public T Get(Id id);                                 // throws if missing
	public bool TryGet(Id id, [MaybeNullWhen(false)] out T value);
	public bool Contains(Id id);
	public ModId OwnerOf(Id id);
	public IEnumerable<KeyValuePair<Id, T>> Entries { get; }   // registration order
}
```

- `Get` throws because by the time gameplay runs, every reference has been checked (section 5). A missing id at that point is a bug.
- `TryGet` is for code that handles absence on purpose, for example a command that takes an id typed by a user.
- **Registration order is preserved and deterministic**: mods in load order, files in ordinal path order within a mod. Network ids, content hashes, and iteration in game logic all depend on this.

### 2.3 Lifecycle

```
Created -> Open (mods register) -> Frozen (read-only, for the life of the mod environment)
```

The mod loader freezes every registry after content loading and linking finish. Writing to a frozen registry throws. There is no unregister; unloading means throwing the whole mod environment away (03-modding.md, section 9).

### 2.4 Duplicates and overrides

- Registering an id that already exists is an error naming both mods.
- `Replace` is allowed only if the registry was created with `AllowReplace`, and it is logged at info level with both owners. Content registries allow it, so a server mod can rebalance a vanilla card. Handler and type registries do not.
- The content pipeline uses `Replace` when a file in one mod has the same id as a file in an earlier mod (03-modding.md, section 7).

### 2.5 `RegistrySet`

```csharp
public sealed class RegistrySet
{
	public Registry<T> Get<T>(RegistryKey<T> key) where T : notnull;
	public bool TryGet<T>(RegistryKey<T> key, [MaybeNullWhen(false)] out Registry<T> registry) where T : notnull;
	public IEnumerable<IRegistry> All { get; }
}
```

Owned by the mod environment. Mods create registries through their registrar (03-modding.md, section 6), which also lets a mod create registries that other mods then fill. This replaces the legacy `CustomContent: Registry<ResourceId, object>`.

### 2.6 Framework registries

Created by the framework in every mod environment:

| Key | Holds |
|---|---|
| `core:content_type` | Content type declarations (03-modding.md) |
| `core:dispatch` | Dispatch families (01-data.md, section 4.6); each family is its own registry of kinds |
| `core:component_family` | Component families (01-data.md, section 6) |
| `core:packet` | Packet types (07-network.md) |
| `core:migration` | Data migrations (01-data.md, section 8) |
| `script:function`, `script:node` | Script functions and node kinds (04-script.md) |
| `battle:entity_type`, `battle:event_type`, `battle:action_type`, `battle:behaviour_type` | Battle engine types (05-battle.md) |

## 3. Typed keys

```csharp
public readonly record struct Key<T>(RegistryKey<T> Registry, Id Id) where T : notnull;
```

Used where a reference to content is stored and the target type matters, for example a deck holding `Key<CardDefinition>` values. The compiler then refuses to pass an attack id where a card id is expected.

## 4. Handler tables

Packets, battle events, and player actions dispatch on CLR type. A small dedicated type does this:

```csharp
public sealed class HandlerTable<TContext, TBase>
{
	public void Register<TMessage>(IHandler<TContext, TMessage> handler) where TMessage : TBase;  // one handler per type
	public void Handle(TContext context, TBase message);   // throws if no handler: the type was accepted by a codec, so a missing handler is a registration bug
}
```

It is frozen with the registries. It does not log per message.

## 5. Holders and linking

Content refers to other content. Files load in an order that does not follow those references, so references are checked after everything is decoded.

```csharp
public sealed class Holder<T> where T : notnull
{
	public Id? ReferenceId { get; }       // set when the data used an id; null when the value was inline
	public T Value { get; }               // throws if not yet linked
}
```

1. **Decode.** `Codecs.HolderOf(registryKey)` produces a `Holder<T>`. If the data was inline, the holder already has its value. If it was an id, the holder is unlinked and registers itself with `DecodeContext.Holders`.
2. **Link.** After every content type of every mod is decoded and registered, the pipeline resolves every unlinked holder against its registry. Missing ids are errors listing the file and path where the reference appeared.
3. **Validate.** Content types with cross-content rules (for example "a deck has 30 cards") run their validators, which can now read `holder.Value`.
4. **Freeze.**

After freeze, `holder.Value` is a direct object reference. Gameplay reads it with no lookup.

Holders are only for content. Runtime state that refers to content (a card entity's definition, a saved deck) stores a `Key<T>` and looks it up through the registry, because runtime state is serialized and outlives one mod environment.

## 6. Content tags (groups of ids)

Many rules want "all units from Red Alert 3" or "cards banned in this mode". Content tags are named groups of ids, defined in data and merged across mods, similar to Minecraft tags.

File `content/vanilla/tags/card/redalert3.json`:

```json
{ "values": [ "units/redalert3/apocalypse_tank", "units/redalert3/assault_destroyer", "#vanilla:redalert3_extra" ] }
```

- A tag belongs to one registry (`card` above). `#id` includes another tag of the same registry.
- Files with the same tag id from several mods are merged (union) in load order. `"replace": true` discards earlier entries.
- Tags are resolved during linking, cycles are errors, and the result is a frozen `ImmutableArray<Key<T>>` plus a set for membership tests.
- Scripts and content can test membership: `{ "type": "vanilla:in_tag", "tag": "#vanilla:redalert3", "value": ... }`.

Tags are part of the design now so registries expose them from the start. They can be implemented in a later milestone (12-plan.md).

## Open questions

1. Allow `Replace` on content registries by default (proposed), or require each content type to opt in?

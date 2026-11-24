# 04. Script Language

The script language lets content describe logic without C#: ability effects, target filters, win conditions, and later quests and world events. It lives in `CardWars.Script` and knows nothing about battles. The battle engine and other systems host it.

## 1. What changes from legacy

| Legacy | Rewrite |
|---|---|
| Each node is a record plus a separate handler class plus a registry lookup on every evaluation | Each node kind is one class. AST nodes compile once into evaluator objects |
| Scripts reach any public property of any object through reflection (`DataTagAccess`) | Scripts see only members that a host type declares (section 4) |
| Values are `DataTag`s with truthiness rules | Values are `ScriptValue`; conditions must be booleans |
| Errors become `NullTag` and a log line during play | Undeclared variables, unknown members on known types, wrong argument names, and type mismatches are found at load time |
| Pausing uses C# iterators that cannot be saved | A small instruction machine with explicit state that can be suspended and, later, serialized |
| Scripts can construct any registered type, including raw state changes, which bypasses game rules | Scripts change the world only through **effects** that the host registers. Effects go through the host's rules (in battle, they raise events) |
| `call` passes named arguments positionally in dictionary order | Arguments are matched by name against the function's declared parameters |
| Variable names are `Id`s defaulted to `cardwars:` | Variable names are plain identifiers scoped to the script |

## 2. Values

```csharp
public readonly struct ScriptValue
{
	public ScriptKind Kind { get; }   // Null, Bool, Int, Float, String, Id, List, Map, Object
}
```

- `Int` is `long`, `Float` is `double`, matching `DataTag`.
- `Id` is a separate kind so `"cross"` written as an id literal resolves to the file's namespace, while a plain string stays a string.
- `List` and `Map` are immutable.
- `Object` wraps an `IScriptObject` provided by the host (a card, a player). Objects are never stored in `DataTag`s.
- Conversions to and from `DataTag` exist for the plain kinds, so scripts can read component values and build effect arguments.

**Conditions must be `Bool`.** `if`, `and`, `or`, `not`, `when` and filter results reject other kinds. No truthiness. `exists` and `is_null` cover the common "is this set" checks.

**Numbers.** `Int op Int` stays `Int` (integer division truncates toward zero, division by zero is a runtime error). Any `Float` operand makes the result `Float`.

## 3. Program structure

### 3.1 Signatures

A script is always compiled against a signature that the host defines for that use:

```csharp
public sealed record ScriptSignature(
	Id Id,                                          // "vanilla:target_filter"
	ImmutableArray<Binding> Bindings,               // names and types the host provides, e.g. card: object<vanilla:card>
	ScriptType Result,                              // Bool for filters and conditions, None for behaviours
	ScriptCapabilities Capabilities);               // Pure, or Effects, or Effects | Prompts
```

Examples in vanilla:

| Signature | Bindings | Result | Capabilities |
|---|---|---|---|
| `vanilla:target_filter` | `card`, `player`, `candidate` | Bool | Pure |
| `vanilla:win_condition` | `team`, `alive_teams`, `alive_team_count`, `alive_player_count` | Bool | Pure |
| `vanilla:hero_lose_condition` | `hero` | Bool | Pure |
| `battle:behaviour_script` (defined by the engine, 05-battle.md section 6.2) | `self`, `event` | None | Effects, Prompts |

Pure scripts can be evaluated anywhere, any number of times. Only the server ever runs scripts; the client never does.

### 3.2 Expressions

Expressions are a dispatch family (`script:expression`, 01-data.md section 4.6). Mods can add kinds. A bare JSON number, boolean, or `null` in expression position is a literal, so simple values stay short. A bare string is a string literal.

Core kinds in `CardWars.Script`:

| Kind | Example |
|---|---|
| `var` | `{ "type": "var", "name": "alive_team_count" }` |
| `id` | `{ "type": "id", "value": "cross" }` |
| `member` | `{ "type": "member", "of": <expr>, "name": "hp" }` |
| `index` | `{ "type": "index", "of": <expr>, "at": <expr> }` |
| `call` | `{ "type": "call", "function": "vanilla:units_in_row", "args": { "battlefield": <expr>, "row": 0 } }` |
| arithmetic | `add`, `sub`, `mul`, `div`, `mod`, `neg` |
| comparison | `eq`, `ne`, `lt`, `le`, `gt`, `ge` |
| logic | `and`, `or` (short-circuit), `not` |
| `if` | `{ "type": "if", "cond": <expr>, "then": <expr>, "else": <expr> }` |
| `coalesce`, `exists`, `is_null` | |
| `list`, `map` | literal constructors |
| `count`, `any`, `all`, `filter`, `map_each` | collection operations with a named element variable |

### 3.3 Statements

Statements are the dispatch family `script:statement`. Core kinds:

| Kind | Purpose |
|---|---|
| `let` | Declare a variable in the current block |
| `set` | Assign an existing variable |
| `if` | Branch |
| `for_each` | Iterate a list, with `break` and `continue` |
| `while` | Loop, counted against the step budget |
| `return` | End the script, with a value if the signature has a result |
| `effect` | Call a host effect, for example `{ "type": "effect", "effect": "vanilla:deal_damage", "args": { "target": <expr>, "amount": 1 } }` |
| `choose` | Ask a participant to choose from options and bind the answer (needs the Prompts capability) |
| `log` | Debug output to the host logger |

### 3.4 Example: a vanilla ability

```json
{
	"description": "When this unit is damaged, deal 1 damage to the attacker.",
	"ability_type": "damaged",
	"behaviour": {
		"type": "script",
		"on": "unit_damaged",
		"when": { "type": "eq", "left": { "type": "member", "of": { "type": "var", "name": "event" }, "name": "target" }, "right": { "type": "var", "name": "self" } },
		"do": [
			{ "type": "effect", "effect": "deal_damage", "args": {
				"target": { "type": "member", "of": { "type": "var", "name": "event" }, "name": "source" },
				"amount": 1
			} }
		]
	}
}
```

This is verbose. A text syntax that compiles to the same AST (for example `when: event.target == self`) is planned after v1. The JSON form stays canonical, so nothing built now is thrown away.

## 4. Host objects

```csharp
public sealed class ScriptObjectType
{
	public Id Id { get; }                                      // "vanilla:card"
	public IReadOnlyDictionary<string, ScriptMember> Members { get; }
}

public sealed record ScriptMember(string Name, ScriptType Type, Func<IScriptObject, ScriptValue> Get);

public interface IScriptObject
{
	ScriptObjectType Type { get; }
}
```

- Host types are registered by mods (`registrar.ScriptObjectType(...)`, an extension method from `CardWars.Script`). A mod can add members to another mod's object type, for example `mymod` adding `poison` to `vanilla:card`.
- Member types are known at compile time, so `{ "type": "member", "of": <card>, "name": "hpp" }` fails at load: `unknown member "hpp" on vanilla:card (did you mean "hp")`.
- Objects are read-only to scripts. Writes go through effects.

## 5. Functions and effects

```csharp
public sealed record ScriptFunction(
	Id Id,
	ImmutableArray<Parameter> Parameters,          // name, type, optional default
	ScriptType Result,
	Func<ScriptCallContext, ScriptArgs, ScriptValue> Invoke);

public sealed record ScriptEffect(
	Id Id,
	ImmutableArray<Parameter> Parameters,
	Action<ScriptEffectContext, ScriptArgs> Invoke);   // the host decides what an effect does
```

- **Functions** are pure queries. They can be used in any signature.
- **Effects** change the world. They are allowed only in signatures with the Effects capability. In battle, every vanilla effect raises an engine event (`deal_damage` raises `UnitDamaged` through the normal pipeline), so other abilities can react, cancel, or modify it.
- Arguments are named. Missing required arguments, unknown names, and type mismatches are compile errors.

## 6. Compilation

1. **Decode** the AST with codecs during content loading.
2. **Compile** against the signature: resolve variables to slots, check member access on known types, check function and effect calls, check capabilities, check that conditions are `Bool`.
3. Produce a `CompiledScript`: an immutable object shared by every execution.

Compilation happens in the content pipeline's validate phase, so script errors appear in the same report as every other content error, with file, line, and path.

## 7. Execution

### 7.1 Model

Expressions evaluate recursively; they cannot pause. Statements compile to a flat list of instructions per script with jumps:

```
0  eval  cond#0 -> r0
1  jump_if_false r0 -> 5
2  eval  args#0 -> r1
3  effect vanilla:deal_damage r1
4  jump 5
5  return
```

```csharp
public sealed class ScriptExecution
{
	public ScriptRunResult Run(int stepBudget);      // Completed(value) | Suspended(prompt) | Failed(error)
	public void Resume(ScriptValue answer);          // after Suspended
}
```

- State is explicit: instruction pointer, locals array, and iterator positions. That makes suspending for a prompt trivial and makes saving an execution possible later.
- The step budget (default 10,000 instructions per run) stops runaway loops. Exceeding it is a runtime failure.

### 7.2 Runtime failures

Some errors can only happen at run time: division by zero, indexing past the end of a list, a null where an object was needed, the step budget. These are content bugs, so they are handled at this boundary:

- The execution stops and returns `Failed` with the script's file, path, and instruction.
- The host logs it once with full context and skips that script for this run. In battle the event continues without that behaviour (05-battle.md, section 7.5).
- Failures are counted per script so tests can assert none happened.

### 7.3 Determinism

Scripts have no access to time, threads, or the file system. Randomness comes only from a host function (`vanilla:random_int`) backed by the battle's seeded generator.

## 8. Testing

- Unit tests per node kind (compile errors and evaluation).
- Signature tests: pure scripts cannot call effects; prompts only where allowed.
- Execution tests: suspend and resume, budget exhaustion, loop control.
- The legacy scripting tests in `src/CardWars.Tests/Scripting` are ported as behaviour checks where the semantics still apply.

## Open questions

1. Strict booleans in conditions (proposed) versus legacy truthiness. Strict catches mistakes like testing a list instead of its count; it makes some conditions one node longer.

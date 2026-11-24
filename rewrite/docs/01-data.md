# 01. Data Layer

The data layer converts between C# objects, a neutral value tree, and bytes. Content files, saves, network packets, and synced registries all go through it. Every later layer depends on it, so it is designed first and most carefully.

## 1. What went wrong in the legacy `DataTag`

The legacy design (`src/CardWars.Core/Data`) was built quickly to fit a collection system. Its flaws decide most of the choices below.

| Legacy behaviour | Consequence | Rewrite answer |
|---|---|---|
| Tags are mutable (`Value { set; }`, `IntTag++` mutates in place, `CompoundTag.Set`) | Shared content gets modified by gameplay; `Clone()` is sprinkled defensively | Tags are immutable. Builders construct them |
| One type does three jobs: file format, script runtime value (`ObjectRefTag`, `IsTruthy`), and live entity storage (`GenericCard.Data`) | Each job constrains the others; live objects leak into "serializable" trees | `DataTag` is only a serializable value tree. Script values (04-script.md) and entity state (05-battle.md) are separate types |
| Numbers are `int` and `float`; `long` and `double` are truncated on write | Silent data loss | `IntTag` holds `long`, `FloatTag` holds `double`. Narrowing happens in codecs with range checks |
| `GuidTag` exists, and any JSON string that parses as a GUID becomes one | A string's kind changes on round-trip | No GUID kind. GUIDs are strings, converted by a codec |
| `NullTag`, a missing key, and C# `null` are used interchangeably | Ambiguous optional fields | A missing key and an explicit `null` are different, and codecs say which they accept |
| Polymorphism through a global `_type` table filled by assembly scanning, with a default `cardwars:` namespace | Cross-mod collisions, session sharing, silent fallback to the base type | Polymorphism is declared per base type, through a registry in the mod environment. Unknown types are errors |
| A reflection mapper with heuristics: constructor matching, `Parse` method detection, single-`Guid`-property wrappers, populating read-only collections, case and underscore insensitive matching | Hard to predict what a given type serializes to; failures are silent | Explicit codecs. No reflection in the mapping path |
| Tolerant parsing returns `0`, `false`, `""` on bad input | Typos in content become default values | Decoding returns errors with a path. Nothing defaults silently |
| Dotted-string paths into tags (`GetByPath`, `SetCardDataBlock("hp", ...)`) | Stringly-typed state access | Typed fields and typed component keys |

## 2. Layers

```
C# object  <--Codec<T>-->  DataTag  <--ITagFormat-->  bytes / text
```

- **`DataTag`**: an immutable value tree. Knows nothing about C# types or formats.
- **`Codec<T>`**: converts one C# type to and from `DataTag`, reports errors with paths, and describes its schema.
- **`ITagFormat`**: converts `DataTag` to and from bytes. JSON for files and debugging, binary for the network.

Keeping these separate means a new format (binary saves, compressed packets) never touches codecs, and a new type never touches formats.

## 3. `DataTag`

### 3.1 Kinds

```csharp
public enum TagKind : byte { Null, Bool, Int, Float, String, List, Compound }
```

Seven kinds, the same as JSON with integers and floats distinguished. This keeps the JSON mapping exact in both directions.

```csharp
public abstract class DataTag : IEquatable<DataTag>
{
	public abstract TagKind Kind { get; }
	// Structural equality and hash code. Two tags are equal if they have the same kind and content.
}

public sealed class NullTag : DataTag       { public static readonly NullTag Instance; }
public sealed class BoolTag : DataTag       { public bool Value { get; }    public static readonly BoolTag True, False; }
public sealed class IntTag : DataTag        { public long Value { get; } }
public sealed class FloatTag : DataTag      { public double Value { get; } }   // finite only; NaN and infinity are rejected
public sealed class StringTag : DataTag     { public string Value { get; } }
public sealed class ListTag : DataTag       { public ImmutableArray<DataTag> Items { get; } }
public sealed class CompoundTag : DataTag   { /* ordered, immutable string -> DataTag map, see 3.3 */ }
```

Decisions and the reasons:

- **Classes, not structs.** Trees need reference semantics for children, and the hierarchy is closed and small. Shared instances for `Null`, `True`, `False`, and small integers avoid allocations for common values. The constructors are `internal`; callers use factory methods (`DataTag.Of(42)`), which return the cached instances.
- **Immutable.** A loaded card definition can be shared by every battle, every thread, and every client sync without copying. Nothing can mutate content at runtime by accident.
- **No truthiness.** "Is this value true" is a script language rule, defined in 04-script.md.
- **Structural equality.** Needed for tests, for diffing battle state per viewer (05-battle.md), and for deduplicating synced content.
- **Finite floats only.** JSON cannot represent NaN or infinity, and they break determinism checks.

### 3.2 Lists

`ListTag` holds `ImmutableArray<DataTag>`. Items may be of mixed kinds; codecs decide what they accept. Built with `ListTag.Builder` or `ListTag.Of(...)`.

### 3.3 Compounds

```csharp
public sealed class CompoundTag : DataTag
{
	public int Count { get; }
	public DataTag? this[string key] { get; }                 // null means the key is absent
	public bool TryGet(string key, [NotNullWhen(true)] out DataTag? value);
	public bool Contains(string key);
	public IEnumerable<KeyValuePair<string, DataTag>> Entries { get; }   // insertion order

	public CompoundTag With(string key, DataTag value);       // returns a new compound
	public CompoundTag Without(string key);
	public static CompoundTag.Builder Builder();
}
```

- **Insertion order is preserved.** Saved JSON stays readable and diffable, and the binary format is deterministic, which lets the server hash synced content.
- **Storage.** Two parallel immutable arrays (keys, values). Lookup is a linear scan below 8 entries and a dictionary index built once at construction above that. Most content compounds are small.
- **Duplicate keys are an error** at build time, and the JSON reader reports them with a position.
- **Absent versus null.** `this[key]` returning C# `null` means absent. A present `NullTag` means the file wrote `null`. Codecs choose how to treat each (see 4.4).

### 3.4 What `DataTag` deliberately does not have

- No path queries. Code that needs a value uses a codec or a typed object.
- No arithmetic operators. Arithmetic belongs to the script interpreter.
- No object references. A live object never goes into a tag.
- No byte-array kind. Nothing needs it yet. If a later feature needs one, it is a new kind with a base64 JSON mapping, added in one place.

## 4. Codecs

### 4.1 Contract

```csharp
public abstract class Codec<T>
{
	public abstract DataResult<T> Decode(DataTag tag, DecodeContext context);
	public abstract DataTag Encode(T value, EncodeContext context);
	public abstract Schema Schema { get; }
}
```

- `Decode` never throws for bad input. It returns a `DataResult<T>`.
- `Encode` assumes the value is valid. An invalid value (for example an `Id` that fails validation) is a bug, and it throws.
- Codec instances are immutable and stateless, so they can be `static readonly` fields next to the type they describe. That is allowed by rule P1.

### 4.2 Results and errors

```csharp
public readonly struct DataResult<T>
{
	public bool IsSuccess { get; }
	public T Value { get; }                         // throws if not success
	public ImmutableArray<DataError> Errors { get; }
}

public sealed record DataError(DataPath Path, string Message);
// DataPath prints as $.abilities[2].behaviour.type
```

- **Errors are collected, not thrown.** A record codec decodes every field and reports every problem in a file in one pass. A modder fixes ten mistakes in one reload instead of ten.
- **Errors carry a path.** The content pipeline prefixes the file and, when the tag came from JSON, the line and column (5.2).
- **Partial results.** A failed decode has no value. There is no "best effort" object.

### 4.3 Decode and encode contexts

```csharp
public sealed class DecodeContext
{
	public string DefaultNamespace { get; }         // for short ids inside a content file (02-ids-registries.md)
	public CodecEnvironment Codecs { get; }         // dispatch registries for polymorphic types (4.6)
	public HolderSink Holders { get; }              // collects references to link after all content loads (02, section 5)
	public DecodeLimits Limits { get; }             // max depth, max list length, max string length
	public DataPath Path { get; }                   // current position; child contexts extend it
}

public sealed class EncodeContext
{
	public CodecEnvironment Codecs { get; }
	public EncodeMode Mode { get; }                 // Persistent, Network, Debug (4.8)
}
```

Contexts are per call tree, created by the caller (the content pipeline, the network layer, the save system). There is no ambient context.

### 4.4 Built-in codecs

All in `CardWars.Core.Data.Codecs`:

| Codec | Accepts | Notes |
|---|---|---|
| `Codecs.Bool` | `BoolTag` | |
| `Codecs.Int32`, `Codecs.Int64` | `IntTag` | `Int32` checks range. A `FloatTag` is an error, even `2.0` |
| `Codecs.Double`, `Codecs.Single` | `FloatTag` or `IntTag` | Integers widen to floats. `Single` checks range |
| `Codecs.String` | `StringTag` | `.MaxLength(n)`, `.NonEmpty()` |
| `Codecs.Guid` | `StringTag` in `D` format | |
| `Codecs.Id` | `StringTag` | Short form uses `DefaultNamespace` (02) |
| `Codecs.Enum<T>(map)` | `StringTag` | Explicit name map, for example `("cell", Cell), ("row", Row)`. No reflection over enum names, so renaming a C# member never changes data |
| `Codecs.ListOf(c)` | `ListTag` | `.MinCount`, `.MaxCount` |
| `Codecs.MapOf(c)` | `CompoundTag` | `string` keys |
| `Codecs.MapOf(keyCodec, c)` | `CompoundTag` | Keys converted through a codec that maps to and from strings, for example `Id` keys |
| `Codecs.Tag` | anything | Passes the raw tag through. Used for preserved unknown data (section 7) |
| `c.Nullable()` | `NullTag` or the inner kind | Explicit `null` allowed |
| `c.Map(to, from)` | | Transform between types, for example `UnitSlotPos` to and from `[x, y]` |
| `c.Validate(check, message)` | | Extra rule after decoding |
| `Codecs.Either(a, b)` | | Tries the first form, then the second. Used for "short form or full form" content |
| `Codecs.Lazy(() => c)` | | Recursive types such as script expressions |

### 4.5 Record codecs

Most content and packet types are records. A record codec lists its fields explicitly:

```csharp
public sealed record AttackPattern(PatternShape Shape, SlotOffset Offset, DamageSpec Damage)
{
	public static readonly Codec<AttackPattern> Codec = RecordCodec.Create<AttackPattern>(r =>
	{
		var shape  = r.Required("shape",  PatternShape.Codec,  x => x.Shape);
		var offset = r.Optional("offset", SlotOffset.Codec,    x => x.Offset, SlotOffset.Zero);
		var damage = r.Required("damage", DamageSpec.Codec,    x => x.Damage);
		return v => new AttackPattern(v[shape], v[offset], v[damage]);
	});
}
```

- **Field names are written by hand** in snake_case. No automatic name conversion, so renaming a C# property never changes data.
- **`Required`, `Optional` with a default, and `OptionalNullable`.** Each field states whether absence is allowed and what it means. When encoding in `Persistent` and `Network` modes, an optional field equal to its default is omitted.
- **Unknown keys are errors** by default, with a "did you mean" suggestion when a known key is within edit distance 2. A record can opt into `r.AllowExtensions(componentFamily)` to accept extension components (section 6) instead.
- **The constructor lambda receives typed field handles** (`v[shape]`), so there is no arity limit and no reflection.
- **Schema comes for free.** The builder records each field's name, codec, required flag, default, and an optional `.Doc("...")` string.

This is more typing than an attribute-driven mapper. That is a deliberate trade: the legacy mapper's guessing was the main source of surprises. A Roslyn source generator that writes these codecs from attributes can be added later without changing the runtime design (12-plan.md lists it as optional).

### 4.6 Polymorphic types (dispatch)

Script expressions, attack shapes, behaviours, player actions, and many other types are "one of several kinds, and mods can add kinds". They use a dispatch codec:

```json
{ "type": "vanilla:add", "left": { "type": "vanilla:literal", "value": 1 }, "right": ... }
```

```csharp
// Declared once by the mod that owns the base type
public static readonly DispatchKey<IScriptExpression> ExpressionKinds = new("script:expression");

// Any mod registers kinds into it
registrar.Dispatch(ExpressionKinds).Register(new Id("vanilla", "add"), typeof(AddExpression), AddExpression.Codec);

// Codec for the base type, used in other records
Codec<IScriptExpression> codec = Codecs.Dispatch(ExpressionKinds);
```

- The discriminator key is `"type"`, reserved inside dispatched compounds only. A dispatched kind cannot have its own field named `type`.
- Each dispatch family is a registry in the mod environment (02-ids-registries.md). Two mod environments can have different kinds with no interference.
- Kinds are registered with their CLR type, so encoding looks the runtime type up in the family's reverse index. Registering two kinds for one CLR type is an error.
- An unknown `type` is a decode error listing the known kinds of that family.
- A short `type` value (no namespace) resolves with the context's `DefaultNamespace`, like any other id in content.

### 4.7 References to other content

A field can reference a registry entry by id, or define the value inline. The legacy `ContentPointer { resource, definition }` did this with two optional fields. The rewrite uses one field that accepts either form:

```json
"basic": "vanilla:cross"
"basic": { "name": "Custom", "charge_cost": 2, "patterns": [ ... ] }
```

```csharp
var basic = r.Optional("basic", Codecs.HolderOf(VanillaRegistries.Attacks), x => x.Basic, defaultBasicAttack);
```

`Holder<T>` and how references are checked after loading are described in 02-ids-registries.md, section 5.

### 4.8 Encode modes

- `Persistent`: saves. Includes everything needed to restore. Omits defaults.
- `Network`: packets and synced content. Same as persistent unless a codec says otherwise (for example omitting debug-only fields).
- `Debug`: logs. Includes defaults so log lines are self-explanatory.

### 4.9 Schemas

Every codec exposes a `Schema` tree: kind, record fields with required flags, defaults, docs, enum values, list bounds, dispatch families and their kinds. Uses:

1. **Error messages** ("expected one of: cell, row, column, all").
2. **Generated JSON Schema files** for each content type, so modders get autocompletion and validation in any editor that supports JSON Schema (for example VS Code). Generated by a small tool in the repo, not at runtime.
3. **Generated reference docs** for content types.

Uses 2 and 3 are later milestones. The `Schema` property exists from the start so no codec has to be revisited.

## 5. Formats

```csharp
public interface ITagFormat
{
	DataResult<DataTag> Read(ReadOnlySpan<byte> input, FormatLimits limits);
	void Write(DataTag tag, IBufferWriter<byte> output);
}
```

### 5.1 Limits

Every read has limits: maximum depth (default 64), maximum string bytes, maximum list length, maximum total input size. Content files, saves, and network frames use different limits. The network limits are strict because the input is untrusted.

### 5.2 JSON

Built on `Utf8JsonReader` and `Utf8JsonWriter` from the BCL.

- Comments (`//`, `/* */`) and trailing commas are accepted when reading content files, because people write these files by hand. Saves and the network never use JSON in production.
- A number without a fraction or exponent that fits `long` is an `IntTag`; any other number is a `FloatTag`.
- Duplicate keys are an error.
- Errors carry line and column. When the content pipeline reads a file, it keeps a side table from tag to source position, so codec errors can say `cards/units/redalert3/apocalypse_tank.json:12:5 $.abilities[0].behaviour: unknown field "resource"`.
- Writing uses tabs, matching the repository's `.editorconfig`, and keeps key order.

### 5.3 Binary

For network frames, and later for saves if JSON becomes too slow.

```
tag      := kind:u8 payload
Null     := (empty)
Bool     := u8 (0 or 1)
Int      := zigzag varint (int64)
Float    := 8 bytes, IEEE 754 little-endian
String   := varint byte length, UTF-8 bytes
List     := varint count, tag*
Compound := varint count, (varint key length, UTF-8 key, tag)*
```

Hand-written, about 200 lines. Deterministic: the same tag always produces the same bytes, so content hashes are stable.

## 6. Components (typed extension data)

Content and state must be typed, but mods must be able to attach their own data to things other mods own, for example a poison counter on a vanilla card or a quest flag on a player profile. Components give both.

```csharp
public sealed class ComponentType<T> : IComponentType where T : notnull
{
	public Id Id { get; }
	public Codec<T> Codec { get; }
	public ComponentFlags Flags { get; }   // Persistent, Synced, and family-specific flags such as visibility (05-battle.md)
}

public sealed class ComponentMap        // immutable
{
	public bool TryGet<T>(ComponentType<T> type, [MaybeNullWhen(false)] out T value);
	public T Get<T>(ComponentType<T> type);              // throws if absent
	public T GetOrDefault<T>(ComponentType<T> type, T fallback);
	public bool Has(IComponentType type);
	public ComponentMap With<T>(ComponentType<T> type, T value);
	public ComponentMap Without(IComponentType type);
}
```

- Component types are registered in a **component family** registry, for example `battle:entity_component` or `server:profile_component`. A family is just a registry of `IComponentType`.
- **Component values must be immutable** (records, immutable collections). That lets state diffing compare by equality and lets maps share values.
- In data, a component map is a compound keyed by component id:

```json
"components": {
	"vanilla:stats": { "hp": 2, "atk": 3 },
	"mymod:poison": { "stacks": 2 }
}
```

- Unknown component ids in **content** are errors.
- Unknown component ids in **saves** are preserved as raw tags (section 7), because a mod may be temporarily removed.

## 7. Preserving unknown data

Persistent data (profiles, world saves) may contain components from a mod that is not installed right now. Dropping that data on the next save would destroy a player's progress. So `ComponentMap` decoded in `Persistent` mode keeps unknown entries as `DataTag` in an `Unresolved` bag, writes them back unchanged, and logs once per mod id. If the mod returns, the data decodes normally.

## 8. Versioning and migrations

Saves outlive code. A persistent MMO needs a way to change data shapes without breaking existing saves.

- Every persisted document has a header:

```json
{ "format": 1, "versions": { "vanilla": 3, "mymod": 1 }, "data": { ... } }
```

- `versions` records the data version of each mod that contributed data, declared by the mod at registration (`registrar.DataVersion = 3`).
- Mods register migrations per document kind:

```csharp
registrar.Migrations.Register(DocumentKinds.Profile, from: 2, to: 3, tag => /* DataTag -> DataTag */);
```

- On load, migrations run in order for each mod whose saved version is lower than the current one, operating on raw tags before codecs see them. Migrations are pure `DataTag` transforms, which is one of the reasons tags are immutable and easy to build.
- A save with a version higher than the installed mod is refused with a clear message, since downgrading is unsafe.
- Content files and network packets are not versioned: content ships with its mod, and the handshake requires matching mod versions.

## 9. Performance notes

- Content decodes once at load. Gameplay reads typed objects, never tags.
- On the success path, decoding allocates only the result objects. Error lists and path strings are created only when an error is reported.
- Network frames use the binary format. Packets with large repeated content (snapshots, content sync) can be compressed later at the frame level without touching codecs.

## 10. Testing

- Round-trip tests for every built-in codec and every format: `decode(encode(x)) == x` and `read(write(t)) == t`.
- Error tests: each failure path produces the expected path and message.
- Fuzz-style tests for the binary and JSON readers with random and truncated inputs, checking that limits hold and nothing throws.
- Architecture test: no codec type has mutable state.

## Open questions

1. Discriminator key `"type"` (proposed) versus keeping `"_type"` from legacy. `"type"` reads better in content files; `"_type"` can never clash with a field name.
2. Should content JSON allow comments? Proposed yes. Some editors flag comments in `.json` files; the alternative is a `.jsonc` extension.

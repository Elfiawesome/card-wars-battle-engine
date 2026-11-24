# 11. Conventions

These rules apply to every file in `rewrite/`. Where a rule can be checked by a tool, it is, and the build fails on violations. The rules will also be written into a steering file (`rewrite/.kiro/steering/conventions.md`) so tooling and agents follow them.

## 1. Build settings

`rewrite/Directory.Build.props` applies to every project:

```xml
<TargetFramework>net10.0</TargetFramework>
<LangVersion>latest</LangVersion>
<Nullable>enable</Nullable>
<ImplicitUsings>enable</ImplicitUsings>
<TreatWarningsAsErrors>true</TreatWarningsAsErrors>
<AnalysisLevel>latest-recommended</AnalysisLevel>
<EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
<GenerateDocumentationFile>true</GenerateDocumentationFile>
```

Only the analyzers that ship with the .NET SDK are used (P3). CI runs `dotnet build`, `dotnet test`, and `dotnet format --verify-no-changes`.

The Godot client project keeps `ImplicitUsings` on as well (legacy turned it off in the client only).

## 2. Layout

- File-scoped namespaces.
- Namespace equals assembly name plus folder path: `src/CardWars.Core/Data/Codecs/RecordCodec.cs` is `CardWars.Core.Data.Codecs`.
- One top-level type per file, named like the file. Exception: a small record and its codec, or a packet and its `PacketType`, may share a file.
- Folders and namespaces are PascalCase, also in the Godot project (legacy used `scripts.cardwars`). Godot resource files (`.tscn`, `.tres`, `.png`) are snake_case.
- Usings: `System` first, then others alphabetically. No unused usings.

## 3. Naming

| Kind | Rule | Example |
|---|---|---|
| Types, methods, properties | PascalCase | `RegistrySet`, `TryGet` |
| Interfaces | `I` + PascalCase, and they are interfaces | `IFileSystem` |
| Private fields | `_camelCase` | `_entries` |
| Locals, parameters | camelCase, full words | `attackerSlot`, not `usid`, `aTypeTex`, `evnt` |
| Constants and `static readonly` | PascalCase | `MaxFrameSize` |
| Booleans | `Is`, `Has`, `Can` prefixes | `IsFrozen` |
| Packets | end in `Packet`; direction lives in `PacketType` | `BattleActionPacket` |
| Events (battle) | end in `Event`, past tense for things that happened | `UnitDamagedEvent` |
| Player actions | end in `Action`, imperative | `PlayCardAction` |
| Ids in data | snake_case paths | `vanilla:units/redalert3/apocalypse_tank` |
| JSON keys | snake_case, written explicitly in codecs | `"charge_cost"` |
| Try pattern | `bool TryX(..., out T value)` with `[MaybeNullWhen(false)]` or `[NotNullWhen(true)]` | `TryGet` |

Abbreviations only when they are the game's own terms: `Pt`, `Hrt`, `Atk` stay in display text and content where players see them, but code uses `Points`, `HeroHealth`, `Attack`. (Open question 1.)

## 4. Language rules

- **Null checks:** `x is null` and `x is not null` for plain checks. `x is { } name` only when introducing a new variable. Never `== null`.
- **Dictionary lookups:** `TryGetValue` when absence is expected and handled. The indexer, or a `Get` that throws, when absence is a bug. Never `ContainsKey` followed by the indexer.
- **Type tests:** pattern matching (`if (x is Card card)`), not `as` plus null check.
- **Collections:** collection expressions (`[]`, `[..a, b]`). Public APIs return `IReadOnlyList<T>`, `IReadOnlyDictionary<TKey, TValue>`, `ImmutableArray<T>`, or `EquatableArray<T>`, never a mutable collection.
- **Records** for immutable data. Classes are `sealed` unless designed for inheritance.
- **Properties for public state.** No public fields (legacy had several).
- **`var`** when the type is obvious from the right-hand side; explicit types otherwise.
- **Expression-bodied members** for single expressions only.
- **No `#region`.**
- **Async:** not used in game logic. The tick thread and the battle engine are synchronous. I/O threads in transports are the exception.
- **LINQ** is fine outside hot paths. In the battle engine's resolution loop and in codecs, use loops.

## 5. Errors

The rule from 00-overview.md, P4, in practice:

| Where | How |
|---|---|
| Content load | Codecs return `DataResult` errors; validators return errors; all collected and reported |
| Network decode | Strict codecs and limits; a bad packet closes the connection with a reason |
| Player actions | `Check` returns a rejection reason |
| Script runtime | Execution returns `Failed`; the host logs and skips (04-script.md) |
| Everything else | Invariant violations throw (`InvalidOperationException`, `ArgumentException`). They are bugs |

`catch` appears only at these boundaries: content pipeline file reads, connection I/O, the instance boundary in the server, the tick loop's outer guard, and the client's frame update. Each one logs with context and contains the failure. `catch (Exception) { }` with no handling is never allowed.

Use `ArgumentNullException.ThrowIfNull` and similar guards only on public framework APIs that mods call. Internal code relies on nullable annotations.

## 6. Comments and docs

- Public framework and modding APIs have XML doc comments, since modders read them.
- Comments explain why, not what. No commented-out code; it is in git history.
- `TODO` comments include what is missing and, if it exists, the milestone or issue: `// TODO(M7): interest management for large worlds`.

## 7. Tests

- One test project per framework project (`tests/CardWars.Core.Tests`, ...), plus `tests/CardWars.Vanilla.Tests` and `tests/CardWars.EndToEnd.Tests`.
- xUnit. Test names read as sentences: `Decode_reports_unknown_field_with_suggestion`.
- No test depends on another test's state. Since there is no global state, tests run in parallel by default.
- Test fixtures (fake mods, content folders) live next to the tests in `Fixtures/`, loaded through `MemoryFileSystem` or a `LocalFileSystem` rooted there.

## 8. Architecture tests

`tests/CardWars.Architecture.Tests` checks rules that analyzers cannot:

1. **Reference graph.** Each project references only what 00-overview.md, section 6 allows. Read from the built assemblies' references.
2. **No mutable statics.** Every static field in framework and vanilla assemblies is `readonly` (or `const`) and of an allowed type: primitives, strings, `Id`, `Key<T>`, `RegistryKey<T>`, codecs, `ComponentType<T>`, immutable collections, records whose members are all allowed types. A small allowlist with a reason per entry covers anything else.
3. **No `System.IO.Path` or `File` outside file system implementations.**
4. **No Godot types outside `CardWars.Client` and `CardWars.Vanilla.Client`.**
5. **Every registered codec has a schema, and every dispatch kind round-trips** a sample value.

## 9. Git

- Branch `rewrite` only. `main` is never touched by this work.
- Commit messages follow the style already used in the repository: `feat: ...`, `fix: ...`, `refactor: ...`, `test: ...`, `docs: ...`, `chore: ...`, with a scope where helpful: `feat(core): add record codec builder`.
- Each commit builds and passes tests.

## Open questions

1. Game abbreviations in code: `Points`, `HeroHealth`, `Attack` (proposed), or keep `Pt`, `Hrt`, `Atk` so code matches the game's vocabulary?

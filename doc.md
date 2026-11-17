# Card Wars Architecture Review (v2 Overhaul)

Status: review-only. This document adds no production-code changes. It records what the
codebase looks like today, where it diverges from the stated design philosophy, and a phased
plan to bring it back in line. Every finding cites a real file and line so later passes can act
on it directly.

Baseline at time of review: `dotnet build` and `dotnet test` succeed. The suite is
`src/CardWars.Tests` and reports **123 passing, 0 failing**. Functional behavior is the thing the
overhaul must preserve; the code and tests behind it are replaceable.

---

## 1. The seven principles (scorecard)

| # | Principle | Verdict | Worst evidence |
|---|-----------|---------|----------------|
| 1 | Data-driven content loading | **Mostly holds** | Content loads from JSON through `ModContentResult`, but vanilla rules are hardcoded in C# switch arms (`VanillaMod.RegisterCardDefinitions`) |
| 2 | Moddable code (vanilla is a mod) | **Partially violated** | Vanilla is 4 separate C# projects stitched in by `.csproj` copy targets, not a self-describing code mod; `mod.json` has no `entry_point` |
| 3 | No static state / independent sessions | **Violated (severe)** | `DataTagTypeRegistry`, `DataTagMapper`, `DataTagSerializer`, `Log` are `static`; tests must call `DataTagTypeRegistry.Clear()` to isolate runs |
| 4 | Client/server separation (server godot-agnostic) | **Violated** | Core client `GameSession` depends on the vanilla `cardwars` namespace; `CardWars.Core` ships a `GodotFileProvider` |
| 5 | Battle engine reusable standalone | **Mostly holds** | `BattleEngine` is instance-scoped and has no Godot/server refs, but it still reaches global `DataTagMapper`/`DataTagTypeRegistry` for (de)serialization |
| 6 | Own systems, no outside libraries | **Holds** | Custom `DataTag` tree, custom `Registry`, custom packet transport; only `System.Text.Json` is used, and only as a transport under `DataTagSerializer` |
| 7 | One consistent pattern everywhere | **Violated** | Registries are per-instance in the engine/server but global-static in `Data`; two different `IFileProvider` Godot implementations exist; `VanillaMod` exists three times with inconsistent contracts |

---

## 2. Overview: how the subsystems are coupled

The project is split into nine projects that, on paper, form clean layers:

```
Core  ->  ModLoader  ->  BattleEngine  ->  BattleEngine.Vanilla
                                   \                      \
                                    Server  ->  Server.Vanilla
                                    Client (Godot)         |
                                    Vanilla.Shared  -------/
```

The project references respect that layering. The coupling problem is not in the `.csproj`
graph; it is in **three hidden global channels** that every layer silently reaches through,
plus **one folder-only boundary** that the compiler cannot enforce.

### 2.1 The global `Data` layer is a shared mutable singleton

`CardWars.Core.Data` exposes its main entry points as `static` classes:

- `DataTagTypeRegistry` — a process-wide `ConcurrentDictionary<string,Type>` populated by
  `ScanAssembly` (`src/CardWars.Core/Data/Global/DataTagTypeRegistry.cs:11-13`).
- `DataTagMapper` — process-wide converters plus a mutable `TypeDiscriminatorKey` setter
  (`src/CardWars.Core/Data/Mapping/DataTagMapper.cs:16,18`).
- `DataTagSerializer` — a shared mutable `JsonSerializerOptions`
  (`src/CardWars.Core/Data/Serialization/Json/DataTagSerializer.cs:8`).

Everything that serializes anything routes through these: the battle engine
(`ScriptContext.ResolveObject`), the server (`Server.RemovePlayer` saving players), the network
layer (`PacketCodec.Encode/Decode`), and the mod loader (`ModContentResult.ReadAsObject`). The
type map is filled by assembly scanning that happens in at least three independent places:
`Server.ScanCoreAssemblies` (`src/CardWars.Server/Server.cs:345-351`),
`GameSession.ScanCoreAssemblies` (`src/CardWars.Client/scenes/core/game_session/GameSession.cs:152-157`),
and `ModLoader.LoadAssemblies` (`src/CardWars.ModLoader/ModLoader.cs`, the `DataTagTypeRegistry.ScanAssembly`
loop). Two game sessions in one process share one type map; the only reason the tests pass
deterministically is that `TestBootstrap` calls `DataTagTypeRegistry.Clear()` first
(`src/CardWars.Tests/Support/TestBootstrap.cs:13`). That `Clear()` call is the smoking gun for
the principle 3 violation: isolation is achieved by resetting a global, not by scoping.

### 2.2 The client's core/vanilla split is folder-only

The client was meant to be two projects (a core client and a vanilla client) but is one Godot
project, with the boundary expressed only by directory names: `scripts/core` + `scenes/core`
versus `scripts/cardwars` + `scenes/cardwars`. Nothing stops core code from calling vanilla code,
and it does. `GameSession` (core, in `scenes/core/game_session`) has
`using CardWars.Client.scripts.cardwars;` and calls `new VanillaMod()` directly
(`src/CardWars.Client/scenes/core/game_session/GameSession.cs:7,174`). A core bootstrap object
hard-instantiates a specific mod; the comment on that line even admits it
(`// TODO: Load dynamically via scanning current assembly bruh`).

### 2.3 `Log` is global and couples logging to serialization

`CardWars.Core.Logging.Log` is static with mutable `OnLog` and `Identity`
(`src/CardWars.Core/Logging/Logger.cs:7,9,26`). Worse, its `ToReadable` path calls
`DataTagMapper.ToTag`, so the logger depends on the global data layer. Any session that sets
`Log.Identity` (GameSession does, line 45) mutates it for the whole process.

### 2.4 Pattern drift between subsystems

The engine and server model a registry as a **per-instance object**: `BattleEngineRegistry`,
`ServerRegistry`, `ClientRegistry`, `DataDrivenRegistry` are all instantiated with `new()` and
owned by their host. The `Data` layer models the same idea (an id → thing table) as a **static
singleton**. Same concept, two opposite lifetimes, which is exactly what principle 7 forbids.

---

## 3. Findings ranked by severity

### FINDING 1 (critical, principle 3) — Static `Data` layer prevents independent sessions

**What.** `DataTagTypeRegistry`, `DataTagMapper`, and `DataTagSerializer` are static, and the
type registry is mutable global state filled by `ScanAssembly`.

**Evidence.**
- `src/CardWars.Core/Data/Global/DataTagTypeRegistry.cs:9-13` — `public static class` holding two
  static `ConcurrentDictionary` maps plus a `Clear()` method.
- `src/CardWars.Core/Data/Mapping/DataTagMapper.cs:10,16,18` — `public static class`, static
  `_globalConverters`, and a settable static `TypeDiscriminatorKey`.
- `src/CardWars.Tests/Support/TestBootstrap.cs:13` — tests must `DataTagTypeRegistry.Clear()`
  then re-scan on every module init, proving runs leak into each other without it.
- Redundant scanning in `Server.cs:345-351` and `GameSession.cs:150-155` (both comment "just in
  case"), a sign nobody owns the lifetime.

**Why it is wrong.** The goal is to spin up multiple independent game sessions in one runtime and
to load/unload mods on the fly. With a global type map, loading a mod in session A changes how
session B deserializes; unloading is impossible because `Clear()` is all-or-nothing. This is the
root violation that the known pain points ("DataTag does not go through the registry system",
"static objects break independent sessions") both point at.

**Fix direction.** Introduce an instance `DataContext` (or `DataRegistry`) that owns the
type map, converters, and discriminator key, and thread it through serialize/deserialize calls.
Make it a first-class registry consistent with `BattleEngineRegistry` (addresses principle 7
too). A session creates its own `DataContext`; mods register into that context; unloading a mod
drops its entries from that context only.

### FINDING 2 (critical, principles 3 + 6) — `DataTag` serialization bypasses the registry system

**What.** The known pain point: `DataTag` type resolution uses its own static
`DataTagTypeRegistry` rather than the project's general-purpose `Registry<TId,T>` /
`RegistryFactory`. So the one place that most needs instance scoping uses the one registry that
is global.

**Evidence.**
- `src/CardWars.Core/Registry/Registry.cs` and `RegistryFactory.cs` — the real registry
  abstraction, per-instance, used everywhere else.
- `src/CardWars.Core/Data/Global/DataTagTypeRegistry.cs` — a parallel, static, bespoke registry
  for `[DataTagType]` discriminators that does not use `Registry<,>` at all.
- `src/CardWars.Core/Data/Access/DataTagAccess.cs:21-25` — per-object member access reaches into
  both static `DataTagMapper.TypeDiscriminatorKey` and static `DataTagTypeRegistry.GetTypeId`.

**Why it is wrong.** Principle 6 says build consistent in-house systems; a second registry that
ignores the first is the opposite. Principle 3 is violated because this registry is the static
one. It also makes polymorphic packet decode (`PacketCodec.Decode` ->
`DataTagMapper.FromTag<IPacket>`) silently depend on global scan order.

**Fix direction.** Express the discriminator map as a `Registry<string, Type>` (or a thin wrapper)
owned by the `DataContext` from Finding 1. Keep `ResourceId` as the id type to match the rest of
the codebase.

### FINDING 3 (high, principle 4 / 7) — Core client depends on vanilla; Core ships a Godot type

**What.** The core/vanilla client boundary is folder-only and is already breached, and
`CardWars.Core` (which the godot-agnostic server references) contains a Godot-specific file
provider.

**Evidence.**
- `src/CardWars.Client/scenes/core/game_session/GameSession.cs:7` —
  `using CardWars.Client.scripts.cardwars;` inside a `scenes/core` file.
- `src/CardWars.Client/scenes/core/game_session/GameSession.cs:174` — `new VanillaMod()`
  (the `scripts/cardwars` mod) constructed by core bootstrap, with a `// TODO ... bruh` admitting it.
- `src/CardWars.Core/Storage/GodotFileProvider.cs` — a `GodotFileProvider` living in Core; every
  method throws `NotImplementedException` except path joins. The real Godot provider is
  `src/CardWars.Client/scripts/core/storage/GDFileProvider.cs`, so Core carries a dead
  Godot-shaped stub it should never have known about.

**Why it is wrong.** Principle 4 wants a hard client/server split and a server that never needs
Godot. A core→vanilla reference means you cannot ship or test the core client without the vanilla
mod, and a Godot type in Core leaks the engine dependency toward the server layer. Principle 7 is
hit because the client expresses a boundary structurally (folders) that the engine/server express
with real project references.

**Fix direction (phased for later).** Split the client into `CardWars.Client.Core` and
`CardWars.Client.Vanilla` as separate assemblies so the compiler enforces the boundary; have core
discover `IClientMod` implementations by scanning (as the TODO wants) instead of `new`-ing a named
type. Delete `Core/Storage/GodotFileProvider.cs` or move the real implementation behind an
interface that Core defines and the client implements. Executing the client split requires Godot
4.7 on the VM (`CardWars.Client.csproj` targets `Godot.NET.Sdk/4.7.2`); it is not needed for this
review pass.

### FINDING 4 (high, principle 2) — Vanilla is not really a mod

**What.** Principle 2 says the vanilla game is itself a mod loaded like any other. In reality
vanilla is four hand-wired C# projects, and its `mod.json` does not declare a code entry point.

**Evidence.**
- `gamedata/mods/cardwars/mod.json` — `{ "id": "cardwars", ... "dependencies": [] }`, no
  `entry_point`, even though `IModManifest.EntryPoint` exists
  (`src/CardWars.ModLoader/IModManifest.cs:10`).
- `src/CardWars.BattleEngine.Vanilla/CardWars.BattleEngine.Vanilla.csproj` and
  `src/CardWars.Server.Vanilla/CardWars.Server.Vanilla.csproj` — `CopyModToVanillaFolder` build
  targets that copy the built DLLs into `gamedata/mods/cardwars/code/`. The mod "packaging" is a
  post-build file copy, not a declared artifact.
- Three different `VanillaMod` classes with three different contracts:
  `BattleEngine.Vanilla.VanillaMod : IBattleEngineMod` (has `ModName`/`Version`),
  `Server.Vanilla.VanillaMod : IServerMod` (no `ModName`/`Version` on the interface),
  `Client.scripts.cardwars.VanillaMod : IClientMod` whose `ModName`/`Version` **throw
  NotImplementedException** (`src/CardWars.Client/scripts/cardwars/VanillaMod.cs:19-20`).

**Why it is wrong.** If vanilla cannot be described and loaded through the same manifest +
entry-point path as a third-party mod, then the mod system is not actually the game's loading
mechanism; it is a side channel. The divergent `VanillaMod` contracts (principle 7) make "a mod"
mean three different things depending on layer.

**Fix direction.** Give `mod.json` an `entry_point` per side and let `ModLoader.LoadModEntry`
discover vanilla the same way it discovers any mod (it already scans assemblies for `IModEntry`).
Unify the mod entry interfaces: one base `IModEntry` with `Name`/`Version`, specialized by side.
Remove the `new VanillaMod()` shortcut in `GameSession` once discovery works.

### FINDING 5 (medium, principle 3) — `Log` is global and depends on the data layer

**Evidence.** `src/CardWars.Core/Logging/Logger.cs:7` (static class), `:9` (mutable `OnLog`),
`:26` (mutable `Identity`, set per session in `GameSession._Ready`), and `ToReadable` calling
`DataTagMapper.ToTag`. Two sessions share one logger identity and one sink.

**Why it is wrong.** Principle 3 again: per-session diagnostics are impossible, and logging drags
in the global serializer. Minor compared to Findings 1-2 but part of the same static-state family.

**Fix direction.** Make the logger an injectable instance (`ILogger`) owned by the session/server,
or at minimum scope `Identity`/`OnLog` per session. Break the `Log -> DataTagMapper` dependency by
passing an already-formatted string.

### FINDING 6 (medium, principle 7) — Two `IFileProvider` Godot implementations; inconsistent registry lifetimes

**Evidence.**
- `src/CardWars.Core/Storage/GodotFileProvider.cs` (stub, throws) vs
  `src/CardWars.Client/scripts/core/storage/GDFileProvider.cs` (real). Two classes, same job,
  different projects, one dead.
- Registry-as-instance (`BattleEngineRegistry`, `ServerRegistry`, `ClientRegistry`,
  `DataDrivenRegistry`) vs registry-as-static (`DataTagTypeRegistry`). See Finding 1.

**Fix direction.** Delete the dead stub; keep one Godot provider in the client. Converge all
registries on the instance pattern.

### FINDING 7 (low, principle 1) — Vanilla content shaping is hardcoded in C#

**Evidence.** `src/CardWars.BattleEngine.Vanilla/VanillaMod.cs` —
`RegisterCardDefinitions` mutates loaded tags in C# (`unitDataTag.Set("card_type","unit")`,
injects `intrinsic_behaviours`, default `target_filter`, default `lose_condition`), and
`GameState.Layout` is seeded with `ResourceId.Vanilla("TODO: haha wrong layout id")`
(`src/CardWars.BattleEngine/State/GameState.cs:64`).

**Why it is wrong.** Principle 1 wants content behavior to live in data. Defaults baked into C#
switch arms mean a mod cannot change them without recompiling. Low severity because the data path
exists; this is about moving the last mile into data.

**Fix direction.** Move card-type defaults and intrinsic behaviours into the JSON content or into
data-defined templates the loader applies.

---

## 4. Cross-cutting note on `ResourceId` and the `cardwars` namespace

`ResourceId.Vanilla(path)` hardcodes the namespace `"cardwars"` in Core
(`src/CardWars.Core/Registry/ResourceId.cs:12`), and `DataTagTypeRegistry.ScanAssembly` defaults
unnamed types to `ResourceId.Vanilla(...)`. Core should not know the vanilla mod's id. This is a
small but telling principle 4/7 leak: the "no favored mod" rule is broken at the id level. Fix by
having the scanning mod supply its own namespace rather than defaulting to `cardwars` in Core.

---

## 5. Phased refactor plan

Each phase is a reviewable slice that leaves the build and the 123-test suite green. Phases are
ordered by dependency and by how much later work they unblock. Behavior must be preserved across
every phase; tests may be rewritten but the game must play identically.

**Phase 0 — Guardrails (no behavior change).**
Add this `doc.md` and `AGENT.md` (this pass). Later: add an architecture test (e.g. a unit test
that fails if `CardWars.Core` references Godot, or if `scenes/core` references `scripts/cardwars`)
so regressions are caught mechanically.

**Phase 1 — Introduce an instance `DataContext` (Findings 1, 2, the `cardwars` leak).**
Create a per-instance data context that owns the type map, converters, and discriminator key.
Thread it through `DataTagMapper`/`DataTagSerializer`/`DataTagAccess` call sites. Keep a temporary
static facade so nothing breaks, then migrate callers. This is the keystone phase; it unblocks
true session isolation and mod unloading. Rewrite `TestBootstrap` to build a context instead of
`Clear()`-ing a global.

**Phase 2 — Route `DataTag` type resolution through the real registry (Finding 2).**
Replace `DataTagTypeRegistry`'s bespoke dictionaries with `Registry<string,Type>` (or `ResourceId`)
owned by the Phase 1 context. Converge on one registry pattern (principle 7).

**Phase 3 — Make logging and other statics session-scoped (Finding 5).**
Introduce `ILogger` owned by server/session; remove the `Log -> DataTagMapper` dependency; scope
`Identity`/`OnLog`.

**Phase 4 — Vanilla becomes a real mod (Finding 4).**
Add `entry_point` to `mod.json`; have `ModLoader` discover vanilla entries by scanning (it already
can); unify the three `VanillaMod` contracts under one coherent `IModEntry` hierarchy; remove
`new VanillaMod()` from `GameSession`. Depends on Phases 1-2 so mod load/unload is context-scoped.

**Phase 5 — Storage cleanup (Finding 6).**
Delete `Core/Storage/GodotFileProvider.cs`; define the storage contract in Core, implement it only
in the client. Small, independent of Phase 4.

**Phase 6 — Client core/vanilla split (Finding 3). Requires Godot 4.7 on the VM.**
Split `CardWars.Client` into `CardWars.Client.Core` and `CardWars.Client.Vanilla` assemblies so
the compiler enforces the boundary; switch core to scanning-based `IClientMod` discovery. Largest
and riskiest slice; deferred to last per the confirmed scope. Executing and validating it needs the
Godot editor/runtime (`Godot.NET.Sdk/4.7.2`), which is not required for the headless engine/server
phases above.

**Phase 7 — Move last-mile content defaults into data (Finding 7).**
Relocate hardcoded card-type defaults and intrinsic behaviours out of `VanillaMod` into JSON/data.

### Dependency summary

```
Phase 0  (docs/guardrails)   -> independent, first
Phase 1  (DataContext)       -> unblocks 2,3,4
Phase 2  (registry converge) -> needs 1
Phase 3  (logging)           -> needs 1 (light)
Phase 4  (vanilla-as-mod)    -> needs 1,2
Phase 5  (storage cleanup)   -> independent (can run anytime)
Phase 6  (client split)      -> needs 4; requires Godot 4.7
Phase 7  (content defaults)  -> needs 4
```

---

## 6. What this pass did and did not do

- Read the full `src/` tree, every `.csproj`, the mod manifests, and the test harness.
- Built the solution and ran the test suite to confirm a green baseline (123 passing).
- Added only `doc.md` and `AGENT.md`. **No production code or tests were modified.**
- Did not run the Godot client (Godot is not installed and is not needed for this review).

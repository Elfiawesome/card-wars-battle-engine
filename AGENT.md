# AGENT.md — Card Wars design philosophy (enforceable)

This file is the contract for anyone (human or agent) changing Card Wars. It turns the seven
design principles into rules with real do/don't examples from this codebase and a review
checklist. If a change breaks a rule here, it is wrong by default; either fix the change or change
this file in the same PR with a stated reason.

The architecture review that motivates these rules is in `doc.md`. Read it before a large change.

---

## The seven principles

1. **Data-driven.** Game content and its behavior live in loadable data (JSON `DataTag` content),
   not in compiled C# constants.
2. **Moddable code.** Code mods load as DLLs through `CardWars.ModLoader`. The vanilla game is
   itself a mod loaded through the same path; it gets no shortcuts.
3. **No static game state.** You must be able to run two independent game sessions in one process
   and load/unload mods on the fly without one session affecting another. No mutable `static`
   state that a session or mod touches.
4. **Client/server separation.** The server runs godot-agnostic. Core must not depend on Godot,
   and core client code must not depend on vanilla/modded code.
5. **Reusable battle engine.** `CardWars.BattleEngine` must stay usable on its own (e.g. a Discord
   bot) with no server, client, or Godot dependency.
6. **Own systems.** Prefer in-house systems (`DataTag`, `Registry`, packet transport) over third-
   party libraries. When you build one, use the existing one; do not add a parallel mechanism.
7. **One pattern everywhere.** If a pattern is used in one subsystem, every subsystem uses it.
   Flag and fix divergence.

---

## Rules and examples

### R1 — No mutable static state that a session or mod reads or writes (principles 3, 7)

State that belongs to a game session, a server, or a mod must live on an instance owned by that
session/server/mod, never in a `static` field or a `static` class.

**Don't** (current violation, `src/CardWars.Core/Data/Global/DataTagTypeRegistry.cs`):

```csharp
public static class DataTagTypeRegistry
{
    private static readonly ConcurrentDictionary<string, Type> _idToType = new();
    public static void ScanAssembly(Assembly assembly) { ... }   // global, shared across sessions
    public static void Clear();                                  // "isolation" = reset the global
}
```

**Do** — own the map on an instance and thread it through:

```csharp
public sealed class DataContext
{
    private readonly Registry<string, Type> _types = new();
    public void Register(string id, Type type) => _types.Register(id, type);
    public Type? GetType(string id) => _types.Get(id);
}
// a session creates its own DataContext; serialize/deserialize take it as a parameter.
```

Allowed static: pure, immutable helpers and caches keyed only by `Type` with no game meaning
(e.g. a reflection cache like `DataTagAccess.Properties`). If a static holds anything a mod or
session can change, it is banned.

### R2 — Serialization goes through the project's registry, not a parallel one (principles 2, 3, 6)

Polymorphic `DataTag` type resolution must use the general `Registry<,>` / `RegistryFactory`
owned by a `DataContext`, with `ResourceId` as the id. Do not stand up a second registry just for
discriminators.

**Don't:** resolve `_type` through the static `DataTagTypeRegistry` + static
`DataTagMapper.TypeDiscriminatorKey` (see `src/CardWars.Core/Data/Access/DataTagAccess.cs:21-25`).

**Do:** look the discriminator up in the context's registry, the same way
`BattleEngineRegistry.Behaviours` resolves a `ResourceId`.

### R3 — `CardWars.Core` and `CardWars.BattleEngine` never reference Godot or the server (principles 4, 5)

The engine must build and run with zero server/client/Godot types. Core must not contain Godot
types either.

**Don't** (current violation): `src/CardWars.Core/Storage/GodotFileProvider.cs` — a Godot-named
provider inside Core. Core defines `IFileProvider`; the Godot implementation belongs in the
client (`GDFileProvider`).

**Do:** Core declares the interface; each host supplies its own implementation. Keep exactly one
implementation per host, no dead stubs.

Quick check: `grep -rn "Godot" src/CardWars.Core src/CardWars.BattleEngine` must return nothing.

### R4 — Core client must not depend on vanilla/modded code (principles 4, 7)

Nothing under `scripts/core` or `scenes/core` may reference `scripts/cardwars` or
`scenes/cardwars`, and core must not construct a named mod.

**Don't** (current violation, `src/CardWars.Client/scenes/core/game_session/GameSession.cs`):

```csharp
using CardWars.Client.scripts.cardwars;   // core file importing vanilla
...
new VanillaMod().OnLoad(ClientRegistry, clientContent); // core hard-wiring a specific mod
```

**Do:** discover `IClientMod` implementations by scanning loaded mod assemblies (the existing
`ModLoader.LoadModEntry<T>()` pattern) and load whatever is found. The boundary should eventually
be two assemblies so the compiler enforces it; until then, this rule is enforced by review and the
grep below.

Quick check: `grep -rn "scripts.cardwars\|scenes.cardwars" src/CardWars.Client/scripts/core src/CardWars.Client/scenes/core`
must return nothing.

### R5 — The vanilla game is loaded as a mod, with no privileged path (principle 2)

Vanilla must be describable by a `mod.json` with an `entry_point` and discovered by the mod loader
exactly like a third-party mod.

**Don't:** rely on `.csproj` `CopyModToVanillaFolder` targets plus a hand-written `new VanillaMod()`
as the load mechanism, and don't leave `mod.json` without an `entry_point`
(see `gamedata/mods/cardwars/mod.json`).

**Do:** declare the entry point in the manifest and let `ModLoader.LoadModEntry<IModEntry>` find
it. A mod entry's `Name`/`Version` must be real values, never
`throw new NotImplementedException()` (current bug in
`src/CardWars.Client/scripts/cardwars/VanillaMod.cs:19-20`).

### R6 — One registry pattern, one id type (principle 7)

Use `Registry<TId,T>`, `RegistryFactory<...>`, or `HandlerRegistry<...>` from
`CardWars.Core.Registry` for every id → thing lookup, keyed by `ResourceId` where an id is a
content/mod identity. Do not invent a new map type for a new subsystem.

### R7 — No new third-party dependencies without justification (principle 6)

Prefer the in-house `DataTag`, `Registry`, and packet transport. Adding a NuGet package needs a
line in the PR explaining why an in-house approach does not fit. `System.Text.Json` is the one
sanctioned external, used only beneath `DataTagSerializer` as a transport.

### R8 — `CardWars.Core` must not hardcode the vanilla namespace (principles 4, 7)

Core code must not bake in `"cardwars"`. `ResourceId.Vanilla` and the scan-default in
`DataTagTypeRegistry` currently do (`src/CardWars.Core/Registry/ResourceId.cs:12`); a scanning mod
should supply its own namespace instead of defaulting to vanilla in Core.

### R9 — Logging is session-scoped and must not pull in the data layer (principles 3, 4)

Prefer an injectable logger owned by the session/server over the global `Log`. Do not add new
`Log -> DataTagMapper` coupling; format to a string at the call site.

---

## Do / Don't quick reference

| Situation | Don't | Do |
|-----------|-------|-----|
| Need a shared table of things | `static` dictionary / `static` class | instance `Registry<TId,T>` owned by a session/context |
| Resolve a polymorphic `DataTag` | static `DataTagTypeRegistry` | the `DataContext`'s registry |
| Core needs file I/O | put a Godot provider in Core | define `IFileProvider` in Core, implement per host |
| Core client needs a mod | `new VanillaMod()` | scan mod assemblies for `IClientMod` |
| Ship vanilla content/code | csproj copy + hand-wiring | `mod.json` `entry_point` + loader discovery |
| New id → thing lookup | a bespoke `Dictionary` wrapper | reuse `Registry`/`RegistryFactory`/`HandlerRegistry` |
| Add a feature needing a lib | add NuGet silently | justify vs in-house, or use `DataTag`/`Registry` |

---

## Pre-merge checklist

Tick every box or explain the exception in the PR.

- [ ] No new mutable `static` state that a session or mod reads/writes (R1).
- [ ] Any new serialization uses the instance data context, not a parallel static registry (R2).
- [ ] `grep -rn "Godot" src/CardWars.Core src/CardWars.BattleEngine` is empty (R3).
- [ ] `grep -rn "scripts.cardwars\|scenes.cardwars" src/CardWars.Client/scripts/core src/CardWars.Client/scenes/core` is empty (R4).
- [ ] No core code constructs a specific mod by name (R4).
- [ ] New mod-facing code loads via `mod.json` + `ModLoader`, not a privileged path; `Name`/`Version` are real (R5).
- [ ] New id → thing lookups reuse the existing registry types, keyed by `ResourceId` (R6).
- [ ] No new third-party dependency, or the PR explains why in-house does not fit (R7).
- [ ] No new hardcoded `"cardwars"` namespace in Core (R8).
- [ ] No new `Log -> DataTagMapper` coupling; logging is session-scoped where feasible (R9).
- [ ] `dotnet build` succeeds and `dotnet test src/CardWars.Tests/CardWars.Tests.csproj` stays green (baseline: 123 passing).
- [ ] Game behavior is unchanged (the overhaul preserves behavior; tests may be rewritten but play must match).

## Build and test (reference)

```
dotnet build src/CardWars.Tests/CardWars.Tests.csproj
dotnet test  src/CardWars.Tests/CardWars.Tests.csproj
```

The client (`CardWars.Client`) targets `Godot.NET.Sdk/4.7.2` and needs Godot 4.7 to build/run;
the engine, server, core, and tests build with the .NET SDK alone.

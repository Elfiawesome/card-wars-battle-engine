# Legacy Audit

Review of the legacy code on `main` at commit `5058c9a`, done before the rewrite design. File references point into the legacy `src/` tree. Section 2 answers questions raised at the start of the rewrite; the design documents (00 to 12) supersede the proposals made there.

## 1. Audit

### 1.1 Philosophy 1 (no global, singleton, or static state)

Running two sessions in one process is not possible today. These are the pieces of mutable process-wide state, in order of how much they block that goal:

| Static | File | Why it matters |
|---|---|---|
| `DataTagTypeRegistry` | `Core/Data/Global/DataTagTypeRegistry.cs` | Every polymorphic deserialize (packets, entities, scripts, content) goes through it. Two sessions with different mod sets would share and overwrite each other's type ids. Tests call `Clear()` in a module initializer to work around it. |
| `DataTagMapper` converters and `TypeDiscriminatorKey` | `Core/Data/Mapping/DataTagMapper.cs` | Mutable global converter table and a settable discriminator key. |
| `TypeMetadataCache`, `DataTagAccess` caches | `Core/Data/Mapping/Internal/TypeMetadataCache.cs`, `Core/Data/Access/DataTagAccess.cs` | `ConcurrentDictionary<Type, ...>` keyed by `Type`. These pin every mod assembly forever, so a collectible `AssemblyLoadContext` could never unload. |
| `Log` (`OnLog`, `Identity`) | `Core/Logging/Logger.cs` | One identity and one sink for the whole process. A host and its integrated server both log as the same user. |
| `DataTagSerializer.JsonSerializerOptions`, `PacketCodec` | `Core/Data/Serialization/Json/DataTagSerializer.cs`, `Core/Network/Packet/PacketCodec.cs` | Static entry points that implicitly use the global type registry. |
| `StorageManager.CurrentSession` | `Core/Storage/StorageManager.cs` | One open save session per storage manager. |
| Godot autoload `LoggerHook` | `Client/project.godot` | A global node whose only job is to subscribe to the static `Log`. |

Static code that is fine and can stay in spirit: `const` values, `static readonly` immutable ids (`SharedIds`, `ScriptBindings`), and pure functions (`NamingUtils.ToSnakeCase`). The rewrite states this rule explicitly (00-overview.md, P1) so "no statics" does not get applied to constants.

### 1.2 Philosophy 2 (data driven, mod compatible)

**Mod loading** (`ModLoader/ModLoader.cs`)

- All mod DLLs load into the default, non-collectible load context. Unloading is impossible, independent of the static caches above.
- A required dependency that is not installed is treated as resolved (`!_mods.ContainsKey(d)` counts as satisfied). The mod then fails later with an unrelated error.
- `LoadBefore`, `EntryPoint`, `ContentPaths`, and `MinVersion` are parsed from `mod.json` and never used.
- A new `Resolving` handler is attached on every `LoadAssemblies` call, and every assembly in the load context is re-scanned into the global type registry once per mod.
- Entry points are found by reflecting over every type in every mod assembly and calling `Activator.CreateInstance`. Each call to `LoadModEntry<T>()` creates fresh mod instances, so the battle `VanillaMod` is instantiated once for the client and once for the server.
- Content is split only by a `content/server` and `content/client` folder, and the category of a file is "its folder names", matched by string in each mod (`case ["cards", "units", ..]`).

**Namespacing**

- `ResourceId.Parse("x")` silently becomes `cardwars:x` (`Core/Registry/ResourceId.cs`). Core knows the name of the base game.
- `[DataTagType]` without an id registers the type as `cardwars:<snake_case_class_name>` regardless of which mod declared it. Two mods with a class called `AttackEvent` collide and the later one silently wins.
- `Registry<TId,T>.Register` overwrites duplicates silently. There is no enumeration, no record of which mod registered an entry, and no point at which a registry becomes read-only.

**Content handling**

- Content is stored as raw `CompoundTag` and parsed at use time, often on every call. `AttackResolver` deserializes an `AttackDefinition` each time a card attacks or is inspected; `Transaction.InstantiateBehaviour` deserializes a `BehaviourDefinition` for every entity on every event.
- Because nothing is validated at load time, typos are silent. Example: `test_mod/.../kimi.json` uses `"ability"` instead of `"abilities"` and gives `"behaviour"` as a list. The card loads and the ability is ignored.
- `PlayerJoinedEventHandler` writes defaults (`hp_max`, `charge`, `hrt_max`, ...) into the registered card definition tag (`def.Set(...)` on the object stored in `CardDefinitions`). Shared content is mutated at runtime by gameplay.
- `VanillaMod.RegisterCardDefinitions` also mutates loaded content (`card_type`, `intrinsic_behaviours`, `target_filter`, `lose_condition`). Card type is inferred from the folder a file is in.
- Base game content contains debug behaviour: `apocalypse_tank.json` points at `test_ability`, which sets the card to 1000 HP and 2000 ATK at every turn end. The starter deck and hero are hard-coded in `PlayerJoined.cs`.

### 1.3 Philosophy 3 (raw design, no external libraries)

This is mostly respected. The only package references are xUnit in the test project, and `System.Text.Json` is part of the BCL. Points to tidy:

- `IFileProvider` has 24 members, several unused, plus a static property in an interface. `GodotFileProvider` in Core is a stub that throws on every call, and the real Godot provider (`Client/scripts/core/storage/GDFileProvider.cs`) is never used; the client uses `LocalFileProvider` with a path computed from the current working directory.
- `System.IO.Path` still leaks into `GameState.Require` and `GDContentLoader`.

### 1.4 Philosophy 4 (simple design)

The codebase has the pattern you described: most handlers start with a chain of `if (state.Require<T>(id) is not { } x) return;` lines, each of which logs a warning and continues. The effect is that real bugs become log lines and the game keeps running in a wrong state. Examples:

- `TagDeserializer` turns unparseable values into `0`, `false`, `""`, or `Guid.Empty`.
- `ScriptContext.Evaluate` turns any handler failure into `NullTag`.
- Every input rejection is `Log.Warn` on the server. The client is never told why its action failed.

The rewrite should validate hard at the three trust boundaries (content load, network decode, player input) and let internal invariant violations throw. 11-conventions.md, section 5 describes this.

### 1.5 Philosophy 5 (split design)

The project references contradict the split:

```
CardWars.BattleEngine         -> Core, ModLoader          (engine depends on the mod loader and file system)
CardWars.BattleEngine.Vanilla -> BattleEngine, Vanilla.Shared  (game rules depend on network/animation ids)
CardWars.Client               -> Core, Server, Vanilla.Shared, BattleEngine.Vanilla   ("Temporary")
```

- The battle engine's mod interface takes `List<ModContentResult>`, so it cannot be used without the mod loader's file walking.
- `BattleEngineRegistry` contains Card Wars concepts (`CardDefinitions`, `AttackDefinitions`, `RuleDefinitions`, `AbilityTypeDefinitions`) and a `Registry<ResourceId, object> CustomContent` bucket.
- Game rules emit animation ids and argument keys from `Vanilla.Shared` (`SharedIds.Animations`), so presentation is defined inside the rules layer.
- The client always links the full server and the full battle rules.

### 1.6 Bugs found while reading

These are concrete defects, independent of the architecture:

1. `Log.Info/Warn/...` pass `ConsoleColor` into the `object? data` parameter of `Print`, so console colours are never applied and `LogEntry.Data` holds a colour. (`Core/Logging/Logger.cs`)
2. `BoolTag.operator false` returns `Value` instead of `!Value`. (`Core/Data/Tags/PrimitiveTags.cs`)
3. `long` is serialized by casting to `int`, and `double` to `float`, so both lose data silently. (`TagSerializer.SerializeValue`)
4. `UnitTakeDamageEvent` has no `[DataTagType]`, so data-driven abilities cannot react to damage, and `ScriptBehaviour` logs a warning for every scripted entity each time damage happens. (`Features/AttackRequest.cs`)
5. `stage_block` uses the behaviour owner (a card id) as `TargetPlayerId`. (`StageBlockInstruction.cs`, `StageBlocksInstruction.cs`)
6. `call` expressions take named arguments in a dictionary and then pass them positionally in dictionary order. (`CallExpression.cs`)
7. Every packet from an authenticated player goes to both the instance handler and the global handler registry. Packets that only an instance handles (`C2S_BattleInput`, `C2S_MoveInputPacket`) therefore log "No handler found" from the global registry every time. (`Server.ProcessPackets`)
8. `PlayerSession.TimePlayed` increments once per 16 ms tick, so it counts ticks, not time. (`Server.ServerLoop`)
9. Battle snapshots and block batches go to every player unfiltered, including opponents' hands and deck order (`BattleInstance.AddPlayer`, `BroadcastBatch` has a TODO for this). Any client can read hidden information.
10. `GameState.GlobalScope` (script globals) is not part of the snapshot, so a joining client and a reloaded battle lose it.
11. Host or join is decided by `username == "Elfiawesome"`. (`GameSession._Ready`)
12. A remote client loads battle content from its own local `content/server` folder. If the client's files differ from the server's, the client renders different cards from what the server simulates.
13. With an integrated server, the client builds its battle registry from the client content list (which has no cards), then the server's registry is replaced by the client's (`OverrideRegistry`), and then the server mod loads content into that shared object. It works only because of call order in `SetupModClient` and `SetupModServer`.
14. `TakeDamageAnimation` uses `rand.Next(-1, 1)` (returns only -1 or 0) and compares the loop index with `maxShakeAmt / 2` (0.03), so `Complete()` fires on the first shake.

---

## 2. Answers to questions raised

### 2.1 Where should content registration and systems live? (your points 2 and 6)

Each side registers only what it executes, and nothing reads files except the mod layer.

- The **mod layer** reads files, validates them against schemas, and produces typed, frozen registries.
- The **battle engine** receives a ready-made set of registries in its constructor. It never sees a file path, a manifest, or a `ModContentResult`.
- The **server** builds those registries from all installed mods and owns the authoritative simulation.
- The **client** does need part of the battle code, and that is legitimate. It keeps a replica of the game state by replaying the same blocks the server applied, so it needs entity types, block types, and block handlers. It does not need input handlers, event handlers, behaviours, rules, the script interpreter, or content read from its own disk.

So the answer to point 6 is "partly". The client should not load the battle rules mod or register server content. It should load a smaller battle model module (state and blocks) and receive the content it displays from the server.

Today the client also runs `BattleActionValidator` locally to highlight legal targets, which is why it needs the rules and the script interpreter. I recommend replacing that with server-computed legal actions: after each batch, the server sends the active player the list of actions it would accept (playable cards and their targets, attackers and their targets). The client highlights from that list. This removes the rules from the client, keeps one source of truth, and leaks no extra information.

### 2.2 Can a C# custom Resource from a DLL be referenced by a mod's PCK? (your point 3)

Not reliably in Godot 4. Godot binds a C# script to its class through the main project assembly. When a scene or resource inside a separately loaded PCK references a C# script that lives in a mod DLL, Godot reports that the associated class could not be found. This is a known, long-standing issue ([godot#76074](https://github.com/godotengine/godot/issues/76074)), and forum users on 4.4 report the same limit with workarounds that reattach scripts by hand ([forum thread](https://forum.godotengine.org/t/how-to-instantiate-a-c-scene-from-an-external-pck/118109)). I have not tested this on 4.7 specifically.

Two things do work:

- A mod PCK can reference C# Resource and Node scripts that live in the **client core assembly**, because that assembly is the one Godot scans.
- A mod PCK can contain GDScript, because GDScript is stored as a resource in the pack.

Design consequence for the rewrite:

- Client core ships a small set of generic C# node and resource types that mod scenes may use (for example a `CardDisplay` base, an `EntityView` node with exported fields).
- Mod scenes in a PCK contain no mod-owned C# scripts. They are plain scenes, optionally with GDScript for visual logic.
- Mod C# code (in the mod's client DLL) attaches behaviour to scenes from the outside, through registered view factories that look up nodes by exported path or group.

### 2.3 Is hot loading and unloading feasible?

- **Content hot reload: yes.** Once registries are built per session and frozen, reloading content means building a new set of registries and starting new instances with them. Running battles keep the old set until they end. This also works for pushing new data-only content to clients.
- **Server DLL unload: possible later, not in v1.** It requires each mod in a collectible `AssemblyLoadContext` and zero references from long-lived objects into mod types. The static caches in Core make this impossible today; the rewrite removes them, so the door stays open.
- **Client DLL unload: not practical.** Godot's .NET integration holds references to types it has seen. Plan for "restart the client session" instead of unloading client code.

Recommendation: design for whole-session teardown (dispose the session, build a new one). That gives content reload now and keeps server DLL unloading achievable.

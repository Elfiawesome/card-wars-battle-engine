# 03. Modding

## 1. What a mod is

A mod is a folder (or later a zip) with a manifest and up to three sides. Every side is optional. A mod with no code is a data pack. The base game is the mod `vanilla` and has no special treatment.

```
vanilla/
  mod.json
  common/
    code/           CardWars.Vanilla.Common.dll (+ .pdb)
    content/        data loaded on both server and client (see section 7 for which is synced)
  server/
    code/           CardWars.Vanilla.Server.dll
    content/        server-only data: worlds, rules, encounter tables, loot
  client/
    code/           CardWars.Vanilla.Client.dll
    content/        client-only data: layouts, animation timings, UI themes
    assets.pck      textures, scenes, fonts, sounds
```

Why three sides instead of the legacy two content folders: the `common` side is what both processes need (types, codecs, components, packets, and content that clients display). Putting it in one place is what lets a remote client get card data from the server instead of from its own disk, and lets the client avoid loading server rules.

## 2. Manifest

`mod.json`, decoded with a record codec like any other content, so errors are reported the same way.

```json
{
	"id": "vanilla",
	"name": "Card Wars",
	"version": "0.1.0",
	"data_version": 1,
	"dependencies": [
		{ "id": "some_lib", "version": ">=1.2.0 <2.0.0" },
		{ "id": "optional_thing", "version": ">=0.1.0", "optional": true }
	],
	"load_after": [],
	"load_before": [],
	"entry": {
		"common": "CardWars.Vanilla.Common.VanillaCommonMod",
		"server": "CardWars.Vanilla.Server.VanillaServerMod",
		"client": "CardWars.Vanilla.Client.VanillaClientMod"
	}
}
```

| Field | Rules |
|---|---|
| `id` | A valid `ModId`. Reserved: `core`, `script`, `battle`, `network`, `server`, `client` (framework namespaces) |
| `version` | Semantic version `major.minor.patch[-pre]`. Parsed by a small built-in type |
| `data_version` | Integer, the version of this mod's persisted data shapes (01-data.md, section 8) |
| `dependencies` | Version ranges with `>=`, `>`, `<=`, `<`, `=`, space-separated meaning "and" |
| `load_after`, `load_before` | Ordering hints without a dependency |
| `entry` | Fully qualified type name per side. Required for a side that has `code/` |

## 3. Discovery

The host gives the loader a list of mod roots (virtual paths, 10-infrastructure.md). For a dedicated server: `mods/` next to the server and the save's own `mods/`. For a client: installed client mods only.

Errors at discovery: two mods with the same id, unreadable or invalid manifest, a side with `code/` but no entry. Each error names the folder.

## 4. Resolution

1. Check every required dependency is present and its version is in range. Optional dependencies that are present must also be in range.
2. Build ordering edges: dependency before dependent, `load_after`, `load_before`.
3. Topological sort. Ties broken by mod id, so the order is the same on every machine.
4. A cycle is an error listing the cycle.

The result is a `ModSet`: the ordered list of mods for one side.

## 5. Assembly loading

### 5.1 Load contexts

Each mod environment gets one `ModLoadContext : AssemblyLoadContext`.

- **Framework assemblies** (`CardWars.Core`, `CardWars.Battle`, and the rest) always resolve from the host's context. Mods and the host must share one copy of the contract types.
- **Mod assemblies** resolve from the `code/` folders of the mods in the `ModSet`, and only those. A mod can reference another mod's assembly only if it declares that mod as a dependency.
- On the **server**, the context is collectible, so unloading the whole environment is possible later.
- On the **client**, the context's parent is the context Godot loaded the client assembly into, so `GodotSharp` resolves to the engine's copy. It is not collectible.
- Assemblies load from streams so files are not locked while the game runs.

### 5.2 One process, two environments

With an integrated host, the client and the server each have a mod environment, and each loads `CardWars.Vanilla.Common.dll` into its own context. The two copies are different CLR types. That is fine, and intended: the client and server talk only through serialized packets (07-network.md), never by passing objects. The legacy object-passing local connection is removed so this rule is enforced.

### 5.3 Rule for mod code

A mod's code must not keep state outside the objects its registrar gives it. Static mutable state in a mod would leak across environments and block unloading. The architecture test that checks framework assemblies (11-conventions.md, section 8) also runs over `vanilla`.

## 6. Entry points and registrars

Each side has an entry interface (defined below with the registrars).

- The loader instantiates each entry type named in the manifest once per environment, with a parameterless constructor.
- On the server, a mod's `common` entry runs, then its `server` entry. On the client, `common` then `client`. Mods run in `ModSet` order, so mod B's `common` runs after mod A's `common` and `server`.
- The registrar is created for that mod only. It knows the mod's id and uses it as the owner of everything registered.

`CardWars.Modding` only references `CardWars.Core`, so its registrar can only offer Core concepts:

```csharp
public class ModRegistrar
{
	public ModId ModId { get; }
	public ILogger Logger { get; }
	public Side Side { get; }

	public Registry<T> CreateRegistry<T>(RegistryKey<T> key, RegistryOptions options) where T : notnull;
	public Registry<T> Registry<T>(RegistryKey<T> key) where T : notnull;     // created by the framework, this mod, or earlier mods
	public DispatchRegistrar<TBase> Dispatch<TBase>(DispatchKey<TBase> key);
	public void ContentType<T>(ContentType<T> type) where T : notnull;
	public void ComponentType<T>(ComponentFamilyKey family, ComponentType<T> type) where T : notnull;
	public MigrationRegistrar Migrations { get; }
	public int DataVersion { get; set; }
}
```

Higher layers add their concepts without Modding knowing about them:

- **Environment modules.** Before any mod loads, the host installs framework modules into the environment. `ScriptModule`, `BattleModule`, and `NetworkModule` each create their registries (script nodes, battle entity types, packet types, and so on; 02-ids-registries.md, section 2.6) and register their built-in kinds. A dedicated server installs Script, Battle, Network, and Server modules; a client installs Script, Battle, Network, and Client modules.
- **Extension methods.** Each layer ships extension methods on `ModRegistrar` for its registries, so mod code reads naturally: `registrar.BattleEvent<UnitDamagedEvent>(...)` (from `CardWars.Battle`), `registrar.Packet(BattleActionPacket.Type)` (from `CardWars.Network`), `registrar.View(...)` (from `CardWars.Client`).
- **Side registrars.** `CardWars.Server` defines `ServerRegistrar : ModRegistrar` (hooks, instance types, commands, packet handlers). `CardWars.Client.Core` defines `ClientRegistrar : ModRegistrar` (packet handlers, instance sessions). The host passes a registrar factory to the loader, which is how the loader creates a side registrar without referencing the side.

```csharp
public interface ICommonModEntry { void Register(ModRegistrar registrar); }
public interface IServerModEntry { void Register(ServerRegistrar registrar); }   // in CardWars.Server
public interface IClientModEntry { void Register(ClientRegistrar registrar); }   // in CardWars.Client.Core
```

Godot-specific registration (views, assets) is extension methods from `CardWars.Client` over registries that the client's Godot module creates, so `Client.Core` stays free of Godot types.

Registration is the only time mods mutate shared structures. Anything a mod needs at run time (a registry, a logger) it receives from a context when its handlers are called, or captures from the registrar during `Register`.

## 7. Content pipeline

### 7.1 Content types

A content type ties a folder name to a registry, a codec, and a side:

```csharp
public sealed record ContentType<T>(
	RegistryKey<T> Registry,
	string Folder,                  // "cards"
	Codec<T> Codec,
	ContentSide Side,               // Common, Server, Client
	bool Synced,                    // Common only: send to clients on join (section 8)
	Func<T, ValidationContext, IEnumerable<DataError>>? Validate = null) where T : notnull;
```

Vanilla declares, for example: `cards`, `attacks`, `ability_types`, `decks`, `battle_rules`, `battlefield_layouts` (common, synced); `worlds`, `encounters` (server); `battle_layouts`, `card_skins` (client).

### 7.2 File layout and ids

```
<side>/content/<namespace>/<folder>/<path>.json   ->   id  <namespace>:<path>   in the registry of <folder>
```

Example: `vanilla/common/content/vanilla/cards/units/redalert3/apocalypse_tank.json` becomes `vanilla:units/redalert3/apocalypse_tank` in the card registry.

- The namespace folder is what lets a mod override another mod's content deliberately: `myserver/common/content/vanilla/cards/units/redalert3/apocalypse_tank.json` replaces the vanilla card. This is logged.
- A mod adding new content uses its own namespace folder.
- Inside a file, short ids resolve to the file's namespace folder, not to the mod that shipped the file. An override file reads exactly like the original.
- Only `.json` files are read. Anything else in a content folder is an error, to catch misplaced files early.

### 7.3 Phases

All mods' content for a side goes through these phases together:

1. **Read.** For each mod in order, each declared content type, each file in ordinal path order: read JSON with content limits.
2. **Decode** with the type's codec. `DefaultNamespace` is the file's namespace folder.
3. **Register** into the type's registry, using `Replace` when an earlier mod already has the id.
4. **Tags.** Merge content tags (02-ids-registries.md, section 6).
5. **Link** all holders and tags.
6. **Validate.** Run each content type's validator.
7. **Freeze** all registries.

Errors from all phases are collected. If there are any, the environment fails to build, and the host shows all of them at once, grouped by mod and file. A server does not start with broken content. A client cannot join with broken local client content.

### 7.4 What a modder sees

```
[vanilla] failed to load 2 files
  common/content/vanilla/cards/units/redalert3/apocalypse_tank.json:14:6  $.abilities[0].behaviour: expected a behaviour id or an object with "type", found an object without "type"
  common/content/vanilla/cards/heroes/kimi.json:4:2  $: unknown field "ability" (did you mean "abilities"?)
```

## 8. Content sync to clients

The server is the source of truth for content too.

1. **Manifest.** During login, the server sends the list of its mods: id, version, sides present, and a hash of each mod's common content after loading. Mods that have `client/` code or assets are marked `client_install_required`.
2. **Check.** The client compares with its installed client mods. A missing required mod or a version mismatch ends the connection with a message naming the mods. (Downloading is a later feature; the manifest already has what it needs.)
3. **Data.** The server sends every registry whose content type is `Synced`, encoded with the codecs from the common assemblies. Data-only mods need nothing installed on the client: their content arrives here.
4. **Build.** The client builds a session mod environment: its installed client mods' code plus the received data, then runs the client content pipeline for client-side content and links everything. Client-side content can reference synced content (a card skin references a card).
5. The client never reads `common/content` from its own disk when connected to a server. Its local copy of `vanilla` is used only for code and client assets.

Integrated host is the same flow over a local connection. There is no shortcut that shares registries between client and server.

## 9. Environment lifecycle and reloading

A `ModEnvironment` is created, used, and disposed as a whole:

```csharp
public sealed class ModEnvironment : IDisposable
{
	public ModSet Mods { get; }
	public CodecEnvironment Codecs { get; }
	public RegistrySet Registries { get; }
	public ILogger Logger { get; }
}
```

- **Content reload (planned, after v1):** build a new environment from the same code and freshly read content. New instances use the new environment; running battles finish on the old one; clients get re-synced. Code does not change, so the existing load context is reused.
- **Server code unload (possible later):** dispose every instance and environment using a collectible context, then unload it. This works only if nothing outside holds a reference into mod types, which the no-static rule and the per-environment caches make achievable.
- **Client code unload:** not supported. Godot keeps references to types it has seen. Leaving a server disposes the session environment but not the client mod assemblies.

## Open questions

1. Should data-only mods be allowed in a client's installed mods at all (for client-side content only), or must client-side content always come with a client mod that has code? Proposed: allowed.

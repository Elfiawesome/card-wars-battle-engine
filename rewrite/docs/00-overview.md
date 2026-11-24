# 00. Overview

## 1. What Card Wars is

Card Wars is a server-authoritative, turn-based card battle MMORPG. Players explore worlds like a normal RPG, and encounters turn into card battles. Story, quests, game modes, events, and roguelike runs sit on top. Everything the game does is meant to be built from mods, including the base game itself, which is the mod `vanilla`.

## 2. Goals of the rewrite

1. Two complete game sessions (for example two servers, or a client plus an integrated server) can run in one process without sharing any state.
2. The base game uses only the public modding API. Nothing in the framework knows the string `vanilla`.
3. Content is typed and validated when it loads. A mistake in a content file produces an error naming the file and the field, not silent misbehaviour later.
4. Servers can add data-only content that clients receive automatically, without installing anything.
5. The battle engine is a standalone library usable in another game.
6. A pure client build contains no server code, and a dedicated server contains no client or Godot code.
7. The server is the only source of truth. The client never runs game rules.
8. Code is consistent enough that any file reads as if one person wrote it.

## 3. Non-goals for the first version

- Hot unloading of mod DLLs. The design keeps it possible on the server (see 03-modding.md), but it is not built in v1.
- Downloading missing client mods or PCKs from the server. The content manifest makes it possible later.
- A text syntax for scripts. The JSON AST is canonical; a text syntax that compiles to the same AST is planned after v1 (04-script.md).
- Saving a battle while it waits on a player's prompt answer (05-battle.md, section 11).
- Real account authentication. v1 has an offline mode where the username is the identity, behind an interface that a real login can replace.

## 4. Principles as rules

The five design philosophies, restated so a reviewer can check a change against them.

### P1. No global state
- No mutable static fields, no singletons, no service locators, no Godot autoloads.
- Allowed: `const`, `static readonly` fields holding immutable values (ids, codecs, empty instances), pure static functions, extension methods.
- Every stateful service is an instance reachable from a context object created by the program's entry point.
- Enforced by an architecture test that scans framework and vanilla assemblies for static fields that are writable or hold a mutable type (11-conventions.md, section 8).

### P2. Data-driven and moddable
- Anything a designer might want to change without code lives in content files with a schema.
- Anything a mod might want to extend is a registry that other mods can add to.
- The base game `vanilla` is loaded through the same mod loader, manifest, and entry points as a third-party mod.

### P3. No external libraries
- Runtime code depends only on the .NET base class library and Godot (client only).
- Allowed BCL namespaces include `System.Text.Json` (only the low-level `Utf8JsonReader`/`Utf8JsonWriter`), `System.Collections.Immutable`, `System.Runtime.Loader`, `System.Net.Sockets`.
- Test projects may use xUnit. Nothing else.
- Platform specifics (file system, Godot APIs) stay behind interfaces and live only in the implementation of those interfaces.

### P4. Simple design
- Validate hard at the trust boundaries: content load, network decode, player actions. Inside, trust invariants and let violations throw.
- No defensive chains of "if missing, log and return" in internal code.
- One way to do each thing. If two mechanisms overlap, delete one.
- Abstractions exist only where a mod or a platform needs to plug in.

### P5. Split design
- Framework, battle engine, server, and client are separate assemblies with references that only point "down" the graph in section 6.
- Code that both sides run lives in an explicit `Common` assembly per mod.
- The integrated host loads the server DLL at runtime; the client project never references it.

## 5. Decisions made

| Decision | Choice | Date |
|---|---|---|
| Name of the base game | `Vanilla` everywhere. Mod id `vanilla`, projects `CardWars.Vanilla.*` | 2026-11-24 |
| Source of truth | Server only. The client never evaluates rules; it renders the server's state and picks from the server's list of legal actions | 2026-11-24 |
| Branch | All rewrite work on branch `rewrite`. `main` and the legacy code are not modified | 2026-11-24 |
| Integrated host | The client loads `CardWars.Server` at runtime into its own load context | 2026-11-24 |
| Static rule | Constants, immutable `static readonly` values, and pure static helpers are allowed | 2026-11-24 |
| Target framework | `net10.0` (LTS). .NET 9 left support on 2026-11-10 ([Microsoft](https://devblogs.microsoft.com/dotnet/dotnet-8-9-end-of-support/)). Confirmed with a Godot spike before M6 (12-plan.md) | proposed |

## 6. Project graph

Arrows mean "references". Nothing references upward.

```
                     CardWars.Core
                    /   |    |    \
     CardWars.Script  Modding  Network  CardWars.Battle
                \       |       |      /
                 \      |       |     /
   CardWars.Vanilla.Common  (Core, Script, Battle, Network contracts)
          /                         \
CardWars.Server                CardWars.Client.Core
(Core, Modding, Network, Battle)  (Core, Modding, Network, Battle)
          |                             |
CardWars.Vanilla.Server         CardWars.Client (Godot)
          |                             |
CardWars.Server.Host (exe)      CardWars.Vanilla.Client (Godot class library)
```

Details:

| Project | Kind | References | Purpose |
|---|---|---|---|
| `CardWars.Core` | library | BCL | Ids, `DataTag`, codecs, formats, registries, logging, virtual file system, clock, random |
| `CardWars.Script` | library | Core | Script AST, compiler, interpreter |
| `CardWars.Modding` | library | Core | Manifests, discovery, dependency resolution, load contexts, registrars, content pipeline |
| `CardWars.Network` | library | Core | Connections, framing, protocol phases, packet registry |
| `CardWars.Battle` | library | Core, Script | Battle engine |
| `CardWars.Server` | library | Core, Modding, Network, Battle | Server host. No Godot. Loadable at runtime by the client |
| `CardWars.Server.Host` | console exe | Server | Dedicated server entry point |
| `CardWars.Client.Core` | library | Core, Modding, Network, Battle | Client logic without Godot: session, protocol, battle replica. Testable with `dotnet test` |
| `CardWars.Client` | Godot project | Client.Core, GodotSharp | Godot shell, screens, views, assets |
| `CardWars.Vanilla.Common` | mod library | Core, Script, Battle, Network | Vanilla content types, components, packets, codecs used on both sides |
| `CardWars.Vanilla.Server` | mod library | Vanilla.Common, Server | Vanilla rules, instances, server handlers |
| `CardWars.Vanilla.Client` | mod library (Godot) | Vanilla.Common, Client | Vanilla views, layouts, animations, UI |

`CardWars.Battle` referencing `CardWars.Script` is deliberate: behaviours and rules can be scripts, and the engine compiles them. The battle engine still knows nothing about mods, files, or the network.

Mod projects build into a mod package folder (`rewrite/mods/vanilla/...`), not into the client or server output. See 03-modding.md.

## 7. Repository layout on the `rewrite` branch

```
/                       legacy code, untouched (src/, gamedata/, CardWars.sln, ...)
/rewrite/
  CardWars.slnx
  Directory.Build.props   shared build settings (11-conventions.md)
  .editorconfig
  docs/                   these documents
  src/                    framework projects
  mods/vanilla/           vanilla mod: projects, content, client assets
  tests/                  one test project per framework project, plus end-to-end tests
  run/                    local data root for development (gitignored saves)
```

When the rewrite reaches parity, a single commit replaces the legacy tree with the contents of `rewrite/`. That is a decision for later and is not part of this plan.

## 8. Glossary

One term per concept. Code, docs, and conversation use these.

| Term | Meaning |
|---|---|
| **Id** | A namespaced identifier `namespace:path`, for example `vanilla:units/redalert3/apocalypse_tank` |
| **Mod** | A package with a manifest, optional code per side, and optional content. `vanilla` is a mod |
| **Side** | `common` (both), `server`, or `client` |
| **Mod environment** | Everything produced by loading a set of mods for one side: type tables, codecs, frozen registries. One per server, one per client session |
| **Registry** | A typed, freezable map from `Id` to values, owned by a mod environment |
| **Content** | Values loaded from mod files into registries (cards, attacks, worlds, rules) |
| **Definition** | An immutable content record, for example `CardDefinition` |
| **DataTag** | An immutable tree of plain values (null, bool, int, float, string, list, compound). The interchange format between files, the network, and typed objects |
| **Codec** | Converts between a C# type and a `DataTag`, with errors and a schema |
| **Component** | A typed piece of data attached to something extensible (a battle entity, a player profile) and identified by a component type `Id` |
| **Server** | One running server: its save, its mod environment, its players, its instances |
| **Account** | A persistent player identity (a `Guid`) |
| **Profile** | An account's persistent game data on one server |
| **Player** | A connected account on a server |
| **Instance** | A server-side space players are in: a world map, a battle, a dungeon |
| **Battle** | One running battle engine |
| **Entity** | A thing in a battle state: a card, a player seat, a deck, a slot |
| **Change** | One mutation of battle state (create entity, destroy entity, set component, remove component) |
| **Change set** | An ordered list of changes applied together, with an optional presentation cue |
| **Cue** | A presentation hint attached to a change set, for example "attack from A to B" |
| **Event** | Something that happened or is about to happen in a battle, which behaviours can react to |
| **Behaviour** | Code or script attached to an entity that reacts to events |
| **Player action** | A request from a battle participant, for example "play this card on that slot" |
| **Legal actions** | The set of player actions the server would accept right now, sent to the client |
| **Prompt** | The battle waiting for one participant to choose from given options mid-resolution |
| **Client session** | The client's state for one connection to one server |
| **View** | Client-side presentation of an instance or entity |

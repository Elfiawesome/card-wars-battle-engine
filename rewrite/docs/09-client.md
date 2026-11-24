# 09. Client

The client renders what the server says and sends what the player chooses. It never evaluates game rules.

## 1. Assemblies

| Assembly | Godot | Contents |
|---|---|---|
| `CardWars.Client.Core` | no | Client session, protocol phases, environment building from synced data, battle and world replicas, legal actions, pending-action tracking |
| `CardWars.Client` | yes (Godot project) | App root, screens, view registry, asset resolver and PCK mounting, Godot file system and log sink, integrated host loader, generic UI building blocks |
| `CardWars.Vanilla.Client` | yes (class library) | Vanilla views, layouts, animations, hand and attack controllers, inspection panels |

Everything in `Client.Core` runs under `dotnet test`, which is how the headless test client in 08-server.md works.

## 2. Startup

1. `Main.tscn` has one root node, `ClientApp`. There are no autoloads.
2. `ClientApp` creates the logger factory (Godot output sink plus a file sink in `user://logs`), the file system mounts, and reads `user://settings.json` (username, recent servers, graphics).
3. It discovers installed client mods (`<install>/mods/*`), resolves them, loads their client and common code into one non-collectible load context, and mounts their PCKs (section 5).
4. It shows the main menu: **Host**, **Join**, **Settings**. Host or join is the player's choice; command-line flags (`--join host:port`, `--host`, `--username name`) do the same for development.

Client mod code loads once per process. What is built per server is the **session environment** (section 3).

## 3. Client session

```csharp
public sealed class ClientSession : IDisposable
{
	public ConnectionPhase Phase { get; }
	public ModEnvironment? Environment { get; }      // built during configuration
	public InstanceSession? CurrentInstance { get; }

	public void Update();                            // called every frame by ClientApp: drains packets, advances phases
	public void Send(IPacket packet);
}
```

Phases follow 07-network.md, section 4:

- `configuration`: compare the server's mod manifest with installed mods; if something is missing, show a screen listing what to install and disconnect. Otherwise collect the synced registries, then build a session `ModEnvironment`: run each installed mod's common and client entry against a fresh registrar, fill synced registries from the server's data, run the client content pipeline (layouts, skins, cue timings), link, freeze. Errors are shown on screen, grouped by mod and file.
- `play`: route instance-scoped packets to `CurrentInstance`.

Disconnecting disposes the session and its environment and returns to the menu with the reason.

## 4. Instance views

When the server sends `EnterInstancePacket(type, key)`, the session creates the matching `InstanceSession` (logic, in `Client.Core`, registered by the mod for that instance type) and the screen asks the view registry for the instance's view (Godot).

```csharp
// Extension methods from CardWars.Client over the view registries its Godot module creates (03-modding.md, section 6)
registrar.InstanceView(VanillaIds.BattleInstance, context => new BattleScreen(context));
registrar.InstanceView(VanillaIds.WorldInstance, context => new WorldScreen(context));
registrar.EntityView(VanillaIds.Card, context => new CardView(context));
registrar.CueView(VanillaCues.Attack, context => new AttackAnimation(context));
```

`ViewContext` gives the session environment, the asset resolver, the logger, and the instance session. No view reaches for a global.

## 5. Assets and PCKs

### 5.1 Layout

Each client mod's `assets.pck` contains files under `res://mods/<mod id>/`. When the PCK is mounted, the client indexes that folder: `res://mods/vanilla/texture/cards/heroes/captain.png` is registered as asset id `vanilla:texture/cards/heroes/captain` of kind texture. Two files that map to the same id (different extensions) are a load error.

- PCKs are mounted with file replacement turned off, so one mod's pack cannot overwrite another mod's files.
- During development, vanilla's assets live inside the Godot project at `res://mods/vanilla/`, with the same paths. Release builds export them as `vanilla/client/assets.pck`. Code does not change between the two.

### 5.2 Resolution

```csharp
public sealed class AssetResolver
{
	public bool TryGet<T>(Id asset, [MaybeNullWhen(false)] out T resource) where T : Resource;
	public T GetOrPlaceholder<T>(Id asset) where T : Resource;    // logs once per missing id
}
```

Content that names assets (card textures, ability icons) is checked against the resolver when the session environment is built. Missing assets produce one warning per id, and views show a placeholder.

### 5.3 C# scripts in mod scenes

Godot binds a C# script referenced from a scene through the assemblies it has scanned for scripts. Scenes loaded from a mod PCK that reference C# classes in a mod DLL fail to bind in Godot 4 ([godot#76074](https://github.com/godotengine/godot/issues/76074)).

The view API works whichever way spike S1 (12-plan.md) turns out:

- **Plan A, if the spike finds a supported way to register a mod assembly's scripts with Godot before its PCK is mounted:** mod scenes may attach mod C# scripts directly.
- **Plan B, otherwise:** mod scenes contain no mod C# scripts (GDScript is allowed for purely visual logic). Views are C# classes created in code with `new`, which Godot handles fine, and they instance the scene as a child and bind nodes by unique name:

```csharp
public sealed partial class CardView : EntityView
{
	private readonly Label _name;

	public CardView(ViewContext context) : base(context)
	{
		var scene = context.Assets.Get<PackedScene>(VanillaAssets.CardScene).Instantiate();
		AddChild(scene);
		_name = scene.GetNode<Label>("%Name");
	}
}
```

Client core also provides a few generic C# node types (in the main assembly, which Godot always scans) that any mod scene may use, for example a node that marks a hit area for entity picking.

## 6. Battle view

The structure of the legacy battle view is kept; the responsibilities are separated.

| Part | Assembly | Responsibility |
|---|---|---|
| `BattleReplica` | Client.Core | Applies projected change sets to a local `BattleState` using the engine's change application. Raises "entity created / changed / destroyed" and "cue" notifications. Holds the latest legal actions and the pending action |
| `BattlePresenter` | Vanilla.Client | Turns notifications into a queue of view updates and animations. Keeps nodes of destroyed entities alive until animations that reference them finish (legacy "pending free" nodes) |
| Entity views | Vanilla.Client | One view per visible entity, created through the view registry |
| Layout | Vanilla.Client | Positions battlefields on screen from `battle_layouts` client content (legacy `DefaultLayout` and `LineLayout`) |
| Animation racks | Vanilla.Client | Parallel animation queues (legacy `BattleAnimationHandler`) driven by cue views |
| Controllers | Vanilla.Client | Hand, attack, and prompt controllers. Selection state is separate from presentation |
| Inspection panels | Vanilla.Client | Card details from synced content (ability types, attack definitions and their pattern grid) |

Controller rules:

- A controller offers only what is in the legal action set. A card is selectable if some play option names it; its targets are that option's targets. Attackers and their targets come from attack options.
- After sending an action, input is locked until the server answers with change sets and new legal actions, or with a rejection, which is shown to the player.
- There is no client-side prediction. Turn-based latency is acceptable, and prediction would need rules on the client.

The attack pattern grid in inspection panels is a pure function of an `AttackDefinition`, which is content, so it lives in `Vanilla.Common` and the client may call it. That is display of content, not rule evaluation.

## 7. Integrated host

1. The client install contains a `server/` folder with `CardWars.Server.dll` and its framework dependencies, and the server sides of installed mods.
2. **Host** creates an `EmbeddedServerLoader`, which loads `CardWars.Server.dll` into a new load context and creates its embedded server entry type. Framework assemblies resolve from the client's context, so the contract types are shared.
3. The contract lives in `CardWars.Network`, so the client never references `CardWars.Server` at compile time:

```csharp
public interface IEmbeddedServer : IDisposable
{
	EmbeddedServerStatus Status { get; }                 // Starting, Running(errors: none), Failed(errors)
	IConnection ConnectLocal();                          // a LocalConnection pair; the server end is added to its listener
	void OpenToNetwork(int port);                        // optional: let friends join over TCP
}
```

4. The server runs its own tick thread with its own mod environment and save under `user://saves/<name>/`. The client connects through `ConnectLocal()` and goes through the normal handshake, login, and content sync.
5. A pure client build ships without the `server/` folder; the Host button is hidden when the loader finds no server assembly.

## 8. Godot specifics

- `GodotFileSystem` implements `IFileSystem` over `res://` and `user://` using Godot's file APIs. It lives in `CardWars.Client`.
- `GodotLogSink` writes to Godot's output with colours per level.
- Nodes receive their context through constructors (code-created views) or a `Setup(context)` call from their parent. Nodes do not search the tree for services.
- The `MoreRichTextLabel` control (`src/CardWars.Client/scenes/core/ui/MoreRichTextLabel.cs` in legacy, 1,779 lines) reads like a drop-in file copied from elsewhere. It is kept in its own folder, excluded from style analyzers, and not rewritten.

## Open questions

1. Should the dedicated server and the client share one install of mods (the client install includes server sides for hosting), or should hosting require a separate server mod install? Proposed: one install.
2. Where does `MoreRichTextLabel` come from, and under what license? If you wrote it, it can move into `CardWars.Client` like any other code.

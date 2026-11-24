# 08. Server

`CardWars.Server` is a library. `CardWars.Server.Host` is the dedicated server executable that wraps it. The Godot client loads the same library at runtime to host a game (09-client.md, section 7). The library has no Godot dependency.

## 1. Structure

```csharp
public sealed class Server : IDisposable
{
	public static ServerStartResult Start(ServerConfig config, IFileSystem files, ILoggerFactory logging, IClock clock);

	public ModEnvironment Mods { get; }
	public PlayerManager Players { get; }
	public InstanceManager Instances { get; }
	public CommandDispatcher Commands { get; }
	public ServerSave Save { get; }

	public void AddListener(IListener listener);     // TCP, local
	public void RequestStop();
}
```

`Start` loads mods, builds the mod environment, opens the save, runs migrations, and starts the tick thread. If mods or content fail to load, it returns every error and starts nothing.

`ServerConfig` comes from `server.json` in the server root: listen address and port, mod folders, tick rate, autosave interval, authentication mode, default world.

## 2. Threading

- One **tick thread** runs all game logic: connections, players, instances, commands. Nothing else touches game state, so game code needs no locks.
- Network threads only read and write bytes into connection queues.
- The tick loop runs at a fixed rate (20 ticks per second by default). Battles are not tick-driven; they run when an action arrives. Worlds use the tick for movement.
- Instances only touch their own state and talk to the rest through the server's APIs on the same thread. That keeps the option of ticking instances in parallel later without changing instance code.

## 3. Players

### 3.1 Connection to player

```
connection accepted -> handshake -> login (IAuthenticator) -> profile loaded -> configuration (content sync) -> play -> placed in an instance
```

```csharp
public interface IAuthenticator
{
	AuthResult Authenticate(LoginRequestPacket request);   // Success(AccountId, DisplayName) | Failure(reason)
}
```

v1 ships `OfflineAuthenticator`: the username maps to a stable account id stored in the save (as legacy did). The rest of the server only sees `AccountId`.

A second login with an account that is already online disconnects the older connection.

### 3.2 Profiles

```csharp
public sealed class Profile
{
	public AccountId Account { get; }
	public string DisplayName { get; }
	public ComponentMap Components { get; }   // family "server:profile_component"
}
```

All game data about a player is components registered by mods: vanilla registers location (`vanilla:location` with world and position), decks, collection, quest progress later. Profiles are saved with the document header and migrations (01-data.md, section 8). Unknown components are preserved.

Profiles are loaded on login, saved on logout, on autosave, and on shutdown.

### 3.3 `Player`

The online wrapper: account, profile, connection, current instance, permission level. Mods attach transient per-session data through a typed key on `Player` (`PlayerData<T>`), which is not saved.

## 4. Instances

### 4.1 Types

```csharp
public interface IInstanceType
{
	Id Id { get; }
	InstancePersistence Persistence { get; }               // Persistent (worlds), Transient (battles)
	IInstance Create(InstanceContext context, InstanceKey key, DataTag? saved);
}

public interface IInstance
{
	InstanceKey Key { get; }                               // (type id, name), for example (vanilla:world, vanilla:city_square)
	void OnPlayerEnter(Player player, EnterReason reason);
	void OnPlayerLeave(Player player, LeaveReason reason);
	void HandlePacket(Player player, IPacket packet);      // only packets scoped to this instance type (07-network.md, 5.3)
	void Tick(TickInfo tick);
	DataTag? Save();                                       // persistent instances only
	void Dispose();
}
```

Mods register instance types in their server entry. Vanilla registers `vanilla:world` and `vanilla:battle`. Later: dungeons, events, roguelike runs.

### 4.2 Manager

- `Instances.GetOrCreate(key)` loads a persistent instance from the save or creates it.
- `Instances.Transfer(player, key, reason)` leaves the current instance, enters the target, and sends the client `EnterInstancePacket(type, key)`. A player is in at most one instance.
- Empty transient instances are disposed. Empty persistent instances are saved and unloaded after a grace period.
- An exception thrown out of an instance's methods is caught here (the only catch in the server besides the tick loop's outer guard): the instance is logged, its players are moved to their profile's fallback location, and it is disposed.

### 4.3 World instance (vanilla)

- Built from a world definition (`vanilla/server/content/vanilla/worlds/*.json`): name, map, spawn point, exits, encounters.
- Movement: the client sends `MovePacket(direction)`; the server integrates positions each tick and broadcasts positions of nearby players. v1 broadcasts to everyone in the world; interest management comes when worlds get large.
- Exits and encounters trigger transfers. The legacy debug warp key and debug battle key become server commands (section 6).

### 4.4 Battle instance (vanilla)

- Created by an encounter or a command with a `VanillaBattleSetup` built from the participating players' profiles.
- Owns a `Battle` and a `BattleReplicator` (05-battle.md, section 10).
- Maps each player to a seat. Spectators are viewers without a seat.
- On `BattleActionPacket(action)`: submits to the engine, sends `ActionRejectedPacket(reason)` to the sender on rejection, sends each viewer its projected change sets, and sends each seat its legal actions.
- On join or reconnect: sends a projected snapshot and the legal actions.
- On end: publishes a `BattleEnded` server hook with the outcome, so mods can grant rewards, then transfers players back to where they came from.

## 5. Hooks

Typed events that mods subscribe to during registration:

```csharp
registrar.Hooks.PlayerJoined.Subscribe((server, player) => ...);   // ServerRegistrar, 03-modding.md section 6
```

Initial set: `ServerStarted`, `ServerStopping`, `PlayerJoined`, `PlayerLeft`, `InstanceCreated`, `InstanceDisposed`, `PlayerEnteredInstance`, `PlayerLeftInstance`, `BattleEnded`. Hooks run on the tick thread in mod load order. This replaces the legacy list of nine `Action` properties on `Server`.

## 6. Commands

```csharp
registrar.Commands.Register(new Command(
	Name: "warp",
	Permission: PermissionLevel.Operator,
	Arguments: [Argument.Id("world")],
	Execute: (context, args) => ...));
```

- Entered by players through a chat packet with a `/` prefix, and by the dedicated server console.
- Arguments are typed and parsed by the dispatcher, with usage messages on errors.
- v1 commands: `warp`, `battle` (start a test battle against another player or a dummy), `kick`, `stop`, `save`, `mods`.

## 7. Save layout

```
<server root>/
  server.json
  mods/                  server mods
  save/
    level.json           save header: mod versions, account index (username -> account id in offline mode)
    profiles/<account>.json
    instances/<type namespace>/<type path>/<instance name>.json
```

All files are documents with the header from 01-data.md, section 8. Writes go to a temporary file and are renamed over the old one, so a crash during save cannot leave half a file.

## 8. Testing

- Server tests run with `MemoryFileSystem`, a fake clock, and `LocalConnection`s, so a full login and battle runs in a unit test.
- A headless test client (from `CardWars.Client.Core`) is used for end-to-end tests: two clients join, enter a battle, play cards, and the test checks each client's replica matches what it may see.

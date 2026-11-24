# 07. Network

`CardWars.Network` moves packets between a client and a server. It knows nothing about games, instances, or Godot.

## 1. Layers

```
packet object  <--PacketType codec-->  DataTag  <--binary format-->  frame payload  <--framing-->  transport
```

## 2. Transports

```csharp
public interface IConnection : IDisposable
{
	ConnectionState State { get; }                 // Open, Closing, Closed
	DisconnectReason? CloseReason { get; }
	void Send(ReadOnlyMemory<byte> frame);
	bool TryReceive(out ReadOnlyMemory<byte> frame);
	void Close(DisconnectReason reason);
}
```

| Transport | Use |
|---|---|
| `TcpConnection` | Remote play. A background reader thread fills a queue; sends are written under one lock |
| `LocalConnection` | Integrated host and tests. A pair of in-memory queues of **frames** (bytes) |

The legacy object-passing local connection is removed. Every packet is serialized, even locally, because the client and an integrated server use different copies of the common mod assemblies (03-modding.md, section 5.2), and because it keeps the code path identical to remote play. The cost is small at turn-based traffic levels.

Transports never call game code. The owner (server host or client session) drains `TryReceive` on its own thread.

## 3. Framing

```
frame := length:u32 little-endian, payload
payload := packet number:varint, body (binary DataTag, 01-data.md section 5.3)
```

- Maximum frame size: 1 MiB by default, configurable. A larger length closes the connection with `frame_too_large`.
- The binary format is read with network limits (depth 32, string and list caps), because the input is untrusted.
- Compression is not in v1. If content sync or snapshots get large, a flag bit in the length prefix can mark a compressed payload without changing anything above framing.

## 4. Protocol phases

Each connection is in one phase. Each phase has its own set of packet types per direction. A packet that does not belong to the current phase and direction closes the connection with `unexpected_packet`.

| Phase | Purpose | Ends when |
|---|---|---|
| `handshake` | Protocol version, what the client is | Server accepts or rejects |
| `login` | Authentication and identity | Server assigns the account |
| `configuration` | Mod manifest check, content sync, registry hash check | Client confirms its environment is built |
| `play` | The game | Disconnect, or the server sends the client back to `configuration` (content reload) |

This replaces the legacy split between "unauthenticated" and "authenticated" handler registries, and means a client cannot send game packets before it has logged in and synced.

### 4.1 Sequence

```
C -> S  Hello(protocol_version, client_name, client_version)
S -> C  HelloAccepted | Disconnect(reason)
C -> S  LoginRequest(username, credentials?)
S -> C  LoginSuccess(account_id, display_name) | Disconnect(reason)
S -> C  ModManifest(mods: [id, version, sides, content_hash, client_install_required])
C -> S  ManifestAccepted | Disconnect(missing mods)
S -> C  RegistryData(registry_id, entries) ...  (one per synced registry)
S -> C  ConfigurationDone(packet_table_hash)
C -> S  ClientReady(packet_table_hash)
--- play ---
```

`packet_table_hash` is a hash of the play-phase packet ids (section 5.2). A mismatch means the two sides were built from different common code, and the server closes the connection with a readable reason.

## 5. Packets

### 5.1 Declaring

```csharp
public interface IPacket;

public sealed class PacketType<T> where T : IPacket
{
	public Id Id { get; }
	public PacketDirection Direction { get; }      // Serverbound, Clientbound
	public ProtocolPhase Phase { get; }
	public Codec<T> Codec { get; }
}
```

Packets are immutable records named for what they carry and ending in `Packet`, for example `BattleChangeSetPacket`. Direction lives in the `PacketType`, not in a name prefix.

Framework phases (`handshake`, `login`, `configuration`) have packets defined in `CardWars.Network` and `CardWars.Server`/`Client.Core`. Play-phase packets are registered by mods through their common entry point, so both sides have the same types and codecs.

### 5.2 Packet numbers

The wire uses small integers, not ids. Per phase and direction, packet types are sorted by id (ordinal) and numbered from 0. Both sides compute the same table from the same set of packet types. The hash of this table is exchanged at the end of configuration.

### 5.3 Routing on the server (play phase)

Every play-phase serverbound packet type declares a **scope**:

- `Global`: handled by the server (chat, commands, settings).
- `Instance(instance type id)`: delivered only to the player's current instance if it is of that type, for example `vanilla:battle_action` to battle instances, `vanilla:move` to world instances.

A packet reaches exactly one handler. A packet for an instance type the player is not in is dropped and counted (not an error: it can race with an instance transfer). The legacy behaviour of sending every packet to both the instance and the global registry is removed.

### 5.4 Routing on the client

Clientbound play packets also declare a scope, so the client session can route instance packets to the current instance view, and drop ones that arrive after the view changed.

## 6. Disconnects and timeouts

- `DisconnectPacket(reason id, message)` in every phase, sent before closing.
- Keepalive: the server sends `KeepAlive(n)` every 10 seconds in `play`; the client echoes it. No echo for 30 seconds closes the connection. Login and configuration have a 30-second total timeout, as the legacy unauthenticated timeout did.
- Reasons are ids (`network:timeout`, `network:protocol_mismatch`, `server:missing_mods`, ...) so the client can show translated messages later.

## 7. Security

- The server treats every byte from a client as untrusted: limits on frames and decoding, strict codecs, packet phase checks, and action checks in the battle engine.
- The client also decodes server data strictly, because a malicious server must not be able to crash it or make it read files. Server data never names file paths; it names asset ids, which the client resolves only inside its mounted asset roots.

## 8. Testing

- Framing tests with partial reads and oversized frames.
- Phase tests: packets in the wrong phase or direction close the connection.
- An end-to-end handshake test over `LocalConnection` with a test mod set, and one over loopback TCP.

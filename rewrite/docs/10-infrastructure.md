# 10. Infrastructure

Small services in `CardWars.Core` that everything else uses. Each is an interface with plain implementations, owned by a context and passed down. None is static.

## 1. Virtual file system

### 1.1 `VirtualPath`

```csharp
public readonly record struct VirtualPath
{
	public string Root { get; }          // mount name, for example "mods", "save", "res", "user"
	public string Path { get; }          // "vanilla/common/content/vanilla/cards/units/x.json", always '/' separated, no "..", no leading '/'

	public static VirtualPath Parse(string text);   // "mods:/vanilla/mod.json"
	public VirtualPath Combine(string relative);
	public VirtualPath Parent { get; }
	public string Name { get; }
	public string NameWithoutExtension { get; }
	public string Extension { get; }
	public string RelativeTo(VirtualPath ancestor);
}
```

All path handling happens on this type, so no code outside a file system implementation uses `System.IO.Path`. `..` segments are rejected at parse time, which also prevents server data or content from escaping a root.

### 1.2 `IFileSystem`

```csharp
public interface IFileSystem
{
	bool FileExists(VirtualPath path);
	bool DirectoryExists(VirtualPath path);
	IEnumerable<VirtualPath> ListFiles(VirtualPath directory, bool recursive);   // ordinal order
	IEnumerable<VirtualPath> ListDirectories(VirtualPath directory);             // ordinal order
	Stream OpenRead(VirtualPath path);
	Stream OpenWrite(VirtualPath path);              // creates parent directories
	void Delete(VirtualPath path);
	void Move(VirtualPath from, VirtualPath to, bool overwrite);   // used for atomic saves
}
```

Six legacy members that were never used, the static property, and the path helpers are gone.

### 1.3 Implementations

| Type | Where | Notes |
|---|---|---|
| `MountedFileSystem` | Core | Maps root names to other file systems. Hosts build one at startup |
| `LocalFileSystem` | Core | A directory on disk |
| `MemoryFileSystem` | Core | For tests and for content received over the network |
| `GodotFileSystem` | Client | `res://` and `user://` through Godot's file APIs |

Listings are always in ordinal order, so content loading is deterministic across operating systems.

## 2. Logging

```csharp
public enum LogLevel { Trace, Debug, Info, Warn, Error }

public interface ILogger
{
	string Category { get; }                                        // "server", "server/battle/12", "client"
	bool IsEnabled(LogLevel level);
	void Log(LogLevel level, string message, Exception? exception = null);
	ILogger Child(string name);                                     // "server" -> "server/battle/12"
}

public interface ILoggerFactory
{
	ILogger Create(string category);
}

public interface ILogSink
{
	void Write(in LogRecord record);   // time, level, category, message, exception
}
```

- The entry point creates one `LoggerFactory` with its sinks: console for the dedicated server, Godot output for the client, a rolling file sink for both.
- An integrated host gives the embedded server its own factory, so server and client logs stay distinguishable (legacy had one static identity for both).
- Extension methods `Info`, `Warn`, and so on are thin wrappers, with `IsEnabled` checks so debug messages cost nothing when disabled.
- No logging in hot paths at `Info` or above. Per-packet and per-event logs are `Trace`.

## 3. Clock

```csharp
public interface IClock
{
	DateTimeOffset UtcNow { get; }
	long MonotonicTicks { get; }       // for tick loops and timeouts
}
```

`SystemClock` for real use, `ManualClock` for tests. The battle engine does not use a clock.

## 4. Random

```csharp
public sealed class DeterministicRandom
{
	public DeterministicRandom(ulong seed);
	public RandomState State { get; }               // serializable
	public static DeterministicRandom FromState(RandomState state);
	public int NextInt(int minInclusive, int maxExclusive);
	public double NextDouble();
	public void Shuffle<T>(Span<T> items);
}
```

- xoshiro256** seeded through SplitMix64. About 60 lines, written in-house, with published test vectors checked in tests.
- Used by battles (state includes it) and anything else that must replay.
- Non-replayed randomness (battle seeds, account ids) comes from a `System.Random` instance owned by the server.

## 5. Small shared types

- `EquatableArray<T>`: an immutable array with structural equality, for use inside component and content records (`ImmutableArray<T>` compares by reference, which would break state diffing and tests).
- `SemanticVersion` and `VersionRange` for manifests.
- `DataPath` (01-data.md).

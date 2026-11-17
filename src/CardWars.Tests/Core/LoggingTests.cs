using CardWars.BattleEngine.Behaviour.DataDriven.Instructions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Instructions;
using CardWars.BattleEngine.Vanilla.Features;
using CardWars.Core.Data;
using CardWars.Core.Data.Tags;
using CardWars.Core.Logging;
using CardWars.Core.Registry;
using CardWars.Core.Storage;
using CardWars.Server.Vanilla.Session;
using CardWars.Vanilla.Shared;
using CardWars.Tests.Support;
using Xunit;

namespace CardWars.Tests.Core;

/// <summary>
/// Phase 3 (doc.md §5, AGENT.md R9 + R1): logging is session-scoped via <see cref="ILogger"/> and
/// the logging path no longer touches the data layer. These tests fail if a mutable static logger
/// or the old Log->DataTagMapper coupling were reintroduced.
/// </summary>
public sealed class LoggingTests
{
	// A pure ILogger sink that records entries and never touches the console or the data layer.
	private sealed class RecordingLogger : ILogger
	{
		public List<LogEntry> Entries { get; } = [];
		public string Identity { get; set; } = "";
		public Action<LogEntry>? OnLog { get; set; }

		private void Emit(string msg, string level)
		{
			var entry = new LogEntry { Level = level, Message = msg };
			OnLog?.Invoke(entry);
			Entries.Add(entry);
		}

		public void Info(string msg) => Emit(msg, "INFO");
		public void Debug(string msg) => Emit(msg, "DEBUG");
		public void Warn(string msg) => Emit(msg, "WARN");
		public void Error(string msg) => Emit(msg, "ERROR");
		public void Custom(string msg, string level = "---", ConsoleColor color = ConsoleColor.Cyan) => Emit(msg, level);

		public string BuildLine(string msg, string level)
			=> Identity != "" ? $"[{Identity}][{level}]: {msg}" : $"[{level}]: {msg}";
	}

	// --- (1) Per-instance Identity/OnLog isolation (session scoping, R1) ---

	[Fact]
	public void TwoLoggerInstances_HaveIndependentIdentityAndOnLog()
	{
		var a = new Logger { Identity = "alice" };
		var b = new Logger { Identity = "bob" };

		// Identity is instance state, not shared static state.
		Assert.Equal("alice", a.Identity);
		Assert.Equal("bob", b.Identity);
		Assert.Equal("[alice][INFO]: hi", a.BuildLine("hi", "INFO"));
		Assert.Equal("[bob][INFO]: hi", b.BuildLine("hi", "INFO"));

		// OnLog is instance state: a's sink must never see b's entries.
		var seenByA = new List<string>();
		var seenByB = new List<string>();
		a.OnLog += e => seenByA.Add(e.Message);
		b.OnLog += e => seenByB.Add(e.Message);

		a.Info("from-a");
		b.Info("from-b");

		Assert.Equal(["from-a"], seenByA);
		Assert.Equal(["from-b"], seenByB);
	}

	// --- (2) The logging path does not depend on the data layer (R9) ---

	[Fact]
	public void Logging_DoesNotTouchDataContext()
	{
		// A fresh DataContext that knows NOTHING. If the logging layer tried to serialize through
		// the data layer, logging arbitrary strings/values would require type registration; it
		// must not. The logger only accepts already-formatted strings.
		var emptyData = new DataContext();
		var logger = new RecordingLogger();

		logger.Info("a plain message");
		logger.Warn(42.ToString());
		logger.Error(new { Unregistered = true }.ToString() ?? "");

		Assert.Equal(3, logger.Entries.Count);
		// emptyData is untouched: it never had a type registered, proving logging did not serialize
		// anything through it.
		Assert.Null(emptyData.GetTypeId(typeof(LoggingTests)));
	}

	[Fact]
	public void LogInstruction_FormatsThroughScriptDataContext_NotTheLogger()
	{
		var registry = TestSupport.CreateRegistry();
		var state = new GameState();
		var recording = new RecordingLogger();
		var runtime = TestSupport.CreateRuntime(registry, state, EntityId.None, recording);

		// The instruction evaluates to a DataTag. The readable output must be produced at the call
		// site using the script's own DataContext (runtime.Data), exactly as the old static
		// Log.ToReadable did, while the logger itself only ever receives a string.
		var handler = new LogInstructionHandler();
		var instruction = new LogInstruction(new LiteralExpression(new IntTag(7)));
		foreach (var _ in handler.Handle(runtime, instruction)) { }

		var entry = Assert.Single(recording.Entries);
		Assert.Equal("INFO", entry.Level);
		Assert.Equal("7", entry.Message);
		// The entry carries no serialized Data object: the data-layer formatting happened before
		// the string reached the logger (R9).
		Assert.Null(entry.Data);

		// A compound value serializes to the same JSON the script's own DataContext produces.
		var compound = new CompoundTag().Set("k", new IntTag(1));
		foreach (var _ in handler.Handle(runtime, new LogInstruction(new LiteralExpression(compound)))) { }
		Assert.Equal(runtime.Data.Serialize(compound), recording.Entries[^1].Message);
	}

	// --- (3) The engine's OWNED logger receives entries during a battle action (not a global) ---

	[Fact]
	public void OwnedEngineLogger_ReceivesEntriesDuringBattleAction()
	{
		var registry = TestSupport.CreateRegistry();
		TestCards.RegisterStarterDeck(registry, null);

		var logger = new Logger();
		var received = new List<LogEntry>();
		logger.OnLog += received.Add;

		// The engine owns this logger and threads it through Transaction -> block application.
		var engine = new BattleEngine.BattleEngine { Registry = registry, Logger = logger };

		engine.HandleInput(EntityId.None, new PlayerJoinedRequestInput(EntityId.New(), 1));

		// Transaction.ApplyBlockBatch logs a DEBUG line per block; a player-join produces several.
		Assert.Contains(received, e => e.Level == "DEBUG" && e.Message.StartsWith("Executing Block"));

		// A second engine with its OWN logger must not see the first engine's entries.
		var otherLogger = new Logger();
		var otherReceived = new List<LogEntry>();
		otherLogger.OnLog += otherReceived.Add;
		var otherEngine = new BattleEngine.BattleEngine { Registry = registry, Logger = otherLogger };

		var before = received.Count;
		otherEngine.HandleInput(EntityId.None, new PlayerJoinedRequestInput(EntityId.New(), 1));

		Assert.NotEmpty(otherReceived);
		Assert.Equal(before, received.Count); // first engine's sink stayed silent
	}

	// --- (4) The SERVER threads its owned logger into the engine and the dispatch registries ---
	// Guards the regression the reviewer found: a running server built engines (via
	// BattleInstanceProvider) and handler registries (ServerRegistry/BattleEngineRegistry/
	// DataDrivenRegistry) without wiring server.Logger, so engine/dispatch diagnostics that the old
	// static Log always printed silently routed to NullLogger. A direct-engine test did not cover
	// the server path, so the regression passed CI.

	[Fact]
	public void Server_ThreadsOwnedLogger_IntoEngineAndDispatchRegistries()
	{
		var recording = new RecordingLogger();
		var storage = new StorageManager("data", new MemoryFileProvider(), recording);
		var server = new CardWars.Server.Server(storage, "test", recording);

		// The server owns the recording logger and injected it into the registries it owns.
		Assert.Same(recording, server.Logger);

		// Load the vanilla battle content into the server's SHARED registry (the one battle engines
		// are built from) and the server-side vanilla mod (which registers the BattleInstanceProvider
		// wired with server.Logger).
		server.LoadMod(new BattleEngine.Vanilla.VanillaMod(), []);
		TestCards.RegisterStarterDeck(server.SharedBattleEngineRegistry, null);
		server.LoadMod(new Server.Vanilla.VanillaMod(), []);

		// Create a battle instance through the real provider path and drive a server-side action.
		var instance = (BattleInstance)server.CreateInstance(SharedIds.Instances.Battle, Guid.NewGuid().ToString());
		Assert.NotNull(instance.Engine);
		// The provider wired the engine to the server's logger (not NullLogger).
		Assert.Same(recording, instance.Engine!.Logger);

		recording.Entries.Clear();
		instance.Engine!.HandleInput(EntityId.None, new PlayerJoinedRequestInput(EntityId.New(), 1));

		// Engine path: Transaction.ApplyBlockBatch logs a DEBUG "Executing Block" line, now reaching
		// the server's sink instead of NullLogger.
		Assert.Contains(recording.Entries, e => e.Level == "DEBUG" && e.Message.StartsWith("Executing Block"));

		// Dispatch path: BattleEngineRegistry's HandlerRegistry logs "Running handler ..." for the
		// input it dispatches, confirming the registries carry server.Logger rather than NullLogger.
		Assert.Contains(recording.Entries, e => e.Level == "DEBUG" && e.Message.StartsWith("Running handler"));
	}
}

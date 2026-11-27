using CardWars.Client.scripts.core.application;
using CardWars.Client.Runtime.Application;
using CardWars.Client.scenes.core.global;
using CardWars.Core.Storage;
using Godot;

namespace CardWars.Client.scenes.core.game_session;

public partial class GameSession : Node
{
	public SessionRuntime? Session { get; private set; }
	private GodotLogSink? _logSink;

	public string ConnectingUsername = "";
	public string TargetIpAddress = "127.0.0.1";
	public int TargetPort = 5060;

	public override void _Ready()
	{
		// Set username from cmd line args
		var args = OS.GetCmdlineArgs();
		ConnectingUsername = args.Length > 2 ? args[2] : "Elfiawesome";
		TargetIpAddress = args.Length > 3 ? args[3] : TargetIpAddress;
		TargetPort = args.Length > 4 ? args[4].ToInt() : TargetPort;
		GetWindow().Title = ConnectingUsername;

		// Setup storage & providers
		var provider = new LocalFileProvider();
		var clientDir = System.Environment.CurrentDirectory;
		var projectRoot = provider.GetFullPath(provider.Combine(provider.Combine(clientDir, ".."), ".."));
		var gamedataPath = provider.Combine(projectRoot, "gamedata");
		
		// Setup Logger Node (NOT ILogSink, tahts in app btw...)
		_logSink = new GodotLogSink();
		AddChild(_logSink);
		
		var app = new GameApplication(provider, gamedataPath, _logSink);

		var mode = ConnectingUsername == "Elfiawesome"
			? SessionMode.Integrated
			: SessionMode.Client;

		// Set up session!!!
		Session = app.CreateSession(
			this,
			ConnectingUsername,
			mode,
			"session_1",
			TargetIpAddress,
			TargetPort);
		Session.OnDebugStatus = SetDebugStatus;
		Session.OnDebugWorld = SetDebugWorld;
		Session.OnDebugPlayers = SetDebugPlayers;
		Session.Start();
	}

	public override void _Process(double delta) => Session?.Process();

	private void SetDebugStatus(string value) => GetNode<Node>("Control/VBoxContainer/Status").Set("content", value);
	private void SetDebugWorld(string value) => GetNode<Node>("Control/VBoxContainer/World").Set("content", value);
	private void SetDebugPlayers(string value) => GetNode<Node>("Control/VBoxContainer/Players").Set("content", value);

	public override void _ExitTree() => ExitCleanup();

	public override void _Notification(int what)
	{
		if (what == NotificationWMCloseRequest) { ExitCleanup(); }
	}

	private void ExitCleanup()
	{
		GD.Print("Cleaning up");
		Session?.Stop();
		_logSink?.Flush();
		GetTree().Quit();
	}
}

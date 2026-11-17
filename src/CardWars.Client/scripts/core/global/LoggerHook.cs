using CardWars.Core.Logging;
using Godot;

namespace CardWars.Client.scripts.core.global;

// Phase 3 (R9/R1): logging is session-scoped. This autoload no longer subscribes to the former
// static Log.OnLog; instead a session hands it its owned ILogger via Attach, and this hook renders
// that logger's entries to the Godot console using the same per-level colouring as before.
// NOTE: CardWars.Client targets Godot 4.7, which is not installed in the engine CI sandbox, so this
// file is NOT compiled/verified there; the edit is kept minimal and consistent with the engine API.
public partial class LoggerHook : Node
{
	/// <summary>
	/// Subscribe the Godot console sink to a session-owned logger. Called by the session that owns
	/// the logger (e.g. GameSession) once it has created it.
	/// </summary>
	public void Attach(ILogger logger)
	{
		logger.OnLog += (logEntry) =>
		{
			string newMsg = logger.BuildLine(logEntry.Message, logEntry.Level);
			static void printColor(string color, string m) => GD.PrintRich($"[color={color}]" + m + "[/color]");
			switch (logEntry.Level)
			{
				case "INFO": printColor("white", newMsg); break;
				case "DEBUG": printColor("grey", newMsg); break;
				case "WARN": printColor("yellow", newMsg); break;
				case "ERROR": GD.PrintErr(newMsg); break;
			}
		};
	}
}

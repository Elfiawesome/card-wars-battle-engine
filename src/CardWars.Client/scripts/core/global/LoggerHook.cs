using Godot;

namespace CardWars.Client.scripts.core.global;

public partial class LoggerHook : Node
{
	public override void _Ready()
	{
		Core.Logging.Log.OnLog += (logEntry) =>
		{

			string newMsg = Core.Logging.Log.BuildLine(logEntry.Message, logEntry.Level);
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
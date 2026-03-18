using Godot;

namespace CardWars.Client;

public partial class LoggerHook : Node
{
	public override void _Ready()
	{
		Core.Logging.Logger.OnLog += (_, level, msg) =>
		{
			string newMsg = $"[{level}]: {msg}";
			static void printColor(string color, string m) => GD.PrintRich($"[color={color}]" + m + "[/color]");
			switch (level)
			{
				case "INFO": printColor("white", newMsg); break;
				case "DEBUG": printColor("grey", newMsg); break;
				case "WARN": printColor("yellow", newMsg); break;
				case "ERROR": GD.PrintErr(newMsg); break;
			}
		};
	}
}
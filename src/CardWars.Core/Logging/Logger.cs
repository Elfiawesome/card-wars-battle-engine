namespace CardWars.Core.Logging;

public static class Logger
{
	public static Action<ConsoleColor, string, string>? OnLog { get; set; }
	public static void Info(string msg) => Print(msg, "INFO", ConsoleColor.White);

	public static void Debug(string msg) => Print(msg, "DEBUG", ConsoleColor.DarkGray);

	public static void Warn(string msg) => Print(msg, "WARN", ConsoleColor.Yellow);

	public static void Error(string msg) => Print(msg, "ERROR", ConsoleColor.Red);

	public static void Custom(string msg, string level = "---", ConsoleColor color = ConsoleColor.Cyan) => Print(msg, level, color);

	private static void Print(string msg, string level, ConsoleColor color = ConsoleColor.White)
	{
		Console.ForegroundColor = color;
		Console.WriteLine($"[{level}]: {msg}");
		Console.ResetColor();
		OnLog?.Invoke(color, level, msg);
	}
}
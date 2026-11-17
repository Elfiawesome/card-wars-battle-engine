namespace CardWars.Core.Logging;

/// <summary>
/// The real session/server-owned logger. Reproduces the behaviour the former static
/// <c>Log.Print</c> had: fire <see cref="OnLog"/> with a <see cref="LogEntry"/>, then write the
/// colored line to the console. <see cref="Identity"/> and <see cref="OnLog"/> are instance state
/// (R1), and nothing here touches the data layer (R9).
/// </summary>
public sealed class Logger : ILogger
{
	public string Identity { get; set; } = "";
	public Action<LogEntry>? OnLog { get; set; }

	public void Info(string msg) => Print(msg, "INFO", ConsoleColor.White);
	public void Debug(string msg) => Print(msg, "DEBUG", ConsoleColor.DarkGray);
	public void Warn(string msg) => Print(msg, "WARN", ConsoleColor.Yellow);
	public void Error(string msg) => Print(msg, "ERROR", ConsoleColor.Red);
	public void Custom(string msg, string level = "---", ConsoleColor color = ConsoleColor.Cyan) => Print(msg, level, color);

	private void Print(string msg, string level, ConsoleColor color)
	{
		var entry = new LogEntry { Level = level, Message = msg };
		OnLog?.Invoke(entry);
		Console.ForegroundColor = color;
		Console.WriteLine(BuildLine(msg, level));
		Console.ResetColor();
	}

	public string BuildLine(string msg, string level)
	{
		string line = "";
		if (Identity != "") line += $"[{Identity}]";
		line += $"[{level}]";
		line += $": {msg}";
		return line;
	}
}

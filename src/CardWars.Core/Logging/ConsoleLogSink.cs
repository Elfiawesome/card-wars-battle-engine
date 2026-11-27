namespace CardWars.Core.Logging;

public sealed class ConsoleLogSink : ILogSink
{
	private readonly object _sync = new();

	public void Write(LogEntry entry)
	{
		lock (_sync)
		{
			Console.ForegroundColor = Color(entry.Level);
			Console.WriteLine(Format(entry));
			Console.ResetColor();
		}
	}

	public static string Format(LogEntry entry)
	{
		var identity = entry.Identity is { Length: > 0 } id ? $"[{id}]" : "";
		return $"{identity}[{entry.Level.ToString().ToUpperInvariant()}]: {entry.Message}";
	}

	private static ConsoleColor Color(LogLevel level) => level switch
	{
		LogLevel.Debug => ConsoleColor.DarkGray,
		LogLevel.Info => ConsoleColor.White,
		LogLevel.Warn => ConsoleColor.Yellow,
		LogLevel.Error => ConsoleColor.Red,
		_ => ConsoleColor.White
	};
}

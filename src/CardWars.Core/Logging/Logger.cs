using CardWars.Core.Data;

namespace CardWars.Core.Logging;

public static class Log
{
	public static Action<LogEntry>? OnLog { get; set; }

	public static void Info(object msg) => Info(ToReadable(msg));
	public static void Info(string msg) => Print(msg, "INFO", ConsoleColor.White);

	public static void Debug(object msg) => Debug(ToReadable(msg));
	public static void Debug(string msg) => Print(msg, "DEBUG", ConsoleColor.DarkGray);

	public static void Warn(object msg) => Warn(ToReadable(msg));
	public static void Warn(string msg) => Print(msg, "WARN", ConsoleColor.Yellow);

	public static void Error(object msg) => Error(ToReadable(msg));
	public static void Error(string msg) => Print(msg, "ERROR", ConsoleColor.Red);

	public static void Custom(object msg) => Custom(ToReadable(msg));
	public static void Custom(string msg, string level = "---", ConsoleColor color = ConsoleColor.Cyan) => Print(msg, level, color);

	public static string Identity { get; set; } = "";

	private static void Print(string msg, string level, object? data = null, ConsoleColor color = ConsoleColor.White)
	{
		var entry = new LogEntry { Level = level, Message = msg, Data = data };
		OnLog?.Invoke(entry);
		Console.ForegroundColor = color;
		Console.WriteLine(BuildLine(msg, level));
		Console.ResetColor();
	}

	public static string BuildLine(string msg, string level)
	{
		string line = "";
		if (Identity != "") line += $"[{Identity}]";
		line += $"[{level}]";
		line += $": {msg}";
		return line;
	}

	public static string ToReadable(object? obj)
	{
		if (obj == null) return "null";

		if (obj is string s) return s;
		if (obj is int || obj is float || obj is bool || obj is Guid)
			return obj.ToString()!;

		if (obj is System.Collections.IEnumerable enumerable)
			return "[" + string.Join(", ", enumerable.Cast<object>().Select(ToReadable)) + "]";

		if (obj is DataTag tag)
			return DataTagSerializer.Serialize(tag);  // compact JSON

		var compound = DataTagMapper.ToTag(obj, includeType: true);
		return DataTagSerializer.Serialize(compound);
	}
}

public class LogEntry
{
	public string Level { get; set; } = "";
	public string Message { get; set; } = "";
	public object? Data { get; set; } = null;
}
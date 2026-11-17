namespace CardWars.Core.Logging;

/// <summary>
/// A no-op <see cref="ILogger"/>. Used as the default fallback wherever no owner-provided logger
/// exists yet. The shared <see cref="Instance"/> is immutable no-op state, so it satisfies R1:
/// there is nothing a session or mod can mutate through it (the <see cref="Identity"/> setter and
/// <see cref="OnLog"/> are ignored). This replaces the old mutable static <c>Log</c> singleton.
/// </summary>
public sealed class NullLogger : ILogger
{
	public static readonly ILogger Instance = new NullLogger();

	public string Identity { get => ""; set { } }
	public Action<LogEntry>? OnLog { get => null; set { } }

	public void Info(string msg) { }
	public void Debug(string msg) { }
	public void Warn(string msg) { }
	public void Error(string msg) { }
	public void Custom(string msg, string level = "---", ConsoleColor color = ConsoleColor.Cyan) { }

	public string BuildLine(string msg, string level)
	{
		string line = "";
		if (Identity != "") line += $"[{Identity}]";
		line += $"[{level}]";
		line += $": {msg}";
		return line;
	}
}

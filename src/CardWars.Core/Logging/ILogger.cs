namespace CardWars.Core.Logging;

/// <summary>
/// Session/server-owned logging sink (R9). An <see cref="ILogger"/> never references the data
/// layer (the tag mapper/serializer/tag types); callers format values to a string before logging.
/// <see cref="Identity"/> and <see cref="OnLog"/> are instance state owned by the
/// session/server/engine, never process-global static state (R1).
/// </summary>
public interface ILogger
{
	void Info(string msg);
	void Debug(string msg);
	void Warn(string msg);
	void Error(string msg);
	void Custom(string msg, string level = "---", ConsoleColor color = ConsoleColor.Cyan);

	/// <summary>Instance-scoped identity prefix (e.g. a session/connection name).</summary>
	string Identity { get; set; }

	/// <summary>Instance-scoped sink fired for every entry this logger emits.</summary>
	Action<LogEntry>? OnLog { get; set; }

	/// <summary>Formats a single display line from a message and level, honouring <see cref="Identity"/>.</summary>
	string BuildLine(string msg, string level);
}

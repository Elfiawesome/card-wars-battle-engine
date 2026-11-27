using System.Collections;

namespace CardWars.Core.Logging;

public sealed class Logger(ILogSink sink, string? identity = null)
{
	private readonly ILogSink _sink = sink;

	public string? Identity { get; } = identity;

	public Logger ForIdentity(string identity) => new(_sink, identity);

	public void Debug(object? message) => Write(LogLevel.Debug, message);

	public void Info(object? message) => Write(LogLevel.Info, message);

	public void Warn(object? message) => Write(LogLevel.Warn, message);

	public void Error(object? message) => Write(LogLevel.Error, message);

	private void Write(LogLevel level, object? message)
		=> _sink.Write(new LogEntry(level, Format(message), Identity));

	private static string Format(object? value)
	{
		if (value is null) return "null";
		if (value is string text) return text;
		if (value is int or float or double or long or bool or Guid)
			return value.ToString() ?? "";

		if (value is IEnumerable enumerable)
			return "[" + string.Join(", ", enumerable.Cast<object?>().Select(Format)) + "]";

		return value.ToString() ?? "<NULL INSTANCE>";
	}
}

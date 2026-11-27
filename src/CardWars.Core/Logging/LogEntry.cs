namespace CardWars.Core.Logging;

public readonly record struct LogEntry(
	LogLevel Level,
	string Message,
	string? Identity = null
);

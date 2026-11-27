namespace CardWars.Core.Logging;

public sealed class NullLogSink : ILogSink
{
	public void Write(LogEntry entry) { }
}

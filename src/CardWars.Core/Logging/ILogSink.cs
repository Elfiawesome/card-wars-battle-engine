namespace CardWars.Core.Logging;

public interface ILogSink
{
	void Write(LogEntry entry);
}

namespace CardWars.Core.Logging;

public class LogEntry
{
	public string Level { get; set; } = "";
	public string Message { get; set; } = "";
	public object? Data { get; set; } = null;
}

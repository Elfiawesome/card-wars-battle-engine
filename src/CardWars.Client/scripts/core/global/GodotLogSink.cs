using System.Collections.Concurrent;
using CardWars.Core.Logging;
using Godot;

namespace CardWars.Client.scripts.core.global;

public partial class GodotLogSink : Node, ILogSink
{
	private readonly ConcurrentQueue<LogEntry> _entries = new();

	public void Write(LogEntry entry) => _entries.Enqueue(entry);

	public override void _Process(double delta) => Flush();

	public void Flush() => Drain();

	public override void _ExitTree() => Drain();

	private void Drain()
	{
		while (_entries.TryDequeue(out var entry))
		{
			var line = ConsoleLogSink.Format(entry);
			switch (entry.Level)
			{
				case LogLevel.Info:
					GD.PrintRich($"[color=white]{line}[/color]");
					break;
				case LogLevel.Debug:
					GD.PrintRich($"[color=gray]{line}[/color]");
					break;
				case LogLevel.Warn:
					GD.PrintRich($"[color=yellow]{line}[/color]");
					break;
				case LogLevel.Error:
					GD.PrintErr(line);
					break;
			}
		}
	}
}

namespace CardWars.BattleEngine.State;

public record struct StatLayerId(Guid Id) : IStateId;


public struct StatLayer()
{
	public StatLayerId Id { get; set; } = new(Guid.NewGuid());
	public string Name = "";
	public int Value { get; set; } = 0;
	public int MaxValue { get; set; } = 0;
}

public class CompositeIntStat
{
	public List<StatLayer> Layers { get; set; } = [];

	public int TotalValue => Layers.Sum(l => l.Value);
	public int TotalMax => Layers.Sum(l => l.MaxValue);
}
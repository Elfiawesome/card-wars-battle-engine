using CardWars.BattleEngine.State.Entity;
using CardWars.Core.Common.Mapping;

// TODO: needs a better name... like data structure? idk?? 
namespace CardWars.BattleEngine.State.Component;

public record struct StatLayerId(Guid Id) : IStateId;

public struct StatLayer()
{
	public StatLayerId Id { get; set; } = new(Guid.NewGuid());
	public string Name { get; set; } = "";
	public int Value { get; set; } = 0;
	public int MaxValue { get; set; } = 0;
}

public class CompositeIntStat
{
	public List<StatLayer> Layers { get; set; } = [];

	public int TotalValue => Layers.Sum(l => l.Value);
	public int TotalMax => Layers.Sum(l => l.MaxValue);

	public void SetLayer(int index, StatLayer stat) { Layers[index] = stat; }
	public void SetLayer(StatLayer stat)
	{
		var index = Layers.FindIndex((s) => s.Id == stat.Id);
		if (index != -1) { Layers[index] = stat; }
	}
	public void AddLayer(StatLayer stat) { Layers.Add(stat); }
}
namespace CardWars.BattleEngine.State;

public class UnitCard(UnitCardId id) : Card<UnitCardId>(id)
{
	public string Name { get; set; } = "";
	
	[EntityStatMapping("hp")]
	public CompositeIntStat Hp { get; set; } = new();
	[EntityStatMapping("atk")]
	public CompositeIntStat Atk { get; set; } = new();
	[EntityStatMapping("pt")]
	public CompositeIntStat Pt { get; set; } = new();
}

public record struct UnitCardId(Guid Id) : ICardId;
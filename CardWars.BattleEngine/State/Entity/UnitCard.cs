using CardWars.BattleEngine.State.Component;
using CardWars.Core.Common.Mapping;

namespace CardWars.BattleEngine.State.Entity;

public class UnitCard(UnitCardId id) : Card<UnitCardId>(id)
{
	[PropertyMapping]
	public string Name { get; set; } = "";
	[PropertyMapping]
	public CompositeIntStat Hp { get; set; } = new();
	[PropertyMapping]
	public CompositeIntStat Atk { get; set; } = new();
	[PropertyMapping]
	public CompositeIntStat Pt { get; set; } = new();
	[PropertyMapping]
	public CompositeIntStat Charge { get; set; } = new();


}

public record struct UnitCardId(Guid Id) : ICardId;
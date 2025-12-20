using CardWars.BattleEngine.State.Component;

namespace CardWars.BattleEngine.State.Entity;

public class UnitCard(UnitCardId id) : Card<UnitCardId>(id)
{
	public string Name = "";
	public CompositeIntStat Hp = new();
	public CompositeIntStat Atk = new();
	public CompositeIntStat Pt = new();
	public CompositeIntStat Charge = new();


}

public record struct UnitCardId(Guid Id) : ICardId;
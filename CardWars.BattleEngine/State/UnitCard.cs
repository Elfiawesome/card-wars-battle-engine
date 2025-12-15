namespace CardWars.BattleEngine.State;

public class UnitCard(UnitCardId id) : EntityState<UnitCardId>(id)
{
	public string Name { get; set; } = "";
	public int Hp { get; set; } = 0;
	public int Atk { get; set; } = 0;
}

public record struct UnitCardId(Guid Id) : EntityId;
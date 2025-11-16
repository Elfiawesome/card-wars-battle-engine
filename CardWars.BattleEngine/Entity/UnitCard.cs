namespace CardWars.BattleEngine.Entity;

public class UnitCard(EntityService service, UnitCardId id) : Card<UnitCardId>(service, id)
{
	public string Name { get; set; } = "Default Name";
	public string FlavourText { get; set; } = "Default Description";
	public int PointCost { get; set; } = 0;
	public int Hp { get; set; } = 0;
	public int Atk { get; set; } = 0;
	public UnitSlotId OwnerUnitSlotId { get; set; }
	public HashSet<AbilityId> AbilitIds { get; set; } = [];
}

public record struct UnitCardId(Guid Id);
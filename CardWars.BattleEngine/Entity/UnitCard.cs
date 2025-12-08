namespace CardWars.BattleEngine.Entity;

public class UnitCard(UnitCardId id) : Card<UnitCardId>(id)
{
	public string Name { get; set; } = "Default Name";
	public string FlavourText { get; set; } = "Default Description";
	public int PointCost { get; set; } = 0;
	public int Hp { get; set; } = 0;
	public int Atk { get; set; } = 0;
	public UnitSlotId OwnerUnitSlotId { get; set; }
	public PlayerId OwnerPlayerId { get; set; }
	public bool IsPlayed { get; set; } = false;
	public HashSet<AbilityId> AbilitIds { get; set; } = [];
}

public record struct UnitCardId(Guid Id);
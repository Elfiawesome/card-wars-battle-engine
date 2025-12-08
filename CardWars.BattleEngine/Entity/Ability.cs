namespace CardWars.BattleEngine.Entity;

public class Ability(AbilityId id) : Entity<AbilityId>(id)
{
	public UnitCardId OwnerCardId { get; set; }
}

public record struct AbilityId(Guid Id);
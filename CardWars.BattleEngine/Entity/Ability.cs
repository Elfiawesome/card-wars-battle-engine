namespace CardWars.BattleEngine.Entity;

public class Ability(EntityService service, AbilityId id) : Entity<AbilityId>(service, id)
{
	public UnitCardId OwnerCardId { get; set; }
}

public record struct AbilityId(Guid Id);
namespace CardWars.BattleEngine.State;

public class Ability(AbilityId id) : EntityState<AbilityId>(id)
{
}

public record struct AbilityId(Guid Id) : EntityId;
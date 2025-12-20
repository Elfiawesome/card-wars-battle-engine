namespace CardWars.BattleEngine.State.Entity;

public class Ability(AbilityId id) : EntityState<AbilityId>(id)
{
}

public record struct AbilityId(Guid Id) : IStateId;
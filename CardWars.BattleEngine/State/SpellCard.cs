namespace CardWars.BattleEngine.State;

public class SpellCard(SpellCardId id) : EntityState<SpellCardId>(id)
{
}

public record struct SpellCardId(Guid Id) : EntityId;
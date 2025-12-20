namespace CardWars.BattleEngine.State.Entity;

public class SpellCard(SpellCardId id) : Card<SpellCardId>(id)
{
}

public record struct SpellCardId(Guid Id) : ICardId;
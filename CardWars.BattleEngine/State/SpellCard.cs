namespace CardWars.BattleEngine.State;

public class SpellCard(SpellCardId id) : Card<SpellCardId>(id)
{
}

public record struct SpellCardId(Guid Id) : ICardId;
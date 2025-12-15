namespace CardWars.BattleEngine.State;

public class Card<TCardId>(TCardId id) : EntityState<TCardId>(id)
	where TCardId : ICardId
{
	
}

public interface ICardId : EntityId;
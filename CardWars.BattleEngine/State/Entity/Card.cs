namespace CardWars.BattleEngine.State.Entity;

public class Card<TCardId>(TCardId id) : EntityState<TCardId>(id)
	where TCardId : ICardId
{
	
}

public interface ICardId : IStateId;
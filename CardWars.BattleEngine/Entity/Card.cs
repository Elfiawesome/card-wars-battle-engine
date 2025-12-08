namespace CardWars.BattleEngine.Entity;

public abstract class Card<T>(T id) : Entity<T>(id)
	where T : struct
{
	
}
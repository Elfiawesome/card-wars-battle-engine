namespace CardWars.BattleEngine.Entity;

public abstract class Card<T>(EntityService service, T id) : Entity<T>(service, id)
	where T : struct
{
	
}
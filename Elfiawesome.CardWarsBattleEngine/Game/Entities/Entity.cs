namespace Elfiawesome.CardWarsBattleEngine.Game.Entities;

public class Entity<T>
{
	public readonly T Id;
	public Entity(T id)
	{
		Id = id;
	}
}
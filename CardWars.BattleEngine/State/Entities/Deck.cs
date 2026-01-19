namespace CardWars.BattleEngine.State.Entities;

public class Deck : IEntity
{
	public EntityId Id { get; init; }
	public HashSet<Guid> List = [];
}
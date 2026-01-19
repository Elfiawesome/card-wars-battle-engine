namespace CardWars.BattleEngine.State.Entities;

public class Player : IEntity
{
	public EntityId Id { get; init; }
	public string Name = "";
	public HashSet<EntityId> Battlefields = [];
	public HashSet<EntityId> HandCards = [];
}
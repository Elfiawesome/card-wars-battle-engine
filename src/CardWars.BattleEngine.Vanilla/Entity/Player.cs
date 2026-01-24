using CardWars.BattleEngine.State;

namespace CardWars.BattleEngine.Vanilla.Entity;

public class Player(EntityId id) : IEntity
{
	public EntityId Id { get; init; } = id;
	public string Name = "";
	public HashSet<EntityId> Battlefields = [];
	public HashSet<EntityId> HandCards = [];


	public int BehaviourPriority => 0;
	public List<BehaviourPointer> GetBehaviours() => [];
}
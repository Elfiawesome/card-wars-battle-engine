using CardWars.BattleEngine.State;

namespace CardWars.BattleEngine.Vanilla.Entity;

public class Deck(EntityId id) : IEntity
{
	public EntityId Id { get; init; } = id;
	public EntityId OwnerPlayerId { get; set; }
	
	public int BehaviourPriority => 0;
	public List<BehaviourPointer> GetBehaviours() => [];
}
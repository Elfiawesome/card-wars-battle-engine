using CardWars.BattleEngine.Behaviour;

namespace CardWars.BattleEngine.State.Entities;

public class Battlefield : IEntity
{
	public EntityId Id { get; init; }
	public EntityId ParentPlayerId { get; set; }

	public int BehaviourPriority => 0;

	public List<BehaviourPointer> GetBehaviours() => [];
}
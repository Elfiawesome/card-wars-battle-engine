using CardWars.BattleEngine.State;

namespace CardWars.BattleEngine.Vanilla.Entity;

public class Player(EntityId id) : IEntity
{
	public EntityId Id { get; init; } = id;
	public string Name = "";

	// Relationships
	public HashSet<EntityId> BattlefieldIds { get; } = [];
	public HashSet<EntityId> DeckIds { get; } = [];
	public HashSet<EntityId> HandCardIds { get; } = [];

	public int BehaviourPriority => 0;
	public List<BehaviourPointer> GetBehaviours() => [];
}
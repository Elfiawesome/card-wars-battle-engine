using CardWars.BattleEngine.State;
using CardWars.Core.Data;

namespace CardWars.BattleEngine.Vanilla.Entity;

[DataTagType()]
public class Player(EntityId id) : IEntity
{
	[DataTag] public EntityId Id { get; init; } = id;
	[DataTag] public string Name { get; set; } = "";

	// Relationships
	[DataTag] public HashSet<EntityId> BattlefieldIds { get; } = [];
	[DataTag] public HashSet<EntityId> DeckIds { get; } = [];
	[DataTag] public HashSet<EntityId> HandCardIds { get; } = [];

	public int BehaviourPriority => 0;
	public List<BehaviourPointer> GetBehaviours() => [];
}
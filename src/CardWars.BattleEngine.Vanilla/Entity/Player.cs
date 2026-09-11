using CardWars.BattleEngine.State;
using CardWars.Core.Data.Attributes;

namespace CardWars.BattleEngine.Vanilla.Entity;

[DataTagType("battle/entity/player")]
public class Player(EntityId id) : IEntity
{
	[DataTag] public EntityId Id { get; init; } = id;
	[DataTag] public string Name { get; set; } = "";
	[DataTag] public int Team { get; set; } = 0;

	// Relationships
	[DataTag] public HashSet<EntityId> BattlefieldIds { get; } = [];
	[DataTag] public HashSet<EntityId> DeckIds { get; } = [];
	[DataTag] public HashSet<EntityId> HandCardIds { get; } = [];

	public int BehaviourPriority => 0;
	public List<BehaviourPointer> GetBehaviours() => [];
}
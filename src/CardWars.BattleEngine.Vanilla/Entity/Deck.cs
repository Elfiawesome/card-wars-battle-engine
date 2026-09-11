using CardWars.BattleEngine.State;
using CardWars.Core.Data.Attributes;

namespace CardWars.BattleEngine.Vanilla.Entity;

[DataTagType("battle/entity/deck")]
public class Deck(EntityId id) : IEntity
{
	[DataTag] public EntityId Id { get; init; } = id;
	[DataTag] public EntityId? OwnerPlayerId { get; set; }
	[DataTag] public List<EntityId> CardIds { get; set; } = [];

	public int BehaviourPriority => 0;
	public List<BehaviourPointer> GetBehaviours() => [];
}
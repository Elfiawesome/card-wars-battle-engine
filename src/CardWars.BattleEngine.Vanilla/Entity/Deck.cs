using CardWars.BattleEngine.State;
using CardWars.Core.Data;

namespace CardWars.BattleEngine.Vanilla.Entity;

[DataTagType()]
public class Deck(EntityId id) : IEntity
{
	[DataTag] public EntityId Id { get; init; } = id;
	[DataTag] public EntityId? OwnerPlayerId { get; set; }
	[DataTag] public List<EntityId> CardIds { get; set; } = [];

	public int BehaviourPriority => 0;
	public List<BehaviourPointer> GetBehaviours() => [];
}
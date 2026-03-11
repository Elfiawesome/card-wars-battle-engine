using CardWars.BattleEngine.State;
using CardWars.Core.Data;

namespace CardWars.BattleEngine.Vanilla.Entity;

[DataTagType()]
public class Battlefield(EntityId id) : IEntity
{
	[DataTag] public EntityId Id { get; init; } = id;
	[DataTag] public EntityId OwnerPlayerId { get; set; }
	[DataTag] public HashSet<EntityId> UnitSlotIds { get; } = [];

	public int BehaviourPriority => 0;

	public List<BehaviourPointer> GetBehaviours() => [];
}
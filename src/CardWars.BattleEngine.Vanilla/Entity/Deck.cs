using CardWars.Core.Data;
using CardWars.BattleEngine.State;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Registry;

namespace CardWars.BattleEngine.Vanilla.Entity;

[DataTagType("battle/entity/deck")]
public class Deck(EntityId id) : IEntity, IParentableEntity
{
	[DataTag] public EntityId Id { get; init; } = id;
	[DataTag] public EntityId? OwnerPlayerId { get; set; }
	[DataTag] public List<EntityId> CardIds { get; set; } = [];
	public EntityId? ParentId => OwnerPlayerId;

	public int BehaviourPriority => 0;



	public List<ContentPointer> GetBehaviours(DataTagContext dataTags) => [];
}
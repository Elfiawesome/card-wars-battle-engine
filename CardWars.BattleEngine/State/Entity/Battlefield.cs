using CardWars.Core.Common.Mapping;

namespace CardWars.BattleEngine.State.Entity;

public class Battlefield(BattlefieldId id) : EntityState<BattlefieldId>(id)
{
	[PropertyMapping]
	public PlayerId OwnerPlayerId { get; set; }


	[PropertyMapping]
	public HashSet<UnitSlotId> ControllingUnitSlotIds { get; set; } = [];
}

public record struct BattlefieldId(Guid Id) : IStateId;
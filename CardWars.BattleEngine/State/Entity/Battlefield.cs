using CardWars.Core.Common.Mapping;

namespace CardWars.BattleEngine.State.Entity;

public class Battlefield(BattlefieldId id) : EntityState<BattlefieldId>(id)
{
	[PropertyMapping("owner_player_id")]
	public PlayerId OwnerPlayerId { get; set; }


	[PropertyMapping("controlling_unit_slot_ids")]
	public HashSet<UnitSlotId> ControllingUnitSlotIds { get; set; } = [];
}

public record struct BattlefieldId(Guid Id) : IStateId;
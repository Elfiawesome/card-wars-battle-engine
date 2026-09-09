using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Client.scenes.vanilla.instance.battle;
using CardWars.Client.scripts.vanilla.registry;
using CardWars.Vanilla.Shared;
using Godot;

namespace CardWars.Client.scripts.vanilla.entity_view;

public class UnitSlotViewHandler : IEntityViewHandler<UnitSlot>
{
	public void Sync(BattleInstance instance, UnitSlot entity)
	{
		var node = instance.GetOrCreateEntityNode(SharedIds.UnitSlot, entity.Id);
		if (node == null) return;

		instance.AttachNodeToOwner(node, entity.OwnerBattlefieldId);

		node.Position = new Vector3(entity.Position.X * 0.95f, 0, -entity.Position.Y * 0.95f);
	}
}

using System.Collections.Generic;
using System.Linq;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Client.scenes.cardwars.instance.battle;
using CardWars.Client.scenes.cardwars.instance.battle.entities;
using CardWars.Client.scripts.cardwars.registry;
using CardWars.Vanilla.Shared;

namespace CardWars.Client.scripts.cardwars.entity_view;

public class UnitSlotViewHandler : IEntityViewHandler<UnitSlot>
{
	public void Sync(BattleInstance instance, IReadOnlyCollection<UnitSlot> entities)
	{
		var activeIds = entities.Select(e => e.Id).ToHashSet();

		foreach (var id in instance.GetEntityNodeIds<UnitSlotNode>())
		{
			if (!activeIds.Contains(id)) instance.FreeEntityNode(id);
		}

		foreach (var entity in entities)
		{
			var node = instance.GetOrCreateEntityNode<UnitSlotNode>(SharedIds.Entities.UnitSlot, entity.Id);
			if (node is null) continue;

			instance.AttachNodeToOwner(node, entity.OwnerBattlefieldId);
			node.HookMouseEvents();
			node.SetGridPosition(entity.Position);
		}
	}
}

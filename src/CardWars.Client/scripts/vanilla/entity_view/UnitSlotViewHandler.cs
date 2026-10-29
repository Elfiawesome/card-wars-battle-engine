using System.Collections.Generic;
using System.Linq;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Client.scenes.vanilla.instance.battle;
using CardWars.Client.scripts.vanilla.registry;
using CardWars.Core.Logging;
using CardWars.Vanilla.Shared;

namespace CardWars.Client.scripts.vanilla.entity_view;

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
			if (node == null) continue;

			instance.AttachNodeToOwner(node, entity.OwnerBattlefieldId);
			node.OnMouseEntered += () => { instance.Hovered3DTarget.Node = node; Log.Info($"Entered: {node}"); };
			node.OnMouseExited += () => { instance.Hovered3DTarget.Node = null; };
			node.OnMouseClicked += () => { instance.OnHovered3DTargetClicked?.Invoke(node); };
			node.SetGridPosition(entity.Position);
		}
	}
}

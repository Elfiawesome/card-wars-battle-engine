using System.Collections.Generic;
using System.Linq;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Client.scenes.vanilla.instance.battle;
using CardWars.Client.scripts.vanilla.registry;
using CardWars.Vanilla.Shared;

namespace CardWars.Client.scripts.vanilla.entity_view;

public class BattlefieldViewHandler : IEntityViewHandler<Battlefield>
{
	public void Sync(BattleInstance instance, IReadOnlyCollection<Battlefield> entities)
	{
		var activeIds = entities.Select(e => e.Id).ToHashSet();

		foreach (var id in instance.GetEntityNodeIds<BattlefieldNode>())
		{
			if (!activeIds.Contains(id)) instance.FreeEntityNode(id);
		}

		foreach (var entity in entities)
		{
			var node = instance.GetOrCreateEntityNode<BattlefieldNode>(SharedIds.Entities.Battlefield, entity.Id);
			if (node is null) continue;

			instance.AttachNodeToOwner(node, entity.OwnerPlayerId);
			node.HookMouseEvents();
		}
	}
}

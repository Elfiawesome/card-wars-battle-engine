using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Client.scenes.vanilla.instance.battle;
using CardWars.Client.scripts.vanilla.registry;
using CardWars.Vanilla.Shared;

namespace CardWars.Client.scripts.vanilla.entity_view;

public class BattlefieldViewHandler : IEntityViewHandler<Battlefield>
{
	public void Sync(BattleInstance instance, Battlefield entity)
	{
		var node = instance.GetOrCreateEntityNode(SharedIds.Battlefield, entity.Id);
		if (node == null) return;

		instance.AttachNodeToOwner(node, entity.OwnerPlayerId);
	}
}

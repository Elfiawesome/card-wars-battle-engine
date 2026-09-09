using System.Linq;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Client.scenes.vanilla.instance.battle;
using CardWars.Client.scripts.vanilla.registry;
using CardWars.Core.Logging;
using CardWars.Vanilla.Shared;
using Godot;

namespace CardWars.Client.scripts.vanilla.entity_view;

public class BattlefieldViewHandler : IEntityViewHandler<Battlefield>
{
	public void Sync(BattleInstance instance, Battlefield entity)
	{
		var node = instance.GetOrCreateEntityNode(SharedIds.Battlefield, entity.Id);
		if (node == null) return;

		instance.AttachNodeToOwner(node, entity.OwnerPlayerId);


		// Reposition all battles
		RearrangeBattles(instance);
	}

	private void RearrangeBattles(BattleInstance instance)
	{
		// Log.Info("All Battlefields");
		// foreach(var b in instance.EntityNodes)
		// {
		// 	Log.Info("Battlefields:");
		// 	Log.Info(b);
		// }
	}
}

using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Client.scenes.vanilla.instance.battle;
using CardWars.Client.scripts.vanilla.registry;
using CardWars.Vanilla.Shared;
using Godot;

namespace CardWars.Client.scripts.vanilla.entity_view;

public class CardViewHandler : IEntityViewHandler<GenericCard>
{
	public void Sync(BattleInstance instance, GenericCard entity)
	{
		if (entity.OwnerUnitSlotId != null && entity.OwnerPlayerId == null)
		{
			var node = instance.GetOrCreateEntityNode<Node3D>(SharedIds.Card, entity.Id);
			if (node == null)
			{

			}
		}

		if (entity.OwnerPlayerId == instance.MyPlayerId && entity.OwnerUnitSlotId == null)
		{
			var card = instance?.HandManagerNode?.GetCard(entity.Id);
			if (card == null)
			{
				instance?.HandManagerNode?.AddCard(entity.Id);
			}
		}
	}
}

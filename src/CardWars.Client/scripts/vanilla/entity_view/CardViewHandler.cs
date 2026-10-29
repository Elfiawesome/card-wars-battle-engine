using System.Collections.Generic;
using System.Linq;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Client.scenes.vanilla.instance.battle;
using CardWars.Client.scripts.vanilla.registry;

namespace CardWars.Client.scripts.vanilla.entity_view;

public class CardViewHandler : IEntityViewHandler<GenericCard>
{
	public void Sync(BattleInstance instance, IReadOnlyCollection<GenericCard> entities)
	{
		var hand = instance.HandManagerNode;
		if (hand is null) return;

		var handIds = entities
			.Where(e => e.OwnerPlayerId == instance.MyPlayerId && e.OwnerUnitSlotId is null)
			.Select(e => e.Id)
			.ToHashSet();

		foreach (var id in hand.Cards.Keys.ToList())
		{
			if (!handIds.Contains(id)) hand.RemoveCard(id);
		}

		foreach (var id in handIds)
		{
			if (hand.GetCard(id) is null) hand.AddCard(id);
		}
	}
}

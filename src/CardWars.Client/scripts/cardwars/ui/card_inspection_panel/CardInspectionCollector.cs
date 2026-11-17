using System.Collections.Generic;
using CardWars.Client.scenes.core.game_session;
using CardWars.Client.scripts.cardwars.registry;
using CardWars.Core.Data.Tags;

namespace CardWars.Client.scripts.cardwars.ui.card_inspection_panel;

public static class CardInspectionCollector
{
	public static IReadOnlyList<InspectionData> Collect(
		CompoundTag data,
		ClientInstanceRegistry registry
	)
	{
		var context = new CardInspectionContext(data, registry);
		var panels = new List<InspectionData>();
		if (registry.Client?.GetExtension<BattleRegistry>() is not { } battle) return panels;

		foreach (var handlerId in battle.CardInspectionHandlerOrder)
		{
			if (battle.CardInspectionHandlers.Get(handlerId) is { } handler)
				panels.AddRange(handler.GetInspectionPanels(context));
		}

		return panels;
	}
}

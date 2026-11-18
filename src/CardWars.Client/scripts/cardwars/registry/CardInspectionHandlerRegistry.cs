using System.Collections.Generic;
using CardWars.Client.scenes.core.game_session;
using CardWars.Client.scripts.cardwars.ui.card_inspection_panel;
using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;

namespace CardWars.Client.scripts.cardwars.registry;

public sealed class CardInspectionHandlerRegistry : Registry<ResourceId, ICardInspectionHandler>
{
	private readonly List<ResourceId> _order = [];

	public new void Register(ResourceId id, ICardInspectionHandler handler)
	{
		if (Get(id) is null) _order.Add(id);
		base.Register(id, handler);
	}

	public IReadOnlyList<CardInspectionData> Collect(CompoundTag data, ClientInstanceRegistry registry)
	{
		var context = new CardInspectionContext(data, registry);
		var panels = new List<CardInspectionData>();

		foreach (var id in _order)
		{
			if (Get(id) is { } handler)
				panels.AddRange(handler.GetInspectionPanels(context));
		}

		return panels;
	}
}

using System.Collections.Generic;
using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;

namespace CardWars.Client.scripts.cardwars.ui.card_inspection_panel;

public sealed class CardInspectionKeywordHandler : ICardInspectionHandler
{
	public IReadOnlyList<CardInspectionData> GetInspectionPanels(CardInspectionContext context)
	{
		var panels = new List<CardInspectionData>();
		if (context.Data.GetList("keywords") is not { } keywords) return panels;

		foreach (var tag in keywords.Items)
		{
			if (tag is not CompoundTag keyword) continue;

			var title = keyword.GetString("name");
			if (string.IsNullOrWhiteSpace(title)) title = "Keyword";

			panels.Add(new CardInspectionData(
				title,
				keyword.GetString("description") ?? "",
				ParseResource(keyword.GetString("texture")),
				null));
		}

		return panels;
	}

	private static ResourceId? ParseResource(string? value)
		=> string.IsNullOrWhiteSpace(value) ? (ResourceId?)null : ResourceId.Parse(value);
}

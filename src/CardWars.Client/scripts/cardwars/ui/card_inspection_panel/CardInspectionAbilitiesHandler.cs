using System.Collections.Generic;
using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;

namespace CardWars.Client.scripts.cardwars.ui.card_inspection_panel;

public sealed class CardInspectionAbilitiesHandler : ICardInspectionHandler
{
	public IReadOnlyList<CardInspectionData> GetInspectionPanels(CardInspectionContext context)
	{
		var panels = new List<CardInspectionData>();
		if (context.Data.GetList("abilities") is not { } abilities) return panels;

		foreach (var tag in abilities.Items)
		{
			if (tag is not CompoundTag ability) continue;

			var typePointer = ability.GetString("ability_type");
			var abilityType = string.IsNullOrWhiteSpace(typePointer)
				? null
				: context.Registry.Battle?.AbilityTypeDefinitions.Get(ResourceId.Parse(typePointer));

			var title = abilityType?.GetString("name") ?? ability.GetString("name");
			if (string.IsNullOrWhiteSpace(title)) title = "Ability";

			panels.Add(new CardInspectionData(
				title,
				abilityType?.GetString("text") ?? "",
				ParseResource(abilityType?.GetString("texture")),
				null,
				abilityType?.GetString("color") ?? "#ffffff"
				));
		}

		return panels;
	}

	private static ResourceId? ParseResource(string? value)
		=> string.IsNullOrWhiteSpace(value) ? (ResourceId?)null : ResourceId.Parse(value);
}

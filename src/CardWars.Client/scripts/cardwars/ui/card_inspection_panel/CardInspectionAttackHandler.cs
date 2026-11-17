using System.Collections.Generic;
using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;

namespace CardWars.Client.scripts.cardwars.ui.card_inspection_panel;

public sealed class CardInspectionAttackHandler : ICardInspectionHandler
{
	public IReadOnlyList<InspectionData> GetInspectionPanels(CardInspectionContext context)
	{
		var panels = new List<InspectionData>();
		if (context.Data.GetList("sp_atks") is not { } spAtks) return panels;

		foreach (var tag in spAtks.Items)
		{
			if (tag is not CompoundTag pointer) continue;

			var attack = ResolveAttack(context, pointer);
			var title = attack?.GetString("name") ?? pointer.GetString("name");
			if (string.IsNullOrWhiteSpace(title)) title = "SP ATK";

			var charge = attack?.GetInt("charge_cost", 1) ?? pointer.GetInt("charge_cost", 1);
			var bypass = attack?.GetBool("bypass") ?? pointer.GetBool("bypass");
			var text = bypass ? $"CHARGE {charge} · BYPASS" : $"CHARGE {charge}";

			panels.Add(new InspectionData(
				title,
				text,
				ParseResource(attack?.GetString("texture")),
				BuildGrid(attack)));
		}

		return panels;
	}

	private static CompoundTag? ResolveAttack(CardInspectionContext context, CompoundTag pointer)
	{
		if (pointer.Has("definition")) return pointer.GetCompound("definition");

		var resource = pointer.GetString("resource");
		return string.IsNullOrWhiteSpace(resource)
			? null
			: context.Registry.Battle?.AttackDefinitions.Get(ResourceId.Parse(resource));
	}

	private static InspectionGrid BuildGrid(CompoundTag? attack)
	{
		var cells = new List<InspectionGridCell>(25);
		for (var index = 0; index < 25; index++)
			cells.Add(new InspectionGridCell(false, false, ""));

		if (attack?.GetList("patterns") is { } patterns)
		{
			foreach (var tag in patterns.Items)
			{
				if (tag is not CompoundTag pattern) continue;

				var type = pattern.GetString("type");
				var x = pattern.GetInt("x");
				var y = pattern.GetInt("y");
				var value = pattern.GetString("value") ?? "";

				switch (type)
				{
					case "cell":
						Fill(cells, x, y, value);
						break;
					case "row":
						for (var column = 0; column < 5; column++) Fill(cells, column, y, value);
						break;
					case "column":
						for (var row = 0; row < 5; row++) Fill(cells, x, row, value);
						break;
					case "all":
						for (var index = 0; index < 25; index++) Fill(cells, index % 5, index / 5, value);
						break;
				}
			}
		}

		return new InspectionGrid(5, cells);
	}

	private static void Fill(List<InspectionGridCell> cells, int x, int y, string text)
	{
		if (x is < 0 or > 4 || y is < 0 or > 4) return;
		cells[y * 5 + x] = new InspectionGridCell(true, false, text);
	}

	private static ResourceId? ParseResource(string? value)
		=> string.IsNullOrWhiteSpace(value) ? (ResourceId?)null : ResourceId.Parse(value);
}

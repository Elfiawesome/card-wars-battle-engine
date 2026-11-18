using System;
using System.Collections.Generic;
using CardWars.BattleEngine.Vanilla.Attacks;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Data.Tags;
using CardWars.Core.Logging;
using CardWars.Core.Registry;

namespace CardWars.Client.scripts.cardwars.ui.card_inspection_panel;

public sealed class CardInspectionAttackHandler : ICardInspectionHandler
{
	public IReadOnlyList<InspectionData> GetInspectionPanels(CardInspectionContext context)
	{
		var panels = new List<InspectionData>();
		if (context.Data.GetList("sp_atks") is not { } spAtks) return panels;
		if (context.Registry.Battle is not { } battleRegistry) return panels;

		foreach (var tag in spAtks.Items)
		{
			var resolver = new AttackResolver(battleRegistry);
			var attack = new AttackProfile(true, resolver.GetAttackDefinitionFromPointer(tag));
			var title = attack.Definition.Name;
			var charge = attack.Definition.ChargeCost;
			var text = attack.Definition.Bypass ? $"CHARGE {charge} · BYPASS" : $"CHARGE {charge}"; ;

			panels.Add(new InspectionData(
				title, text, attack.Definition.Texture, BuildGrid(3, attack)
			));
		}
		return panels;
	}

	private InspectionGrid BuildGrid(int size, AttackProfile attack)
	{
		List<InspectionGridCell> inspectionGridCells = [];

		int Midpoint = size / 2;
		for (var x = 0; x < size; x++)
		{
			for (var y = 0; y < size; y++)
			{
				bool filled = false;
				bool crossed = false;
				string text = "";
				if (attack.IsHit(new UnitSlotPos(Midpoint, Midpoint), new UnitSlotPos(x, y), out var spec))
				{
					filled = true;
					if (spec.Percent != 0.0) { text = $"{(int)Math.Round(spec.Percent * 100)}%"; }
					else if (spec.Amount != 0.0) { text = $"{spec.Amount}"; }
					if (attack.Definition.Bypass) crossed = true;
				}

				inspectionGridCells.Add(new(
					filled, crossed, text
				));
			}
		}

		return new InspectionGrid(size, inspectionGridCells);
	}
}

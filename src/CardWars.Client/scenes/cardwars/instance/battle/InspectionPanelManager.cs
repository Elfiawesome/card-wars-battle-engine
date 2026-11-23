using CardWars.BattleEngine;
using CardWars.BattleEngine.Vanilla.Attacks;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Client.scenes.core.game_session;
using CardWars.Client.scripts.cardwars.registry;
using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;
using Godot;
using System;
using System.Collections.Generic;

namespace CardWars.Client.scenes.cardwars.instance.battle;

public partial class InspectionPanelManager : Control
{
	private const float PANEL_GAP = 12f;

	private readonly List<Control> _inspectionPanels = [];
	public VBoxContainer Container => GetNode<VBoxContainer>("VBoxContainer");

	public void PopulatePanels(GenericCard card, BattleRegistry battleRegistry, ClientInstanceRegistry clientInstanceRegistry)
	{
		// For now, all the details are hardcoded
		List<CardInspectionData> panels = GetPanelsData(card, clientInstanceRegistry);
		if (panels.Count == 0) return;

		foreach (var inspectionData in panels)
		{
			var panel = AddPanel(inspectionData, battleRegistry, clientInstanceRegistry);
			if (panel == null) return;
		}
	}

	public Control? AddPanel(CardInspectionData inspectionData, BattleRegistry battleRegistry, ClientInstanceRegistry clientInstanceRegistry)
	{
		// Get the inspection design skin TODO: Maybe switch it up with skins?
		var panel = clientInstanceRegistry.Client?.UserInterface.Instantiate<Control>(battleRegistry.DefaultCardInspectionPanel);
		if (panel is null) return null;

		Container.AddChild(panel);
		panel.MouseFilter = MouseFilterEnum.Ignore;
		panel.MouseBehaviorRecursive = MouseBehaviorRecursiveEnum.Disabled;

		ApplyInspectionData(panel, inspectionData, clientInstanceRegistry);

		_inspectionPanels.Add(panel);
		return panel;
	}

	public void ArrangePanels(Rect2 rect)
	{
		if (_inspectionPanels.Count == 0) return;

		Container.Size = new(rect.Size.X, 0);

		var xside = Math.Sign(Size.X / 2 - rect.Position.X);
		if (xside == 0) xside = 1;
		float x = rect.Position.X + xside * (rect.Size.X / 2 + PANEL_GAP);
		float y = Math.Min(rect.Position.Y - rect.Size.Y / 2, Size.Y - Container.Size.Y);
		if (xside == -1) x -= Container.Size.X;
		Container.Position = new Vector2(x, y);

		for (var index = 0; index < _inspectionPanels.Count; index++)
		{
			var panel = _inspectionPanels[index];
			panel.Set("card_rect", new Rect2(rect.Position - Container.Position, rect.Size));
			if (panel.HasMethod("play_intro")) panel.Call("play_intro", [index * 0.03]);
		}
	}

	public void PopulateAndArrangePanels(
		GenericCard card,
		BattleRegistry battleRegistry,
		ClientInstanceRegistry clientInstanceRegistry,
		Rect2 rect
	)
	{
		PopulatePanels(card, battleRegistry, clientInstanceRegistry);
		ArrangePanels(rect);
	}

	public void ClearInspectionPanels()
	{
		foreach (var panel in _inspectionPanels)
		{
			if (!IsInstanceValid(panel)) continue;
			panel.GetParent()?.RemoveChild(panel);
			panel.QueueFree();
		}

		_inspectionPanels.Clear();
	}

	private void ApplyInspectionData(Control panel, CardInspectionData data, ClientInstanceRegistry clientInstanceRegistry)
	{
		if (panel.HasMethod("set_content"))
		{
			var texture = data.IconId is { } iconId
				? clientInstanceRegistry.Client?.Resources.Instantiate<Texture2D>(iconId) ?? new ImageTexture()
				: new ImageTexture();
			panel.Call("set_content", data.Title, texture, data.Text, data.Color);
		}

		if (data.Grid is not { } grid || !panel.HasMethod("set_grid")) return;

		var cells = new Godot.Collections.Array();
		foreach (var cell in grid.Cells)
		{
			var item = new Godot.Collections.Dictionary();
			if (cell.Filled) item["filled"] = true;
			if (cell.Crossed) item["crossed"] = true;
			if (!string.IsNullOrWhiteSpace(cell.Text)) item["text"] = cell.Text;
			cells.Add(item);
		}

		panel.Call("set_grid", cells, grid.Width);
	}

	public List<CardInspectionData> GetPanelsData(GenericCard card, ClientInstanceRegistry registry)
	{
		if (registry.Battle is not { } battleRegistry) return [];

		List<CardInspectionData> panelDataList = [];
		if (card.Data.GetList("abilities") is { } abilities)
		{
			// Add Ability Panels
			foreach (var tag in abilities.Items)
			{
				if (tag is not CompoundTag ability) continue;

				var typePointer = ability.GetString("ability_type");
				var abilityType = string.IsNullOrWhiteSpace(typePointer)
					? null
					: registry.Battle?.AbilityTypeDefinitions.Get(ResourceId.Parse(typePointer));

				var title = abilityType?.GetString("name") ?? ability.GetString("name");
				if (string.IsNullOrWhiteSpace(title)) title = "Ability";

				ResourceId? textureId = null;
				if (abilityType?.GetString("texture") is { } abilityTextureId)
					textureId = ResourceId.Parse(abilityTextureId);

				panelDataList.Add(new CardInspectionData(
					title,
					abilityType?.GetString("text") ?? "",
					textureId,
					null,
					abilityType?.GetString("color") ?? "#ffffff"
				));
			}
		}

		// SP Atk
		if (card.Data.GetList("sp_atks") is { } spAtks)
		{
			foreach (var tag in spAtks.Items)
			{
				var resolver = new AttackResolver(battleRegistry);
				var attack = new AttackProfile(true, resolver.GetAttackDefinitionFromPointer(tag));
				var title = attack.Definition.Name;
				var charge = attack.Definition.ChargeCost;
				var text = attack.Definition.Bypass ? $"CHARGE {charge} · BYPASS" : $"CHARGE {charge}"; ;

				panelDataList.Add(new CardInspectionData(
					title, text, attack.Definition.Texture, BuildGrid(3, attack)
				));
			}

			static CardInspectionGrid BuildGrid(int size, AttackProfile attack)
			{
				List<CardInspectionGridCell> inspectionGridCells = [];

				int Midpoint = size / 2;
				for (var y = 0; y < size; y++)
				{
					for (var x = 0; x < size; x++)
					{
						bool filled = false;
						bool crossed = false;
						string text = "";
						if (attack.IsHit(new UnitSlotPos(Midpoint, Midpoint), new UnitSlotPos(x, y), out var spec))
						{
							filled = true;
							if (spec.Percent != 0.0) { text = $"{(int)Math.Round(spec.Percent * 100)}%"; }
							else if (spec.Amount != 0.0) { text = $"{spec.Amount}"; }
						}

						if (Midpoint == x && Midpoint == y)
						{
							text = "";
							if (attack.Definition.Bypass) { crossed = true; }
						}

						inspectionGridCells.Add(new(
							filled, crossed, text
						));
					}
				}
				return new CardInspectionGrid(size, inspectionGridCells);
			}
		}

		return panelDataList;
	}
}

public sealed record CardInspectionData(
	string Title,
	string Text,
	ResourceId? IconId,
	CardInspectionGrid? Grid,
	string Color = "#ffffff"
);

public sealed record CardInspectionGrid(
	int Width,
	IReadOnlyList<CardInspectionGridCell> Cells
);

public readonly record struct CardInspectionGridCell(
	bool Filled,
	bool Crossed,
	string Text
);
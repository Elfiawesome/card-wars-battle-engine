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
	public enum PanelGroupings { Ability, AbilityType, SpAtks }

	private const float PANEL_GAP = 12f;

	private readonly List<Control> _inspectionPanels = [];
	public VBoxContainer Container => GetNode<VBoxContainer>("VBoxContainer");

	public void PopulatePanels(GenericCard card, BattleViewRegistry battleView, ClientInstanceRegistry clientInstanceRegistry)
	{
		// For now, all the details are hardcoded
		List<CardInspectionData> panels = [
			..GetPanelsData(PanelGroupings.Ability, card, clientInstanceRegistry),
			..GetPanelsData(PanelGroupings.AbilityType, card, clientInstanceRegistry),
			..GetPanelsData(PanelGroupings.SpAtks, card, clientInstanceRegistry)
		];

		if (panels.Count == 0) return;

		foreach (var inspectionData in panels)
		{
			var panel = AddPanel(inspectionData, battleView, clientInstanceRegistry);
			if (panel == null) return;
		}
	}

	public Control? AddPanel(CardInspectionData inspectionData, BattleViewRegistry battleView, ClientInstanceRegistry clientInstanceRegistry)
	{
		// Get the inspection design skin TODO: Maybe switch it up with skins?
		var panel = clientInstanceRegistry.Client?.UserInterface.Instantiate<Control>(battleView.DefaultCardInspectionPanel);
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
		BattleViewRegistry battleView,
		ClientInstanceRegistry clientInstanceRegistry,
		Rect2 rect
	)
	{
		PopulatePanels(card, battleView, clientInstanceRegistry);
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

	public List<CardInspectionData> GetPanelsData(PanelGroupings panelGroupings, GenericCard card, ClientInstanceRegistry registry)
	{
		if (registry.Client?.GetExtension<BattleClientRuntime>() is not { } battleRuntime) return [];
		var battleEngine = battleRuntime.Engine;
		var definitions = battleRuntime.Definitions;

		List<CardInspectionData> panelDataList = [];
		switch (panelGroupings)
		{
			case PanelGroupings.AbilityType:
				{
					if (card.Data.GetList("abilities") is { } abilities)
					{
						foreach (var tag in abilities.Items)
						{
							if (tag is not CompoundTag ability) continue;

							var typePointer = ability.GetString("ability_type");
							var abilityType = string.IsNullOrWhiteSpace(typePointer)
								? null
								: definitions?.AbilityTypeDefinitions.Get(ResourceId.Parse(typePointer));

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
				}
				break;
			case PanelGroupings.Ability:
				{
					if (card.Data.GetList("abilities") is { } abilities)
					{
						foreach (var tag in abilities.Items)
						{
							if (tag is not CompoundTag ability) continue;

							var typePointer = ability.GetString("ability_type");
							var abilityType = string.IsNullOrWhiteSpace(typePointer)
								? null
								: definitions?.AbilityTypeDefinitions.Get(ResourceId.Parse(typePointer));

							var title = abilityType?.GetString("name") ?? ability.GetString("name");
							if (string.IsNullOrWhiteSpace(title)) title = "Ability";

							ResourceId? textureId = null;
							if (abilityType?.GetString("texture") is { } abilityTextureId)
								textureId = ResourceId.Parse(abilityTextureId);

							panelDataList.Add(new CardInspectionData(
								title,
								ability?.GetString("description") ?? "",
								textureId,
								null,
								abilityType?.GetString("color") ?? "#ffffff"
							));
						}
					}
				}
				break;
			case PanelGroupings.SpAtks:
				{
					if (card.Data.GetList("sp_atks") is { } spAtks)
					{
						foreach (var tag in spAtks.Items)
						{
							var resolver = new AttackResolver(battleEngine);
							var attack = new AttackProfile(true, resolver.GetAttackDefinitionFromPointer(tag));
							var title = attack.Definition.Name;
							var charge = attack.Definition.ChargeCost;
							var text = attack.Definition.Bypass ? $"CHARGE {charge} · BYPASS" : $"CHARGE {charge}"; ;

							panelDataList.Add(new CardInspectionData(
								title, text, attack.Definition.Texture, attack.RenderPattern(3)
							));
						}
					}
				}
				break;
		}
		return panelDataList;
	}
}

public sealed record CardInspectionData(
	string Title,
	string Text,
	ResourceId? IconId,
	AttackProfile.RenderedAttackPatternGrid? Grid,
	string Color = "#ffffff"
);
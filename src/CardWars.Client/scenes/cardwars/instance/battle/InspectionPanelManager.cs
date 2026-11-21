using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Client.scenes.core.game_session;
using CardWars.Client.scripts.cardwars.registry;
using CardWars.Client.scripts.cardwars.ui.card_inspection_panel;
using CardWars.Core.Logging;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CardWars.Client.scenes.cardwars.instance.battle;

public partial class InspectionPanelManager : Control
{
	private const float PANEL_GAP = 12f;

	private readonly List<Control> _inspectionPanels = [];

	private float PanelHeights => _inspectionPanels.Sum(s => s.Size.X + PANEL_GAP);
	private float PanelWidths => _inspectionPanels.Sum(s => s.GetMinimumSize().X);

	public void PopulatePanels(GenericCard card, BattleRegistry battleRegistry, ClientInstanceRegistry clientInstanceRegistry)
	{
		var panels = battleRegistry.CardInspectionHandlers.Collect(card.Data, clientInstanceRegistry);
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

		AddChild(panel);
		// TODO: Fucking change this design choice please
		panel.Modulate = Color.FromString(inspectionData.Color, Colors.White).Lightened(0.8f);
		panel.MouseFilter = MouseFilterEnum.Ignore;
		panel.MouseBehaviorRecursive = MouseBehaviorRecursiveEnum.Disabled;

		ApplyInspectionData(panel, inspectionData, clientInstanceRegistry);

		_inspectionPanels.Add(panel);
		return panel;
	}

	public void ArrangePanels(Rect2 rect)
	{
		var xside = Math.Sign(Size.X / 2 - rect.Position.X);
		if (xside == 0) xside = 1;
		float x = rect.Position.X + xside * (rect.Size.X / 2 + PANEL_GAP);
		float y = Math.Min(rect.Position.Y - rect.Size.Y / 2, Size.Y - PanelHeights);
		if (xside == -1) x -= PanelWidths;

		float currentY = 0;
		for (var index = 0; index < _inspectionPanels.Count; index++)
		{
			Log.Info(_inspectionPanels.Count);
			var panel = _inspectionPanels[index];
			panel.Position = new(x, y + currentY);
			
			currentY += panel.Size.Y + PANEL_GAP;
			// To make it neater, we standardize the x
			panel.Size = new(PanelWidths, panel.Size.Y);
			// Play animation
			panel.Set("card_rect", rect);
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
			panel.Call("set_content", data.Title, texture, data.Text);
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
}

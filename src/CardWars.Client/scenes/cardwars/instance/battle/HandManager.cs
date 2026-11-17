using System;
using System.Collections.Generic;
using System.Linq;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.BattleEngine.Vanilla.Features;
using CardWars.Client.scenes.cardwars.ui.battle;
using CardWars.Client.scenes.cardwars.ui.card_display;
using CardWars.Client.scripts.cardwars.registry;
using CardWars.Client.scripts.cardwars.ui.card_inspection_panel;
using CardWars.Core.Logging;
using CardWars.Core.Registry;
using CardWars.Vanilla.Shared;
using Godot;

namespace CardWars.Client.scenes.cardwars.instance.battle;

public partial class HandManager : Control
{
	public const float HandAngleDeg = 100.0f;
	public const float CardAngleDeg = 180.0f;
	public const float HoverRaiseAmount = 60.0f;
	public const float SelectedLowerAmount = 180.0f;
	public const int IndexGap = 3;
	public int HoverZIndex => _cards.Count * IndexGap;

	public Vector2 CardScale = Vector2.One * 0.2f; // 0.15f;

	public CardDisplay? HoveredCard = null;
	public CardDisplay? SelectedCard = null;
	public LineArrow? LineArrow = null;
	public HashSet<EntityId> ArrowTargets = [];
	public BattleInstance? BattleInstance = null;

	private Dictionary<EntityId, CardDisplay> _cards = [];
	public IReadOnlyDictionary<EntityId, CardDisplay> Cards => _cards;
	private readonly List<Control> _inspectionPanels = [];


	public override void _Ready()
	{
		Resized += ArrangeCard;
	}

	public override void _Process(double delta)
	{
		if (LineArrow is not { } lineArrow || BattleInstance?.CameraNode is not { } camera) return;

		if (BattleInstance.Hovered3DTarget is { } target && ArrowTargets.Contains(target.EntityId))
			lineArrow.SnapTo(camera, target);
		else
			lineArrow.FollowMouse();
	}


	public void RefreshTargeting()
	{
		ArrowTargets.Clear();

		if (SelectedCard is null) return;
		if (BattleInstance is not { } instance) return;
		if (instance.ActionValidator is not { } validator) return;

		var cardEntry = Cards.FirstOrDefault(entry => entry.Value == SelectedCard);
		if (instance.State.Get<GenericCard>(cardEntry.Key) is { } candidate
			&& validator.CanStartPlayCard(instance.State, instance.MyPlayerId, candidate, out _))
		{
			ArrowTargets = [.. validator.GetPlayableTargets(instance.State, instance.MyPlayerId, candidate)];
		}

		instance.TargetHighlighter.Clear(this);
		instance.TargetHighlighter.Set(this, ArrowTargets);
	}

	public CardDisplay? GetCard(EntityId entityId) => _cards.TryGetValue(entityId, out var card) ? card : null;

	public void AddCard(EntityId id)
	{
		if (BattleInstance is not { } instance || instance.State.Get<GenericCard>(id) is not { } entity) return;

		// TODO: Get skin and place it inside
		var card = instance.Registry.Client?.UserInterface.Instantiate<CardDisplay>(SharedIds.UI.StandardCardDisplay);
		if (card is null) return;

		card.Scale = Vector2.Zero;
		card.Position = new(0, 0);

		card.MouseEntered += () => OnCardMouseEntered(card);
		card.MouseExited += () => OnCardMouseExited(card);
		card.GuiInput += (e) => OnCardGuiInput(e, card);
		AddChild(card);
		card.SetData(entity.Data, instance.Registry);
		card.SetDisplay(CardDisplay.DisplayType.Simplified);
		_cards[id] = card;
		ArrangeCard();
	}

	public void RemoveCard(EntityId id)
	{
		if (_cards.TryGetValue(id, out var c))
		{
			_cards.Remove(id);

			if (HoveredCard == c) HoveredCard = null;

			if (SelectedCard == c)
			{
				ArrowTargets.Clear();
				ResetArrows();
			}

			c.currentAnimationTween?.Kill();
			c.currentAnimationTween = null;
			c.MouseFilter = MouseFilterEnum.Ignore;
			var tween = c.CreateTween()
				.SetParallel(true)
				.SetTrans(Tween.TransitionType.Cubic)
				.SetEase(Tween.EaseType.Out);

			tween.TweenProperty(c, "scale", Vector2.Zero, 0.25);
			tween.TweenProperty(c, "position", c.Position + new Vector2(0f, 50f), 0.25);
			tween.TweenProperty(c, "modulate:a", 0.0, 0.25);
			tween.TweenProperty(c, "rotation", c.Rotation - Mathf.Pi / 4, 0.25);
			tween.Finished += c.QueueFree;
			ArrangeCard();
		}
	}

	private void OnCardMouseEntered(CardDisplay card)
	{
		HoveredCard = card;
		ArrangeCard();
	}

	private void OnCardMouseExited(CardDisplay card)
	{
		if (HoveredCard == card) { HoveredCard = null; ArrangeCard(); }
	}

	private void OnCardGuiInput(InputEvent @event, CardDisplay card)
	{
		if (@event is InputEventMouseButton inputEventMouseButton)
		{
			if (inputEventMouseButton.ButtonIndex == MouseButton.Left)
			{
				if (inputEventMouseButton.Pressed)
				{
					if (SelectedCard is null)
					{
						var cardId = Cards.FirstOrDefault(entry => entry.Value == card).Key;
						if (BattleInstance is not { } instance || instance.ActionValidator is not { } validator) return;
						if (instance.State.Get<GenericCard>(cardId) is not { } cardEntity) return;
						if (!validator.CanStartPlayCard(instance.State, instance.MyPlayerId, cardEntity, out _)) return;

						SelectedCard = card;
						RefreshTargeting();

						LineArrow?.QueueFree();
						LineArrow = BattleInstance?.Registry.Client?.UserInterface.Instantiate<LineArrow>(SharedIds.UI.LineArrow);
						if (LineArrow is not null) AddChild(LineArrow);
					}
				}
				ArrangeCard();
			}
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton inputEventMouseButton)
		{
			if (inputEventMouseButton.Pressed && inputEventMouseButton.ButtonIndex == MouseButton.Left)
			{
				if (SelectedCard is not null)
				{
					if (BattleInstance is not { } instance
						|| instance.Hovered3DTarget?.EntityId is not { } targetId
						|| !ArrowTargets.Contains(targetId))
					{
						ResetArrows();
						return;
					}

					var cardId = Cards.FirstOrDefault(entry => entry.Value == SelectedCard).Key;
					if (instance.State.Get<GenericCard>(cardId) is not { } card
						|| instance.ActionValidator is not { } validator)
					{
						ResetArrows();
						return;
					}

					if (!validator.CanPlayCard(instance.State, instance.MyPlayerId, card, targetId, out var reason))
					{
						// No session logger is threaded to this UI node yet; use the immutable no-op
						// fallback (R1). Not compiled here (client targets Godot 4.7, not installed).
						NullLogger.Instance.Debug($"Card play rejected: {reason}");
						ResetArrows();
						return;
					}

					instance.SendInput(new UseCardRequestInput(cardId, targetId));
				}
			}
			ArrangeCard();
		}
	}

	private void ResetArrows()
	{
		SelectedCard = null;
		BattleInstance?.TargetHighlighter.Clear(this);
		LineArrow?.QueueFree();
		LineArrow = null;
	}

	private void ArrangeInspectionPanels(Rect2? cardRect, CardDisplay? card)
	{
		ClearInspectionPanels();
		if (cardRect is not { } rect || card is null) return;
		if (BattleInstance is not { } instance) return;
		// Set middle pos
		rect = new Rect2(rect.Position - rect.Size / 2, rect.Size);

		var cardId = EntityId.None;
		foreach (var entry in _cards)
		{
			if (entry.Value != card) continue;
			cardId = entry.Key;
			break;
		}

		if (cardId.IsNone || instance.State.Get<GenericCard>(cardId) is not { } entity) return;

		var panels = CardInspectionCollector.Collect(entity.Data, instance.Registry);
		if (panels.Count == 0) return;

		var prepared = new List<Control>();
		var totalHeight = 0f;
		var panelWidth = 0f;
		const float panelGap = 12f;

		foreach (var data in panels)
		{
			var panel = instance.Registry.Client?.UserInterface.Instantiate<Control>(SharedIds.UI.StandardCardInspectionPanel);
			if (panel is null) continue;

			panel.MouseFilter = MouseFilterEnum.Ignore;
			panel.MouseBehaviorRecursive = MouseBehaviorRecursiveEnum.Disabled;
			ApplyInspectionData(panel, data, instance);

			var minimum = panel.GetCombinedMinimumSize();
			var size = new Vector2(Mathf.Max(350f, minimum.X), Mathf.Max(76f, minimum.Y));
			panel.CustomMinimumSize = size;
			panel.Size = size;
			panelWidth = Mathf.Max(panelWidth, size.X);
			totalHeight += size.Y;
			prepared.Add(panel);
		}

		if (prepared.Count == 0) return;
		totalHeight += (prepared.Count - 1) * panelGap;

		var viewport = Size;
		var leftSpace = rect.Position.X;
		var rightSpace = viewport.X - rect.End.X;
		var placeLeft = leftSpace > rightSpace;
		const float cardGap = 16f;
		var x = placeLeft ? rect.Position.X - panelWidth - cardGap : rect.End.X + cardGap;
		x = Mathf.Clamp(x, 16f, Mathf.Max(16f, viewport.X - panelWidth - 16f));
		// var y = Mathf.Clamp(rect.GetCenter().Y - totalHeight / 2f, 16f, Mathf.Max(16f, viewport.Y - totalHeight - 16f));
		var y = Mathf.Clamp(rect.Position.Y, 0f, Mathf.Max(0f, viewport.Y - totalHeight));


		var cursorY = y;
		for (var index = 0; index < prepared.Count; index++)
		{
			var panel = prepared[index];
			panel.Position = new Vector2(x, cursorY);
			panel.ZIndex = HoverZIndex;
			panel.Set("card_dimensions", rect);
			panel.Set("panel_index", index);
			panel.Set("animated_from_right", placeLeft);
			AddChild(panel);
			_inspectionPanels.Add(panel);
			cursorY += panel.Size.Y + panelGap;
		}
	}

	private static void ApplyInspectionData(Control panel, InspectionData data, BattleInstance instance)
	{
		if (panel.HasMethod("set_content"))
		{
			var texture = data.IconId is { } iconId
				? instance.Registry.Client?.Resources.Instantiate<Texture2D>(iconId) ?? new ImageTexture()
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

	private void ClearInspectionPanels()
	{
		foreach (var panel in _inspectionPanels)
		{
			if (!IsInstanceValid(panel)) continue;
			panel.GetParent()?.RemoveChild(panel);
			panel.QueueFree();
		}

		_inspectionPanels.Clear();
	}

	private void ArrangeCard()
	{
		var cards = _cards.Select(s => s.Value).ToList();
		int totalCard = cards.Count;
		if (totalCard == 0)
		{
			HoveredCard = null;
			ClearInspectionPanels();
			ResetArrows();
			return;
		}

		Vector2 cardSize = Vector2.Zero;
		foreach (var c in cards) { if (c is CardDisplay cd) { cardSize = cd.Size; break; } }
		float cardWidth = cardSize.X * CardScale.X;
		float cardHeight = cardSize.Y * CardScale.Y;

		Vector2 handCenter = new(Size.X / 2, Size.Y - cardHeight / 2);

		float margin = 200f;
		float availableWidth = Size.X - margin * 2;

		float overlapFraction = 0.45f;
		float desiredSpacing = cardWidth * (1.0f - overlapFraction);

		float spacing = totalCard > 1 ? Math.Min(desiredSpacing, availableWidth / (totalCard - 1)) : 0;

		float curveHeight = 15.0f;
		float maxRotationDeg = 12.0f;

		float halfRange = (totalCard - 1) / 2.0f;
		Rect2? inspectionRect = null;

		for (var i = 0; i < totalCard; i++)
		{
			var n = cards[i];
			if (n is CardDisplay c)
			{
				c.ZIndex = i * IndexGap;
				float offset = i - halfRange;
				float normOffset = offset / Math.Max(halfRange, 1);

				float x = handCenter.X + offset * spacing;
				float y = handCenter.Y - curveHeight * normOffset * normOffset;

				float rotDeg = maxRotationDeg * normOffset;

				Vector2 newPos = new Vector2(x, y) - c.Size / 2;
				float newRot = float.DegreesToRadians(rotDeg);
				Vector2 newScale = CardScale;

				c.Modulate = new Color(Modulate, 1.0f);
				if (c == SelectedCard)
				{
					newPos.Y = handCenter.Y / 2;
					newPos.X = handCenter.X + handCenter.X / 2;
					newPos -= c.Size / 2;

					newRot = 0;
					c.ZIndex = HoverZIndex;
					// c.Modulate = new Color(Modulate, 0.5f);
					newScale *= 1.2f;

					if (LineArrow is not null) LineArrow.Position = newPos + c.Size / 2;
				}
				else
				{
					if (SelectedCard is null)
					{
						if (c == HoveredCard)
						{
							newPos.Y -= HoverRaiseAmount;
							newRot = 0;
							c.ZIndex = HoverZIndex;
							newScale *= 1.2f;
							inspectionRect = new Rect2(newPos + c.Size / 2, newScale * c.Size);
						}
					}
					else
					{
						newPos.Y += SelectedLowerAmount;
						newPos.X += 0;
						// c.Modulate = new Color(Modulate, 0.5f);
					}
				}

				c.currentAnimationTween?.Kill();
				c.currentAnimationTween = null;
				var t = c.CreateTween();
				t.SetParallel(true).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
				t.TweenProperty(c, "position", newPos, 0.2);
				t.TweenProperty(c, "rotation", newRot, 0.2);
				t.TweenProperty(c, "scale", newScale, 0.2);
				c.currentAnimationTween = t;
			}
		}

		// Arrange panels
		ArrangeInspectionPanels(inspectionRect, HoveredCard);
	}
}

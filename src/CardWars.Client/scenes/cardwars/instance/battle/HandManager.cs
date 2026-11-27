using System;
using System.Collections.Generic;
using System.Linq;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.BattleEngine.Vanilla.Features;
using CardWars.Client.scenes.cardwars.instance.battle.entities;
using CardWars.Client.scenes.cardwars.ui.battle;
using CardWars.Client.scenes.cardwars.ui.card_display;
using CardWars.Core.Data.Tags;
using CardWars.Core.Logging;
using CardWars.Core.Registry;
using CardWars.Vanilla.Shared;
using Godot;

namespace CardWars.Client.scenes.cardwars.instance.battle;

public partial class HandManager : Control
{
	public const float HandAngleDeg = 100.0f;
	public const float CardAngleDeg = 180.0f;
	public const float HoverRaiseAmount = 300.0f;
	public const float SelectedLowerAmount = 180.0f;
	public const int IndexGap = 3;
	public int HoverZIndex => _cards.Count * IndexGap;

	public Vector2 CardScale = Vector2.One * 0.25f; // 0.15f;

	public CardDisplay? HoveredCard = null;
	public CardDisplay? SelectedCard = null;
	public LineArrow? LineArrow = null;
	public HashSet<EntityId> ArrowTargets = [];
	public BattleInstance? BattleInstance { get; private set; }

	public void Setup(BattleInstance battleInstance)
	{
		if (BattleInstance is not null)
			throw new InvalidOperationException("Hand manager has already been initialized.");
		BattleInstance = battleInstance;
	}

	private Dictionary<EntityId, CardDisplay> _cards = [];
	public IReadOnlyDictionary<EntityId, CardDisplay> Cards => _cards;

	private Node? _isHoldingCardChanged;

	public override void _Ready()
	{
		Resized += ArrangeCard;
	}

	private void HandleLineArrow()
	{
		if (LineArrow is { } lineArrow)
		{
			if (!(BattleInstance?.Hovered3DTarget is { } target) || !(BattleInstance.CameraNode is { } camera))
			{
				lineArrow.FollowMouse(); return;
			}

			if (ArrowTargets.Contains(target.EntityId))
				lineArrow.SnapTo(camera, target);
			else
				lineArrow.FollowMouse();
		}
	}

	private void HandleEnitity3DPanels()
	{
		if ((BattleInstance?.Hovered3DTarget is { } target) && (BattleInstance.CameraNode is { } camera))
		{
			// FUCKING HELL THIS IS UNHOLY
			if (_isHoldingCardChanged != target)
			{
				BattleInstance.InspectionPanelManagerNode?.ClearInspectionPanels(); _isHoldingCardChanged = target;
			}
			else
			{
				return;
			}

			var id = BattleInstance.EntityNodes.First(s => s.Value == target).Key;
			if ((BattleInstance.State.Get<UnitSlot>(id)?.HoldingCardId) is { } cardId
				&& BattleInstance.State.Get<GenericCard>(cardId) is { } card
				&& BattleInstance.BattleView is { } battleView
				&& BattleInstance.EntityNodes.TryGetValue(cardId, out var cardTarget)
				&& cardTarget is Card3D card3D
			)
			{
				var rect = GetScreenRect(card3D, camera);
				BattleInstance?.InspectionPanelManagerNode?.PopulateAndArrangePanels(
					card,
					battleView,
					BattleInstance.Registry,
					rect
				);
			}
		}
	}

	public override void _Process(double delta)
	{
		// TODO: FIX THIS MESS BROSKI
		// Tbh using process to scan battleinstance's 3d target is such a lazy thing to do so uh yeah
		HandleLineArrow();
		HandleEnitity3DPanels();
	}

	private static Rect2 GetScreenRect(Card3D card, Camera3D camera)
	{
		if (card.CollisionShape is not { } collisionShape) return new();

		var half = card.BoxShape.Size / 2.0f;
		var transform = collisionShape.GlobalTransform;

		var min = new Vector2(float.MaxValue, float.MaxValue);
		var max = new Vector2(float.MinValue, float.MinValue);

		for (var i = 0; i < 8; i++)
		{
			var corner = new Vector3(
				(i & 1) == 0 ? -half.X : half.X,
				(i & 2) == 0 ? -half.Y : half.Y,
				(i & 4) == 0 ? -half.Z : half.Z);

			var screen = camera.UnprojectPosition(transform * corner);
			min = min.Min(screen);
			max = max.Max(screen);
		}

		return new Rect2((min + max) / 2.0f, max - min);
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
		var displayIdN = BattleInstance.BattleView?.DefaultCardDisplay;
		if (displayIdN is not { } displayId) return;
		var card = instance.Registry.Client?.UserInterface.Instantiate<CardDisplay>(displayId);
		if (card is null) return;

		card.Scale = Vector2.Zero;
		card.Position = new(0, 0);

		card.MouseEntered += () => OnCardMouseEntered(card);
		card.MouseExited += () => OnCardMouseExited(card);
		card.GuiInput += (e) => OnCardGuiInput(e, card);
		AddChild(card);
		card.SetData(entity.Data, instance.Registry);
		card.SetDisplay(CardDisplay.DisplayType.Full);
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
						Log.Debug($"Card play rejected: {reason}");
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

	private void ArrangeCard()
	{
		var cards = _cards.Select(s => s.Value).ToList();
		int totalCard = cards.Count;
		if (totalCard == 0)
		{
			HoveredCard = null;
			BattleInstance?.InspectionPanelManagerNode?.ClearInspectionPanels();
			ResetArrows();
			return;
		}

		Vector2 cardSize = Vector2.Zero;
		foreach (var c in cards) { if (c is CardDisplay cd) { cardSize = cd.Size; break; } }
		float cardWidth = cardSize.X * CardScale.X;
		float cardHeight = cardSize.Y * CardScale.Y;

		Vector2 handCenter = new(Size.X / 2, Size.Y /*- cardHeight / 2*/ );

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
					newScale *= 0.8f;

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
		BattleInstance?.InspectionPanelManagerNode?.ClearInspectionPanels();
		if (HoveredCard is not null)
		{
			if (inspectionRect is { } rect)
			{
				if (BattleInstance?.State.Get<GenericCard>(Cards.FirstOrDefault(s => s.Value == HoveredCard).Key) is { } card
					&& BattleInstance.BattleView is { } battleView
					&& BattleInstance.InspectionPanelManagerNode is { } managerNode
				)
				{
					managerNode.ZIndex = HoverZIndex;
					managerNode.PopulatePanels(card, battleView, BattleInstance.Registry);
					managerNode.ArrangePanels(rect);
				}
			}
		}
	}
}

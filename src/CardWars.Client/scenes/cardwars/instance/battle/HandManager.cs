using System;
using System.Collections.Generic;
using System.Linq;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.BattleEngine.Vanilla.Features;
using CardWars.Core.Logging;
using CardWars.Vanilla.Shared;
using Godot;

namespace CardWars.Client.scenes.vanilla.instance.battle;

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
		// TODO: Get skin and place it inside
		var card = BattleInstance?.ClientRegistry?.UserInterface.Instantiate<CardDisplay>(SharedIds.UI.DefaultCardDisplay);
		if (card is null) return;
		card.Scale = Vector2.Zero;
		card.Position = new(0, 0);

		card.MouseEntered += () => OnCardMouseEntered(card);
		card.MouseExited += () => OnCardMouseExited(card);
		card.GuiInput += (e) => OnCardGuiInput(e, card);
		AddChild(card);
		if (BattleInstance?.State.Get<GenericCard>(id) is { } entity && BattleInstance?.ClientRegistry is { } clientRegistry) card.SetData(entity.Data, clientRegistry);
		_cards[id] = card;
		ArrangeCard();
	}

	public void RemoveCard(EntityId id)
	{
		if (_cards.TryGetValue(id, out var c))
		{
			_cards.Remove(id);

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
						LineArrow = BattleInstance?.ClientRegistry?.UserInterface.Instantiate<LineArrow>(SharedIds.UI.LineArrow);
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
		if (totalCard == 0) return;

		Vector2 cardSize = Vector2.Zero;
		foreach (var c in cards) { if (c is CardDisplay cd) { cardSize = cd.Size; break; } }
		float cardWidth = cardSize.X * CardScale.X;
		float cardHeight = cardSize.X * CardScale.X;

		Vector2 handCenter = new(Size.X / 2, Size.Y - cardHeight / 2);

		float margin = 200f;
		float availableWidth = Size.X - margin * 2;

		float overlapFraction = 0.45f;
		float desiredSpacing = cardWidth * (1.0f - overlapFraction);

		float spacing = totalCard > 1 ? Math.Min(desiredSpacing, availableWidth / (totalCard - 1)) : 0;

		float curveHeight = 15.0f;
		float maxRotationDeg = 12.0f;

		float halfRange = (totalCard - 1) / 2.0f;

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
	}
}

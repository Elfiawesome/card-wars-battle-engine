using System;
using System.Collections.Generic;
using System.Linq;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.BattleEngine.Vanilla.Targeting;
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
	public const int HoverZIndex = 10;

	public Vector2 CardScale = Vector2.One * 0.15f;

	public CardDisplay? HoveredCard = null;
	public CardDisplay? SelectedCard = null;
	public Node2D? LineArrow = null;
	public HashSet<EntityId> ArrowTargets = [];
	public BattleInstance? BattleInstance = null;

	// private List<CardDisplay> _cards = [];
	private Dictionary<EntityId, CardDisplay> _cards = [];
	public IReadOnlyDictionary<EntityId, CardDisplay> Cards => _cards;

	public override void _Ready()
	{
		Resized += ArrangeCard;
	}

	public override void _Process(double delta)
	{
		if (LineArrow is not { } lineArrow) return;

		if (BattleInstance?.Hovered3DTarget.Node is { } target3D
			&& BattleInstance.CameraNode is { } camera)
		{
			var viewportPoint = camera.UnprojectPosition(target3D.GlobalPosition);
			var localPoint = lineArrow.GetGlobalTransformWithCanvas().AffineInverse() * viewportPoint;
			lineArrow.Set("target", localPoint);
			lineArrow.Set("smoothness", 0.2);
		}
		else
		{
			lineArrow.Set("smoothness", 0.8);
			lineArrow.Set("target", lineArrow.GetLocalMousePosition());
		}
	}


	public CardDisplay? GetCard(EntityId entityId) => _cards.TryGetValue(entityId, out var card) ? card : null;

	public void AddCard(EntityId id)
	{
		var card = BattleInstance?.ClientRegistry?.UserInterface.Instantiate<CardDisplay>(SharedIds.UI.CardDisplay);
		if (card is null) return;
		card.Scale = Vector2.Zero;
		card.Position = new(0, 0);

		card.MouseEntered += () => OnCardMouseEntered(card);
		card.MouseExited += () => OnCardMouseExited(card);
		card.GuiInput += (e) => OnCardGuiInput(e, card);
		AddChild(card);
		_cards[id] = card;
		ArrangeCard();
	}

	public void RemoveCard(EntityId id)
	{
		if (_cards.TryGetValue(id, out var c))
		{
			_cards.Remove(id);
			c.QueueFree(); // TODO: Make it animate away
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
						SelectedCard = card;

						LineArrow?.QueueFree();
						LineArrow = BattleInstance?.ClientRegistry?.UserInterface.Instantiate<Node2D>(SharedIds.UI.LineArrow);
						AddChild(LineArrow);
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
			if (inputEventMouseButton.Pressed)
			{
				if (SelectedCard is not null)
				{
					SelectedCard = null;

					LineArrow?.QueueFree();
					LineArrow = null;
				}
			}
			ArrangeCard();
		}
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
						else
						{
							c.ZIndex = 0;
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

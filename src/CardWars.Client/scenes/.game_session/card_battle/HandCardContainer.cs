using System;
using Godot;

namespace CardWars.Client;

public partial class HandCardContainer : Control
{
	public const float HoverRaiseAmount = 100;
	public const int HoverZIndex = 10;
	PackedScene CardDisplayScene = GD.Load<PackedScene>("res://scenes/game_session/card_battle/card_display.tscn");
	CardDisplay? hoveredCard = null;

	public override void _Ready()
	{
		Resized += ArrangeCard;
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey inputEventKey)
		{
			if (inputEventKey.Keycode == Key.Q && inputEventKey.Pressed)
			{
				AddCard();
			}
		}
	}

	public void AddCard()
	{
		var card = CardDisplayScene.Instantiate<CardDisplay>();
		AddChild(card);
		card.OnCardMouseEntered += (c) => { hoveredCard = c; ArrangeCard(); };
		card.OnCardMouseExited += (c) => { hoveredCard = null; ArrangeCard(); };
		ArrangeCard();
	}

	public void ArrangeCard()
	{
		var cards = GetChildren();
		int totalCard = cards.Count;
		if (totalCard == 0) return;

		Vector2 handCenter = new(Size.X / 2, Size.Y - 60);

		Vector2 cardSize = ((CardDisplay)cards[0]).Size;
		float cardWidth = cardSize.X;

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

				if (c == hoveredCard)
				{
					newPos.Y -= HoverRaiseAmount;
					newRot = 0;
					c.ZIndex = HoverZIndex;
				}
				else
				{
					c.ZIndex = 0;
				}

				c.currentAnimationTween?.Kill();
				c.currentAnimationTween = null;
				var t = c.CreateTween();
				t.SetParallel(true).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.InOut);
				t.TweenProperty(c, "position", newPos, 0.2);
				t.TweenProperty(c, "rotation", newRot, 0.2);
				c.currentAnimationTween = t;
			}
		}
	}
}
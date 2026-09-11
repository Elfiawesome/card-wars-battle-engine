using System;
using System.Collections.Generic;
using CardWars.BattleEngine.State;
using CardWars.Vanilla.Shared;
using Godot;

namespace CardWars.Client.scenes.vanilla.instance.battle;

public partial class HandManager : Control
{
	public const float HandAngleDeg = 100.0f;
	public const float CardAngleDeg = 180.0f;
	public const float HoverRaiseAmount = 60.0f;
	public const int HoverZIndex = 10;

	public Vector2 CardScale = Vector2.One * 0.15f;

	public CardDisplay? HoveredCard = null;
	public BattleInstance? BattleInstance = null;

	private List<CardDisplay> _cards = [];
	private Dictionary<EntityId, CardDisplay> _cardsIdMap = [];

	public override void _Ready()
	{
		Resized += ArrangeCard;
	}


	public CardDisplay? GetCard(EntityId entityId) => _cardsIdMap.TryGetValue(entityId, out var card) ? card : null;

	public void AddCard(EntityId id)
	{
		var card = BattleInstance?.ClientRegistry?.UserInterface.Instantiate<CardDisplay>(SharedIds.CardDisplay);
		if (card == null) return;
		card.Scale = Vector2.Zero;
		card.Position = new(0, 0);

		card.MouseEntered += () => OnCardMouseEntered(card);
		card.MouseExited += () => OnCardMouseExited(card);
		AddChild(card);
		_cards.Add(card);
		_cardsIdMap[id] = card;
		ArrangeCard();
	}

	public void RemoveCard(EntityId id)
	{
		
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

	private void ArrangeCard()
	{
		var cards = _cards;
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

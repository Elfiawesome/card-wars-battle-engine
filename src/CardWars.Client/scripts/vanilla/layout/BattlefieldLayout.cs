using System.Collections.Generic;
using System.Linq;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Client.scenes.vanilla.instance.battle;
using Godot;

namespace CardWars.Client.scripts.vanilla.layout;

public abstract class BattlefieldLayout : IBattleLayoutHandler
{
	public abstract void Compute(BattleInstance battle);

	protected static BattlefieldNode? GetNode(BattleInstance battle, EntityId id)
	{
		if (battle.GetEntityNode<BattlefieldNode>(id) is not { } node) return null;
		node.RefreshBounds();
		return node;
	}

	protected static float RowWidth(List<BattlefieldNode> row, float gap)
		=> row.Sum(node => node.Width) + gap * Mathf.Max(0, row.Count - 1);

	protected static void PlaceRow(BattleInstance battle, List<BattlefieldNode> row, Vector3 rowCenter, float facing, float gap)
	{
		var tangent = new Vector3(Mathf.Cos(facing), 0f, -Mathf.Sin(facing));
		var cursor = -RowWidth(row, gap) * 0.5f;
		foreach (var node in row)
		{
			var offset = cursor + node.Width * 0.5f;
			cursor += node.Width + gap;
			node.PlaceAt(rowCenter + tangent * offset, facing);
			PlaceHeroes(battle, node);
		}
	}

	protected static void PlaceHeroes(BattleInstance battle, BattlefieldNode node)
	{
		if (battle.State.Get<Battlefield>(node.EntityId) is not { } battlefield) return;
		if (battlefield.HeroCardIds.Count == 0) return;

		var cards = new List<Card3D>();
		foreach (var heroId in battlefield.HeroCardIds)
		{
			if (battle.GetEntityNode<Card3D>(heroId) is { } card)
				cards.Add(card);
		}

		if (cards.Count == 0) return;

		var bounds = cards[0].GetHighlightBounds();
		var width = bounds?.Size.X ?? 1.1f;
		var depth = bounds?.Size.Y ?? 0.82f;
		var spacing = width + 0.15f;
		var gap = battle.State.Layout.LayoutConfig.GetFloat("hero_gap", 0.3f);
		var z = node.BoundsCenter.Z + node.Depth * 0.5f + gap;

		for (var i = 0; i < cards.Count; i++)
		{
			var x = (i - (cards.Count - 1) * 0.5f) * spacing;
			cards[i].Position = new Vector3(x, 0.02f, z);
			cards[i].Rotation = new Vector3(0f, Mathf.Pi / 2f, 0f);
		}
	}
}

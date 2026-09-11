using System.Collections.Generic;
using System.Linq;
using CardWars.BattleEngine.State;
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

	protected static void PlaceRow(List<BattlefieldNode> row, Vector3 rowCenter, float facing, float gap)
	{
		var tangent = new Vector3(Mathf.Cos(facing), 0f, -Mathf.Sin(facing));
		var cursor = -RowWidth(row, gap) * 0.5f;
		foreach (var node in row)
		{
			var offset = cursor + node.Width * 0.5f;
			cursor += node.Width + gap;
			node.PlaceAt(rowCenter + tangent * offset, facing);
		}
	}
}

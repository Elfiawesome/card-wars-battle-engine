using System.Collections.Generic;
using System.Linq;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Client.scenes.vanilla.instance.battle;
using Godot;

namespace CardWars.Client.scripts.vanilla.layout;

public sealed class LineLayout : BattlefieldLayout
{
	private const float DefaultGap = 1f;

	public override void Compute(BattleInstance battle)
	{
		var state = battle.State;
		var config = state.Layout.LayoutConfig;
		var gap = config.GetFloat("gap", DefaultGap);

		var teams = state.OfType<Player>()
			.GroupBy(player => player.Team)
			.OrderBy(group => group.Key)
			.ToList();

		var nearDepth = 0f;
		var farDepth = 0f;

		for (var teamIndex = 0; teamIndex < teams.Count; teamIndex++)
		{
			var row = new List<BattlefieldNode>();
			foreach (var player in teams[teamIndex])
				foreach (var battlefieldId in player.BattlefieldIds)
					if (GetNode(battle, battlefieldId) is { } node) row.Add(node);
			if (row.Count == 0) continue;

			var rowDepth = row.Max(node => node.Depth);
			var nearSide = teamIndex % 2 == 0;
			var rowCenterZ = nearSide ? nearDepth + rowDepth * 0.5f : -(farDepth + rowDepth * 0.5f);
			if (nearSide) nearDepth += rowDepth + gap;
			else farDepth += rowDepth + gap;

			PlaceRow(row, new Vector3(0f, 0f, rowCenterZ), nearSide ? 0f : Mathf.Pi, gap);
		}
	}
}

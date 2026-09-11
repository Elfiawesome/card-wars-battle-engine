using System;
using System.Collections.Generic;
using System.Linq;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Client.scenes.vanilla.instance.battle;
using CardWars.Core.Data.Tags;
using Godot;

namespace CardWars.Client.scripts.vanilla.layout;

public sealed class DefaultLayout : BattlefieldLayout
{
	private const float DefaultGap = 1f;

	public override void Compute(BattleInstance battle)
	{
		var state = battle.State;
		var config = state.Layout.LayoutConfig;
		var gap = config.GetFloat("gap", DefaultGap);
		var teamGap = config.GetFloat("team_gap", gap * 2f);
		var centerIds = ReadCenterIds(config);

		var centered = new List<BattlefieldNode>();
		var teams = new List<List<BattlefieldNode>>();

		foreach (var team in state.OfType<Player>().GroupBy(player => player.Team).OrderBy(group => group.Key))
		{
			var row = new List<BattlefieldNode>();
			foreach (var player in team)
				foreach (var battlefieldId in player.BattlefieldIds)
				{
					if (GetNode(battle, battlefieldId) is not { } node) continue;
					if (centerIds.Contains(battlefieldId)) centered.Add(node);
					else row.Add(node);
				}

			if (row.Count > 0) teams.Add(row);
		}

		var centerRadius = 0f;
		foreach (var node in centered)
		{
			node.PlaceAt(Vector3.Zero, 0f);
			centerRadius = Mathf.Max(centerRadius, node.Radius);
		}

		if (teams.Count == 0) return;
		if (teams.Count == 1)
		{
			PlaceRow(teams[0], Vector3.Zero, 0f, gap);
			return;
		}

		// Every team owns an arc proportional to its total width, so a wider
		// team curves around the ring instead of spilling past its neighbours.
		var arcLength = teams.Sum(row => RowWidth(row, gap) + teamGap);
		var maxDepth = teams.Max(row => row.Max(node => node.Depth));

		var radius = Mathf.Max(arcLength / Mathf.Tau, maxDepth * 0.5f + gap * 0.5f);
		if (centerRadius > 0f)
			radius = Mathf.Max(radius, centerRadius + maxDepth * 0.5f + gap);

		var startAngle = Mathf.Pi * 0.5f - Mathf.Tau * (RowWidth(teams[0], gap) * 0.5f) / arcLength;
		var cursor = 0f;

		foreach (var row in teams)
		{
			var rowStart = cursor;
			var inner = 0f;

			foreach (var node in row)
			{
				var arc = rowStart + inner + node.Width * 0.5f;
				var angle = startAngle + Mathf.Tau * arc / arcLength;
				var normal = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
				node.PlaceAt(normal * radius, Mathf.Pi * 0.5f - angle);
				inner += node.Width + gap;
			}

			cursor = rowStart + RowWidth(row, gap) + teamGap;
		}
	}

	private static HashSet<EntityId> ReadCenterIds(CompoundTag config)
	{
		var ids = new HashSet<EntityId>();
		if (config.Get("center") is StringTag single && Guid.TryParse(single.Value, out var id))
			ids.Add(new EntityId(id));
		if (config.GetList("center") is { } list)
			foreach (var item in list.Items)
				if (item is StringTag text && Guid.TryParse(text.Value, out var parsed))
					ids.Add(new EntityId(parsed));
		return ids;
	}
}

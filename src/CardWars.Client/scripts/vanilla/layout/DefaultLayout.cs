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
	private const float DefaultGap = 0.3f;
	private const float DefaultCurvature = 0.2f;
	private const float DefaultFacingCurve = 0.2f;

	public override void Compute(BattleInstance battle)
	{
		var state = battle.State;
		var config = state.Layout.LayoutConfig;
		var gap = config.GetFloat("gap", DefaultGap);
		var teamGap = config.GetFloat("team_gap", gap * 2f);
		var curvature = Mathf.Clamp(config.GetFloat("curvature", DefaultCurvature), 0f, 1f);
		var facingCurve = Mathf.Clamp(config.GetFloat("facing_curve", DefaultFacingCurve), 0f, 1f);
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

		// Each team is a curved side of a regular polygon. The distance to each
		// side is derived from the widest team so no team spills past its
		// neighbours; the bow (curvature) bends the side itself.
		var sideCount = teams.Count;
		var apothem = 0f;

		foreach (var row in teams)
		{
			var half = RowWidth(row, gap) * 0.5f;
			var rowDepth = row.Max(node => node.Depth);
			var need = curvature * half + rowDepth * 0.5f + gap * 0.5f;

			if (sideCount >= 3)
				need = Mathf.Max(need, half / Mathf.Tan(Mathf.Pi / sideCount) + teamGap);
			if (centerRadius > 0f)
				need = Mathf.Max(need, centerRadius + rowDepth * 0.5f + gap);

			apothem = Mathf.Max(apothem, need);
		}

		for (var i = 0; i < sideCount; i++)
		{
			var row = teams[i];
			var half = RowWidth(row, gap) * 0.5f;
			var bow = curvature * half;

			var angle = Mathf.Pi * 0.5f + Mathf.Tau * i / sideCount;
			var normal = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
			var tangent = new Vector3(Mathf.Sin(angle), 0f, -Mathf.Cos(angle));

			// facing_curve: 0 = every battlefield faces along the line (the
			// team's overall facing), 1 = each one turns to follow the curve.
			var lineFacing = Mathf.Pi * 0.5f - angle;

			var inner = -half;
			foreach (var node in row)
			{
				var offset = inner + node.Width * 0.5f;
				inner += node.Width + gap;

				var bowOffset = half > 0f ? -bow * (offset / half) * (offset / half) : 0f;
				var position = (apothem + bowOffset) * normal + offset * tangent;
				var curveFacing = Mathf.Atan2(position.X, position.Z);
				node.PlaceAt(position, Mathf.LerpAngle(lineFacing, curveFacing, facingCurve));
			}
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

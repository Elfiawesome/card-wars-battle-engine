using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;

namespace CardWars.BattleEngine.Vanilla.Attacks;

public enum AttackPatternType { Row, Column, All, Cell }

public readonly record struct AttackDamageSpec(
	[property: DataTag] float Percent = 0.0f,
	[property: DataTag] int Amount = 0
)
{
	public int Resolve(int atkValue)
		// Take the highest of each pattern's attack spec
		=> Math.Max(
			(int)Math.Round(atkValue * Percent),
			Amount
		);
}

public readonly record struct AttackPattern(
	[property: DataTag] AttackPatternType Type,
	[property: DataTag] UnitSlotPos Offset,
	[property: DataTag] AttackDamageSpec Damage
);

public sealed record AttackDefinition(
	[property: DataTag] IReadOnlyList<AttackPattern> Patterns,
	[property: DataTag] ResourceId Texture,
	[property: DataTag] string Name = "BA Atk",
	[property: DataTag] int ChargeCost = 1,
	[property: DataTag] bool Bypass = false
)
{
	public static AttackDefinition DefaultBasic { get; } = new(
		[
			new AttackPattern(
			AttackPatternType.Cell,
			new UnitSlotPos(0, 0),
			new AttackDamageSpec(1.0f, 0))
		],
		"cardwars:texture/icon/stat/atk"
	);
}


public sealed record AttackProfile
{
	public AttackDefinition Definition;
	public bool IsSpAtk; // TODO: Wtf is this for???

	public AttackProfile(bool isSpAtk, AttackDefinition? definition = null)
	{
		Definition = definition ?? AttackDefinition.DefaultBasic;
		IsSpAtk = isSpAtk;
	}

	public bool IsHit(out AttackDamageSpec attackDamageSpec)
		=> IsHit(new(0, 0), new(0, 0), out attackDamageSpec);

	public bool IsHit(UnitSlotPos initialTargetPos, UnitSlotPos targetPos, out AttackDamageSpec attackDamageSpec)
	{
		bool hit = false;
		float percent = 0f;
		int amount = 0;

		foreach (var pattern in Definition.Patterns)
		{
			if (!PatternMatches(pattern, initialTargetPos, targetPos))
				continue;

			if (!hit)
			{
				percent = pattern.Damage.Percent;
				amount = pattern.Damage.Amount;
				hit = true;
			}
			else
			{
				percent = Math.Max(percent, pattern.Damage.Percent);
				amount = Math.Max(amount, pattern.Damage.Amount);
			}
		}

		attackDamageSpec = hit ? new AttackDamageSpec(percent, amount) : default;
		return hit;
	}

	private static bool PatternMatches(AttackPattern pattern, UnitSlotPos origin, UnitSlotPos target)
	{
		return pattern.Type switch
		{
			// Single cell at origin + offset
			AttackPatternType.Cell =>
				target.X == origin.X + pattern.Offset.X &&
				target.Y == origin.Y + pattern.Offset.Y,

			// Entire horizontal row: offset applies to Y only
			AttackPatternType.Row =>
				target.Y == origin.Y + pattern.Offset.Y,

			// Entire vertical column: offset applies to X only
			AttackPatternType.Column =>
				target.X == origin.X + pattern.Offset.X,

			// Everything, offset is ignored
			AttackPatternType.All => true,

			_ => false
		};
	}

	public RenderedAttackPatternGrid RenderPattern(int targetSize = 3)
	{
		List<RenderedAttackPatternCell> inspectionGridCells = [];

		int Midpoint = targetSize / 2;
		for (var y = 0; y < targetSize; y++)
		{
			for (var x = 0; x < targetSize; x++)
			{
				var isCenter = Midpoint == x && Midpoint == y;
				var text = "";
				var filled = false;
				var crossed = false;
				if (IsHit(new UnitSlotPos(Midpoint, Midpoint), new UnitSlotPos(x, y), out var spec))
				{
					crossed = isCenter;
					filled = true;
					text = isCenter ? "" :
						spec.Percent != 0.0 ? $"{(int)Math.Round(spec.Percent * 100)}%" :
						spec.Amount != 0.0 ? $"{spec.Amount}" :
						string.Empty;
				}
				inspectionGridCells.Add(
					new RenderedAttackPatternCell(spec, filled, crossed, text)
				);
			}
		}
		return new RenderedAttackPatternGrid(targetSize, inspectionGridCells);
	}

	public sealed record class RenderedAttackPatternGrid(
		int Width,
		IReadOnlyList<RenderedAttackPatternCell> Cells
	);

	public sealed record class RenderedAttackPatternCell(
		AttackDamageSpec? Spec = null,
		bool Filled = false,
		bool Crossed = false,
		string Text = ""
	);
}
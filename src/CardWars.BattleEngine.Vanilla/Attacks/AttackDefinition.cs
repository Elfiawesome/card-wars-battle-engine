using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Tags;

namespace CardWars.BattleEngine.Vanilla.Attacks;

public enum AttackPatternType { Row, Column, All, Cell }

public readonly record struct AttackDamageSpec(
	float Percent = 0.0f,
	int Amount = 0
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
	[property: DataTag] string Name = "BA Atk",
	[property: DataTag] int ChargeCost = 1,
	[property: DataTag] bool Bypass = false
);


public sealed record AttackProfile
{
	public string Name;
	public int ChargeCost;
	public bool Bypass;
	public IReadOnlyList<AttackPattern> Patterns;
	public bool IsSpAtk;

	public AttackProfile(string name, int chargeCost, bool bypass, IReadOnlyList<AttackPattern> patterns, bool isSpAtk)
	{
		Name = name;
		ChargeCost = chargeCost;
		Bypass = bypass;
		Patterns = patterns;
		IsSpAtk = isSpAtk;
	}

	public AttackProfile(bool isSpAtk, AttackDefinition? definition = null)
	{
		definition ??= new AttackDefinition([]);
		Name = definition.Name;
		ChargeCost = definition.ChargeCost;
		Bypass = definition.Bypass;
		Patterns = definition.Patterns;
		IsSpAtk = isSpAtk;
	}

	public bool IsHit(out AttackDamageSpec attackDamageSpec)
		=> IsHit(new(0, 0), new(0, 0), out attackDamageSpec);

	public bool IsHit(UnitSlotPos initialTargetPos, UnitSlotPos targetPos, out AttackDamageSpec attackDamageSpec)
	{
		bool hit = false;
		float percent = 0f;
		int amount = 0;

		foreach (var pattern in Patterns)
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
}
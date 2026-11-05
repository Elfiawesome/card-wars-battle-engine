using CardWars.Core.Data.Tags;
using CardWars.Core.Logging;

namespace CardWars.BattleEngine.Vanilla.Attacks;

public enum AttackPatternType
{
	Single,
	Row,
	Column,
	All,
	Cell
}

public readonly record struct AttackDamageSpec(bool IsPercent, int Value)
{
	public int Resolve(int attack)
		=> IsPercent ? attack * Value / 100 : Value;
}

public readonly record struct AttackPattern(
	AttackPatternType Type,
	int X,
	int Y,
	AttackDamageSpec Damage
);

public sealed record AttackDefinition(
	string Name,
	int ChargeCost,
	bool Bypass,
	IReadOnlyList<AttackPattern> Patterns
)
{
	public static AttackDefinition DefaultBasic { get; } = new("Basic Attack", 1, false, []);
}

public sealed record AttackProfile(
	string Name,
	int ChargeCost,
	bool Bypass,
	IReadOnlyList<AttackPattern> Patterns,
	bool IsSpAtk
);

public static class AttackDefinitionParser
{
	public static bool TryParse(CompoundTag tag, string fallbackName, out AttackDefinition definition)
	{
		definition = AttackDefinition.DefaultBasic;

		var name = tag.GetString("name", fallbackName);
		var chargeCost = Math.Max(0, tag.GetInt("charge_cost", 1));
		var bypass = tag.GetBool("bypass");
		var patterns = new List<AttackPattern>();

		if (tag.GetList("patterns") is { } patternList)
		{
			foreach (var item in patternList.Items)
			{
				if (item is not CompoundTag patternTag)
				{
					Log.Warn($"Attack [{name}] pattern must be a compound");
					continue;
				}

				if (!TryParsePatternType(patternTag.GetString("type"), out var type))
				{
					Log.Warn($"Attack [{name}] has unknown pattern type [{patternTag.GetString("type")}]");
					continue;
				}

				if (!TryParseDamage(patternTag.Get("value"), out var damage))
				{
					Log.Warn($"Attack [{name}] has invalid pattern value");
					continue;
				}

				patterns.Add(new AttackPattern(
					type,
					patternTag.GetInt("x"),
					patternTag.GetInt("y"),
					damage));
			}
		}

		definition = new AttackDefinition(name, chargeCost, bypass, patterns);
		return true;
	}

	private static bool TryParsePatternType(string value, out AttackPatternType type)
	{
		switch (value.Trim().ToLowerInvariant())
		{
			case "single": type = AttackPatternType.Single; return true;
			case "row": type = AttackPatternType.Row; return true;
			case "column": type = AttackPatternType.Column; return true;
			case "all": type = AttackPatternType.All; return true;
			case "cell": type = AttackPatternType.Cell; return true;
			default: type = default; return false;
		}
	}

	private static bool TryParseDamage(DataTag? tag, out AttackDamageSpec damage)
	{
		damage = default;

		switch (tag)
		{
			case IntTag integer when integer.Value >= 0:
				damage = new AttackDamageSpec(false, integer.Value);
				return true;
			case FloatTag number when number.Value >= 0f:
				damage = new AttackDamageSpec(false, (int)MathF.Floor(number.Value));
				return true;
			case StringTag text:
				return TryParseTextDamage(text.Value, out damage);
			default:
				return false;
		}
	}

	private static bool TryParseTextDamage(string text, out AttackDamageSpec damage)
	{
		damage = default;
		text = text.Trim();

		if (text.EndsWith('%'))
		{
			if (!int.TryParse(text[..^1], out var percent) || percent < 0) return false;
			damage = new AttackDamageSpec(true, percent);
			return true;
		}

		if (!int.TryParse(text, out var flat) || flat < 0) return false;
		damage = new AttackDamageSpec(false, flat);
		return true;
	}
}

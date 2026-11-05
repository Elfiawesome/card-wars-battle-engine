using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Data.Tags;
using CardWars.Core.Logging;
using CardWars.Core.Registry;

namespace CardWars.BattleEngine.Vanilla.Attacks;

public sealed class AttackResolver(BattleEngineRegistry registry)
{
	public AttackProfile Resolve(GenericCard card)
	{
		if (card.Data.GetList("sp_atk") is { Count: > 0 } spAtks)
		{
			var definitions = new List<AttackDefinition>();

			foreach (var item in spAtks.Items)
			{
				if (TryResolve(item, "SP ATK", out var definition))
					definitions.Add(definition);
			}

			if (definitions.Count > 0)
			{
				return new AttackProfile(
					string.Join(" + ", definitions.Select(definition => definition.Name)),
					definitions.Sum(definition => definition.ChargeCost),
					definitions.Any(definition => definition.Bypass),
					definitions.SelectMany(definition => definition.Patterns).ToList(),
					true);
			}

			Log.Warn($"Card [{card.Id}] has no valid sp_atk definitions, falling back to ba_atk");
		}

		if (TryResolve(card.Data.Get("ba_atk"), "Basic Attack", out var basic))
			return FromDefinition(basic, false);

		return FromDefinition(AttackDefinition.DefaultBasic, false);
	}

	public bool CanAfford(GenericCard card, AttackProfile attack)
		=> card.Charge >= attack.ChargeCost;

	private bool TryResolve(DataTag? tag, string fallbackName, out AttackDefinition definition)
	{
		definition = AttackDefinition.DefaultBasic;

		switch (tag)
		{
			case StringTag reference:
				var id = ResourceId.Parse(reference.Value);
				var raw = registry.AttackDefinitions.Get(id);
				if (raw is null)
				{
					Log.Warn($"Attack definition [{id}] was not found");
					return false;
				}

				return AttackDefinitionParser.TryParse(raw, fallbackName, out definition);

			case CompoundTag inline:
				return AttackDefinitionParser.TryParse(inline, fallbackName, out definition);

			default:
				return false;
		}
	}

	private static AttackProfile FromDefinition(AttackDefinition definition, bool isSpAtk)
		=> new(
			definition.Name,
			definition.ChargeCost,
			definition.Bypass,
			definition.Patterns,
			isSpAtk);
}

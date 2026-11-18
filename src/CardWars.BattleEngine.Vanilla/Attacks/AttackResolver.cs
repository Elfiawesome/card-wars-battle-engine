using CardWars.BattleEngine.Rules;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Data.Mapping;
using CardWars.Core.Data.Tags;
using CardWars.Core.Logging;

namespace CardWars.BattleEngine.Vanilla.Attacks;

public sealed class AttackResolver
{
	public AttackProfile Resolve(GenericCard card)
	{
		if (card.Data.GetList("sp_atks") is { Count: > 0 } spAtks)
		{
			var definitions = new List<AttackDefinition>();

			foreach (var item in spAtks.Items)
			{
				if (item is CompoundTag compoundTag)
				{
					var def = DataTagMapper.FromTag<AttackDefinition>(compoundTag);
					definitions.Add(def);
				}
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

		// Get the custom BA (if have any)
		if (card.Data.Get("ba_atk") is CompoundTag baAtk)
		{
			var def = DataTagMapper.FromTag<AttackDefinition>(baAtk);
			return new AttackProfile(false, def);
		}

		// Default
		return new AttackProfile(false, AttackDefinition.DefaultBasic);
	}

	public bool CanAfford(GenericCard card, AttackProfile attack)
		=> card.Charge >= attack.ChargeCost;
}

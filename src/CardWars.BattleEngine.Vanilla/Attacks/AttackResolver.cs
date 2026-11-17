using CardWars.BattleEngine.Rules;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Data;
using CardWars.Core.Data.Tags;
using CardWars.Core.Logging;

namespace CardWars.BattleEngine.Vanilla.Attacks;

public sealed class AttackResolver(BattleEngineRegistry registry, DataContext data, ILogger? logger = null)
{
	private readonly ILogger _logger = logger ?? NullLogger.Instance;

	public AttackProfile Resolve(GenericCard card)
	{
		if (card.Data.GetList("sp_atks") is { Count: > 0 } spAtks)
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

			_logger.Warn($"Card [{card.Id}] has no valid sp_atk definitions, falling back to ba_atk");
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
		if (tag is not CompoundTag pointerTag) return false;

		ContentPointer pointer;
		try
		{
			pointer = data.FromTag<ContentPointer>(pointerTag);
		}
		catch (Exception ex)
		{
			_logger.Warn($"Could not parse attack pointer for [{fallbackName}]: {ex.Message}");
			return false;
		}

		if (pointer.Resource is { } id)
		{
			var raw = registry.AttackDefinitions.Get(id);
			if (raw is null)
			{
				_logger.Warn($"Attack definition [{id}] was not found");
				return false;
			}

			return AttackDefinitionParser.TryParse(raw, fallbackName, out definition, _logger);
		}

		return pointer.Definition is { } inline
			&& AttackDefinitionParser.TryParse(inline, fallbackName, out definition, _logger);
	}

	private static AttackProfile FromDefinition(AttackDefinition definition, bool isSpAtk)
		=> new(
			definition.Name,
			definition.ChargeCost,
			definition.Bypass,
			definition.Patterns,
			isSpAtk);
}

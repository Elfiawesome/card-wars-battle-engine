namespace CardWars.BattleEngine.Definition;

public record UnitDefinition(
	string Name,
	string WorldDefinitionId,
	string FlavorText,
	int BaseHp,
	int BaseAtk,
	int BasePt,
	List<string> AbilityDefinitionIds
);
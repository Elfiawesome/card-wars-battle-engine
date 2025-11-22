namespace CardWars.BattleEngine.Definition;

public sealed class DefinitionService
{
	public readonly Dictionary<string, WorldDefinition> WorldDefinitions = [];
	public readonly Dictionary<string, UnitDefinition> UnitDefinitions = [];
	public readonly Dictionary<string, AbilityDefinition> AbilityDefinitions = [];

	public DefinitionService()
	{
		// Should be loaded via file. But we just write it here first TODO!
		WorldDefinitions.Add("cardwars:destiny_2", new("Destiny 2"));
		WorldDefinitions.Add("cardwars:genshin_impact", new("Genshin Impact"));
		WorldDefinitions.Add("cardwars:plant_vs_zombies", new("Plant Vs Zombies"));
		WorldDefinitions.Add("cardwars:red_alert_3", new("Red Alert 3"));

		// Destiny 2
		UnitDefinitions.Add("cardwars:destiny_2/harpy", new(
			"Harpy", "cardwars:destiny_2",
			"Harpies fly rapidly in and out of combat, opening up their shells and revealing a writhing mass of filaments.",
			3, 1, 1, []
		));
		UnitDefinitions.Add("cardwars:destiny_2/goblin", new(
			"Goblin", "cardwars:destiny_2",
			"As with all members of the Vex collective, Goblins share a single mind. This allows them to coordinate their swarming assaults on Guardians.",
			10, 6, 3, []
		));
		UnitDefinitions.Add("cardwars:destiny_2/hobgoblin", new(
			"Hobgoblin", "cardwars:destiny_2",
			"Hobgoblins are hardened Vex units fitted with improved optics and acute sensors in their horns.",
			12, 15, 12, []
		));

		// Genshin Impact
		UnitDefinitions.Add("cardwars:genshin_impact/hutao", new(
			"Hu Tao", "cardwars:genshin_impact",
			"\"Pyre, pyre, pants on fire!\"",
			22, 15, 13, []
		));

		// Plant Vs Zombies
		UnitDefinitions.Add("cardwars:plant_vs_zombies/cob_cannon", new(
			"Cob Cannon", "cardwars:plant_vs_zombies",
			"A corn cannon.",
			20, 15, 20, []
		));

		// Red Alert 3
		UnitDefinitions.Add("cardwars:red_alert_3/spy", new(
			"Spy", "cardwars:red_alert_3",
			"The spies deployed by the Allies to reconnoiter behind enemy lines and relay information back to their Allied masters.",
			15, 0, 8, []
		));

	}
}
namespace CardWars.BattleEngine.Definitions;

public class DefinitionLibrary
{
	public Dictionary<string, UnitDefinition> Units = [];

	public DefinitionLibrary()
	{
		Units.Add("archer", new UnitDefinition()
		{
			Name = "Archer",
			FlavorText = "A swift and precise marksman.",
			Hp = 15,
			Atk = 5,
			Pt = 2,
		});
		Units.Add("warrior", new UnitDefinition()
		{
			Name = "Warrior",
			FlavorText = "A formidable frontline combatant.",
			Hp = 30,
			Atk = 10,
			Pt = 5,
		});
		Units.Add("mage", new UnitDefinition()
		{
			Name = "Mage",
			FlavorText = "A master of arcane arts.",
			Hp = 10,
			Atk = 8,
			Pt = 3,
		});
		Units.Add("rogue", new UnitDefinition()
		{
			Name = "Rogue",
			FlavorText = "A cunning and agile infiltrator.",
			Hp = 20,
			Atk = 7,
			Pt = 4,
		});
		Units.Add("healer", new UnitDefinition()
		{
			Name = "Healer",
			FlavorText = "A devoted practitioner of restorative magic.",
			Hp = 18,
			Atk = 3,
			Pt = 3,
		});
	}
}

public class UnitDefinition
{
	public string Name { get; set; } = "";
	public string FlavorText { get; set; } = "";
	public int Hp { get; set; } = 0;
	public int Atk { get; set; } = 0;
	public int Pt { get; set; } = 0;
}
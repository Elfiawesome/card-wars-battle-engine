namespace CardWars.BattleEngine.Definitions;

public class DefinitionLibrary
{
	public Dictionary<string, UnitDefinition> Units = [];

	public DefinitionLibrary()
	{
		Units.Add("archer", new UnitDefinition()
		{
			Name = "Archer Me",
			FlavorText = "A simple archer",
			Hp = 15,
			Atk = 5,
			Pt = 2,
		});
		Units.Add("warrior", new UnitDefinition()
		{
			Name = "Warrior",
			FlavorText = "A simple Warrir",
			Hp = 30,
			Atk = 15,
			Pt = 10,
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
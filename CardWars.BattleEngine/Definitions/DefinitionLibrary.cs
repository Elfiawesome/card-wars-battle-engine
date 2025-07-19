namespace CardWars.BattleEngine.Definitions;

public class DefinitionLibrary
{
	public DefinitionLibrary()
	{
	}

	public void LoadAllGameData(string rootDataPath)
	{
		string heroesPath = Path.Combine(rootDataPath, "Heroes");
		string unitsPath = Path.Combine(rootDataPath, "Units");

		if (!Directory.Exists(unitsPath))
		{
			return;
		}

		string[] files = Directory.GetFiles(unitsPath, "*.json");
		foreach (string filePath in files)
		{
			string jsonString = File.ReadAllText(filePath);
			Console.WriteLine(jsonString);
		}
	}
}
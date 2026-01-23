using System.Reflection;

namespace CardWars.ModLoader;

public class ModLoader
{
	public static void LoadMod()
	{
		var dll = Assembly.LoadFile(@"C:\Users\elfia\OneDrive\Desktop\CardWars\src\CardWars.BattleEngine.Vanilla\bin\Debug\net9.0\CardWars.BattleEngine.Vanilla.dll");
		
	}
}


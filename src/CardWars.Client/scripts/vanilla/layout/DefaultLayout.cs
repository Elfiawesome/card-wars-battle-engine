using CardWars.Client.scenes.vanilla.instance.battle;

namespace CardWars.Client.scripts.vanilla.layout;

public class  DefaultLayout : IBattleLayoutHandler
{
	public void Compute(BattleInstance battle)
	{
		foreach(var b in battle.State.Entities)
		{
			
		}
	}
}
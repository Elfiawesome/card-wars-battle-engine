namespace Elfiawesome.CardWarsBattleEngine.Game.Intents;

public class SetupGame : GameIntent
{
	public override void Execute(CardWarsBattleEngine engine)
	{
		// Do whatever to create battlefields etc
		engine.isGameStarted = true;
		foreach (var player in engine._players)
		{
			var battlefield = engine.AddBattlefield();
			player.Value.AttachBattlefield(battlefield);
		}
	}
}
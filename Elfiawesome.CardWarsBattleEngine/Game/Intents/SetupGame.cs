namespace Elfiawesome.CardWarsBattleEngine.Game.Intents;

public class SetupGame : GameIntent
{
	public override void Execute(CardWarsBattleEngine engine)
	{
		engine.isGameStarted = true;

		// Initialize the battlefield for each players
		foreach (var _p in engine._players)
		{
			var playerId = _p.Key;
			var player = _p.Value;

			var battlefield = engine.AddBattlefield();
			player.AttachBattlefield(battlefield);
		}
	}
}
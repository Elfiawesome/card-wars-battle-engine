namespace Elfiawesome.CardWarsBattleEngine.Game.Intents;

public class SetupGame : GameIntent
{
	public override void Execute(CardWarsBattleEngine engine)
	{
		engine.isGameStarted = true;

		// I want everything that happen in this game "saved" so that we can replay it again
		// So when the game for example initializes, we need to know what we did so that we can pack that data and sent off to the clients on what to do

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
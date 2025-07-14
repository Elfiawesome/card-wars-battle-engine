using Elfiawesome.CardWarsBattleEngine.Game.Actions;

namespace Elfiawesome.CardWarsBattleEngine.Game.Intents;

public class TurnMoveOn : GameIntent
{
	public override void Execute(CardWarsBattleEngine engine)
	{
		engine.QueueAction(new UpdatePlayerTurnsAction(
			engine._turnOrder,
			engine._currentTurnOrder + 1
		));
	}
}
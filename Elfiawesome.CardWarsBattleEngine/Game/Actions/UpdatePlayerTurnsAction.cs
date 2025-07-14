using Elfiawesome.CardWarsBattleEngine.Game.Entities;

namespace Elfiawesome.CardWarsBattleEngine.Game.Actions;

public class UpdatePlayerTurnsAction(List<PlayerId> turnOrder, int currentTurn) : GameAction
{
	public List<PlayerId> TurnOrder { get; set; } = turnOrder;
	public int currentTurn { get; set; } = currentTurn;

	public override void Execute(CardWarsBattleEngine engine)
	{
		engine._turnOrder = TurnOrder;
		engine._currentTurnOrder = currentTurn;
	}
}
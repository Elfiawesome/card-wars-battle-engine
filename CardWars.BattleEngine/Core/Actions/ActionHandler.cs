using CardWars.BattleEngine.Core.States;

namespace CardWars.BattleEngine.Core.Actions;

public abstract class ActionHandler<TActionData> where TActionData : IActionData
{
	public abstract bool Handle(GameState gameState, TActionData actionData);
}
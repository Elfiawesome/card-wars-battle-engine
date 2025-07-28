using CardWars.BattleEngine.Core.Events;
using CardWars.BattleEngine.Core.States;

namespace CardWars.BattleEngine.Core.Actions;

public abstract class ActionHandler<TActionData> where TActionData : ActionData
{
	public abstract void Handle(GameState gameState, EventManager eventManager, TActionData actionData);
}
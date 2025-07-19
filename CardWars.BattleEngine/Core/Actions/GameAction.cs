using CardWars.BattleEngine.Core.States;

namespace CardWars.BattleEngine.Core.Actions;

public abstract class GameAction
{
	public abstract void Execute(GameState state);
}
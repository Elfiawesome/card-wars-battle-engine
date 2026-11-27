using CardWars.BattleEngine.State;

namespace CardWars.BattleEngine.registry;

public interface IGameOverPolicy
{
	bool IsGameOver(GameState state);
}

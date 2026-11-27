using CardWars.BattleEngine.State;

namespace CardWars.BattleEngine.registry;

public interface IStateInitializer
{
	void Initialize(GameState state);
}

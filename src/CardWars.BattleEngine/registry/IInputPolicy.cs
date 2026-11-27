using CardWars.BattleEngine.State;

namespace CardWars.BattleEngine.registry;

public interface IInputPolicy
{
	bool CanProcessInput(GameState state, EntityId playerId);
}

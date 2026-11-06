using CardWars.BattleEngine.State;

namespace CardWars.BattleEngine.Rules;

public interface IEntityCondition
{
	bool IsMet(GameState state, EntityId entityId);
}

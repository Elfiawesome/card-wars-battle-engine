using CardWars.BattleEngine.State;
using CardWars.Core.Registry;

namespace CardWars.BattleEngine.registry;

public interface IEntityBehaviourProvider
{
	IReadOnlyList<ContentPointer> GetBehaviours(IEntity entity);
}

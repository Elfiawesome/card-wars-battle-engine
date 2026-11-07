using CardWars.BattleEngine.Rules;

namespace CardWars.BattleEngine.State;

public interface IEntity
{
	public EntityId Id { get; init; }
	public int BehaviourPriority { get; }

	public List<ContentPointer> GetBehaviours();
}
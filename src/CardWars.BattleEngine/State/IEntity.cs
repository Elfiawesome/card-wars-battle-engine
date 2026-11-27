using CardWars.BattleEngine.Rules;
using CardWars.Core.Data;

namespace CardWars.BattleEngine.State;

public interface IEntity
{
	public EntityId Id { get; init; }
	public int BehaviourPriority { get; }

	public List<ContentPointer> GetBehaviours(DataTagContext dataTags);
}

public interface IParentableEntity
{
	public EntityId? ParentId { get; }
}
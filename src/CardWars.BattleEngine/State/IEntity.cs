using CardWars.BattleEngine.Rules;
using CardWars.Core.Data;

namespace CardWars.BattleEngine.State;

public interface IEntity
{
	public EntityId Id { get; init; }
	public int BehaviourPriority { get; }

	// How the entity retrieves all the behaviours determined by whatever is in the entity.
	// Passing the DataTagContext (and possibly the entire registry in future if needed), to 
	// tell what ContentPointer in it here
	public List<ContentPointer> GetBehaviours(DataTagContext dataTags);
}

public interface IParentableEntity
{
	public EntityId? ParentId { get; }
}
using CardWars.BattleEngine.Rules;
using CardWars.Core.Data;

namespace CardWars.BattleEngine.State;

public interface IEntity
{
	public EntityId Id { get; init; }
	public int BehaviourPriority { get; }

	/// <summary>
	/// Returns the entity's behaviour pointers, deserializing any inline/content pointers
	/// through <paramref name="data"/> so resolution uses the owning engine's context rather
	/// than the static facade.
	/// </summary>
	public List<ContentPointer> GetBehaviours(DataContext data);
}
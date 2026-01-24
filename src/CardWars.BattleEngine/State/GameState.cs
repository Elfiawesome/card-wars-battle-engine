// CardWars.BattleEngine/State/GameState.cs

namespace CardWars.BattleEngine.State;

public class GameState
{
	private readonly Dictionary<EntityId, IEntity> _entities = [];

	// --- CRUD ---

	public void Add(IEntity entity) => _entities[entity.Id] = entity;

	public bool Remove(EntityId id) => _entities.Remove(id);

	public IEntity? Get(EntityId id)
		=> _entities.TryGetValue(id, out var e) ? e : null;

	public T? Get<T>(EntityId id) where T : class, IEntity
		=> Get(id) as T;

	// --- Queries ---

	public IEnumerable<IEntity> All => _entities.Values;

	public IEnumerable<T> OfType<T>() where T : class, IEntity
		=> _entities.Values.OfType<T>();

	public IEnumerable<IEntity> Where(Func<IEntity, bool> predicate)
		=> _entities.Values.Where(predicate);

	// --- Behaviour Collection ---
	// Returns (EntityId, BehaviourPointer) sorted by entity priority

	public TurnState Turn { get; set; } = new();

	public IEnumerable<(EntityId entityId, BehaviourPointer pointer)> GetAllBehaviourPointers()
	{
		return _entities.Values
			.OrderBy(e => e.BehaviourPriority)
			.SelectMany(e => e.GetBehaviours()
				.OrderBy(b => 0) // Will be sorted by behaviour priority after instantiation
				.Select(b => (e.Id, b)));
	}
}
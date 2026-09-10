using System.Runtime.CompilerServices;
using CardWars.Core.Data;
using CardWars.Core.Logging;
using CardWars.Core.Registry;

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

	public T? Require<T>(EntityId id,
		[CallerFilePath] string file = "",
		[CallerMemberName] string member = "") where T : class, IEntity
	{
		// Fast path — identical cost to Get<T>
		if (_entities.TryGetValue(id, out var entity) && entity is T typed)
			return typed;

		// Slow path — only on failure, only then do we build strings
		var source = $"{Path.GetFileNameWithoutExtension(file)}.{member}";
		if (id == EntityId.None)
			Log.Warn($"[{source}] Require<{typeof(T).Name}> called with EntityId.None");
		else if (entity == null)
			Log.Warn($"[{source}] Entity not found: {typeof(T).Name} [{id}]");
		else
			Log.Warn($"[{source}] Type mismatch: [{id}] is {entity.GetType().Name}, expected {typeof(T).Name}");

		return null;
	}

	// --- Queries ---

	public IEnumerable<IEntity> All => _entities.Values;

	public IEnumerable<T> OfType<T>() where T : class, IEntity
		=> _entities.Values.OfType<T>();

	public IEnumerable<IEntity> Where(Func<IEntity, bool> predicate)
		=> _entities.Values.Where(predicate);

	[DataTag]
	public TurnState Turn { get; set; } = new()
	{
		TurnOrder = [],
		AllowedPlayerInputs = [],
		TurnIndex = 0,
		TurnNumber = 0,
		Phase = TurnPhase.Setup
	};

	[DataTag]
	public LayoutState Layout { get; set; } = new()
	{
		Layout = ResourceId.Vanilla("TODO: haha wrong layout id"),
		LayoutConfig = new CompoundTag()
	};

	[DataTag] public IReadOnlyDictionary<EntityId, IEntity> Entities => _entities;

	public IEnumerable<(EntityId entityId, BehaviourPointer pointer)> GetAllBehaviourPointers()
	{
		return _entities.Values
			.OrderBy(e => e.BehaviourPriority)
			.SelectMany(e => e.GetBehaviours()
				.OrderBy(b => 0) // Will be sorted by behaviour priority after instantiation
				.Select(b => (e.Id, b)));
	}

	public void Clear()
	{
		_entities.Clear();
		Turn = default;
		Layout = default;
	}

	public GameStateSnapshot ToSnapshot()
	{
		var snapshot = new GameStateSnapshot
		{
			Entities = [.. Entities.Select(s => s.Value)],
			Turn = Turn.Copy(),
			Layout = Layout,
		};
		return snapshot;
	}

	public void FromSnapshot(GameStateSnapshot snapshot)
	{
		Clear();
		foreach (var e in snapshot.Entities)
		{
			_entities.Add(e.Id, e);
		}
		Turn = snapshot.Turn.Copy();
		Layout = snapshot.Layout.Copy();
	}
}


public class GameStateSnapshot
{
	[DataTag] public List<IEntity> Entities { get; set; } = [];
	[DataTag] public LayoutState Layout { get; set; } = default;
	[DataTag] public TurnState Turn { get; set; } = default;
}
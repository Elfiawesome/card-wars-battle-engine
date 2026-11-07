using System.Runtime.CompilerServices;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.Core.Data.Access;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Tags;
using CardWars.Core.Logging;
using CardWars.Core.Registry;

namespace CardWars.BattleEngine.State;

public class GameState : IDataTagAccessible
{
	private readonly Dictionary<EntityId, IEntity> _entities = [];

	// --- CRUD ---

	public void Add(IEntity entity) => _entities[entity.Id] = entity;

	public bool Remove(EntityId id) => _entities.Remove(id);

	public IEntity? Get(EntityId? id)
		=> id is { } key && _entities.TryGetValue(key, out var e) ? e : null;

	public T? Get<T>(EntityId? id) where T : class, IEntity
		=> Get(id) is T typed ? typed : null;

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
		else if (entity is null)
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
	public TurnState Turn { get; set; } = new();

	[DataTag]
	public LayoutState Layout { get; set; } = new()
	{
		Layout = ResourceId.Vanilla("TODO: haha wrong layout id"),
		LayoutConfig = new CompoundTag()
	};

	[DataTag] public ResourceId RuleId { get; set; } = ResourceId.Empty;
	[DataTag] public GameOutcome Outcome { get; set; } = GameOutcome.Continue;

	public bool IsGameOver => Outcome.IsGameOver;
	public int? WinningTeam => Outcome.WinningTeam;
	public bool IsDraw => Outcome.IsDraw;

	[DataTag] public IReadOnlyDictionary<EntityId, IEntity> Entities => _entities;

	// For modding?
	public ScriptScope GlobalScope { get; } = new();

	public bool TryGetMember(string member, out DataTag? value)
	{
		value = null;
		return false;
	}

	public bool TrySetMember(string member, DataTag value) => false;

	public bool TryGetIndex(DataTag index, out DataTag? value)
	{
		var entityId = index switch
		{
			GuidTag guidTag => new EntityId(guidTag.Value),
			StringTag stringTag when Guid.TryParse(stringTag.Value, out var guid) => new EntityId(guid),
			_ => EntityId.None
		};

		var entity = Get(entityId);
		value = entity is null ? null : new ObjectRefTag(entity);
		return entity is not null;
	}

	public bool TrySetIndex(DataTag index, DataTag value) => false;

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
		GlobalScope.Clear();
		Turn = new();
		Layout = default;
		RuleId = ResourceId.Empty;
		Outcome = GameOutcome.Continue;
	}

	public GameStateSnapshot ToSnapshot()
	{
		var snapshot = new GameStateSnapshot
		{
			Entities = [.. Entities.Select(s => s.Value)],
			Turn = Turn.Clone(),
			Layout = Layout,
			RuleId = RuleId,
			Outcome = Outcome,
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
		Turn = snapshot.Turn is { } turn ? turn.Clone() : new();
		Layout = snapshot.Layout.Copy();
		RuleId = snapshot.RuleId;
		Outcome = snapshot.Outcome;
	}
}


public class GameStateSnapshot
{
	[DataTag] public List<IEntity> Entities { get; set; } = [];
	[DataTag] public LayoutState Layout { get; set; } = default;
	[DataTag] public TurnState Turn { get; set; } = new();
	[DataTag] public ResourceId RuleId { get; set; } = ResourceId.Empty;
	[DataTag] public GameOutcome Outcome { get; set; } = GameOutcome.Continue;
}
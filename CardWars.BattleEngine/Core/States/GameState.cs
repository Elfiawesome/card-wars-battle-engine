namespace CardWars.BattleEngine.Core.States;

public class GameState
{
	// public EventManager EventManager;
	public Dictionary<PlayerId, Player> Players { get; set; } = [];
	public Dictionary<BattlefieldId, Battlefield> Battlefields { get; set; } = [];
	public Dictionary<UnitSlotId, UnitSlot> UnitSlots { get; set; } = [];
	public Dictionary<UnitId, Unit> Units { get; set; } = [];

	private long _playerIdCounter = 0;
	private long _battlefieldIdCounter = 0;
	private long _unitSlotIdCounter = 0;
	private long _unitIdCounter = 0;
	public PlayerId PlayerIdCounter { get => new(_playerIdCounter++); }
	public BattlefieldId BattlefieldIdCounter { get => new(_battlefieldIdCounter++); }
	public UnitSlotId UnitSlotIdCounter { get => new(_unitSlotIdCounter++); }
	public UnitId UnitIdCounter { get => new(_unitIdCounter++); }
}

public class EventManager
{
	public EventLink<> OnUnitCreated = new();
	public EventLink<> OnUnitDestroyed = new();
	public EventLink<> OnUnitSummoned = new();
	public EventLink<> OnUnitUnsummoned = new();
	public EventLink<> OnUnitAddedToHand = new();
	public EventLink<> OnUnitRemovedFromHand = new();
}

public class EventLink<TId, TSubscriber, TContext>
	where TId : notnull
	where TContext : EventContext
	where TSubscriber : EventSubscriber<TContext>
{
	private List<TSubscriber> _globalSubscribers = [];
	private Dictionary<TId, List<TSubscriber>> _specificSubscribers = [];
	

	public void Subscribe(TSubscriber subscriber)
	{
		_globalSubscribers.Add(subscriber);
	}

	public void Unsubscribe(TSubscriber subscriber)
	{
		_globalSubscribers.Remove(subscriber);
		
	}
}

public abstract class EventSubscriber<TContext> where TContext : EventContext
{
	public int Priority = 0;
}

public abstract class EventContext
{
	
}
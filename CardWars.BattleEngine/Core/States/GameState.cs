namespace CardWars.BattleEngine.Core.States;

public class GameState
{
	public Dictionary<PlayerId, Player> Players { get; set; } = [];
	public Dictionary<BattlefieldId, Battlefield> Battlefields { get; set; } = [];
	public Dictionary<UnitSlotId, UnitSlot> UnitSlots { get; set; } = [];
	public Dictionary<UnitId, Unit> Units { get; set; } = [];

	private long _playerIdCounter = 0;
	private long _battlefieldIdCounter = 0;
	private long _unitSlotIdCounter = 0;
	private long _unitIdCounter = 0;
	public long PlayerIdCounter { get => _playerIdCounter++; }
	public long BattlefieldIdCounter { get => _battlefieldIdCounter++; }
	public long UnitSlotIdCounter { get => _unitSlotIdCounter++; }
	public long UnitIdCounter { get => _unitIdCounter++; }
}

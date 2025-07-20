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
	public PlayerId PlayerIdCounter { get => new(_playerIdCounter++); }
	public BattlefieldId BattlefieldIdCounter { get => new(_battlefieldIdCounter++); }
	public UnitSlotId UnitSlotIdCounter { get => new(_unitSlotIdCounter++); }
	public UnitId UnitIdCounter { get => new(_unitIdCounter++); }
}

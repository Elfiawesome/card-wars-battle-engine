namespace CardWars.BattleEngine.Core.GameState;

public class PlayerState
{
	public readonly PlayerStateId Id;
	public string Name = "";
	public List<BattlefieldStateId> ControllingBattlefields = [];

	public PlayerState(PlayerStateId id)
	{
		Id = id;
	}
}

public readonly record struct PlayerStateId(Guid Value);
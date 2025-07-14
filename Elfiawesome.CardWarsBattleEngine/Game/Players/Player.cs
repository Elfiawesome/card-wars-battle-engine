using Elfiawesome.CardWarsBattleEngine.Game.Battlefields;

namespace Elfiawesome.CardWarsBattleEngine.Game.Players;

public class Player
{
	public readonly PlayerId Id;
	public readonly List<BattlefieldId> ControllingBattlefieldIds = [];

	public Player(PlayerId id)
	{
		Id = id;
	}

	public void AttachBattlefield(Battlefield Battlefield)
	{
		if (!ControllingBattlefieldIds.Contains(Battlefield.Id))
		{
			ControllingBattlefieldIds.Add(Battlefield.Id);
		}
	}
}
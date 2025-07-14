namespace Elfiawesome.CardWarsBattleEngine.Game;

public class Player
{
	public readonly Guid Id;
	public readonly List<Guid> ControllingBattlefieldIds = [];

	public Player(Guid id)
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
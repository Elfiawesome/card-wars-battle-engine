namespace Elfiawesome.CardWarsBattleEngine.Game.Entities;

public class Player : Entity<PlayerId>
{
	public string Name = "";
	public readonly List<BattlefieldId> ControllingBattlefieldIds = [];

	public Player(PlayerId id) : base(id)
	{
		
	}

	public void AttachBattlefield(Battlefield Battlefield)
	{
		if (!ControllingBattlefieldIds.Contains(Battlefield.Id))
		{
			ControllingBattlefieldIds.Add(Battlefield.Id);
		}
	}
}

public readonly record struct PlayerId(long Id);
namespace Elfiawesome.CardWarsBattleEngine.Game;

public class Player
{
	public readonly Guid Id;
	public readonly List<Guid> ControllingBattlefieldIds = [];

	public Player(Guid id)
	{
		Id = id;
	}
}
namespace Elfiawesome.CardWarsBattleEngine.Game.Battlefields;

public readonly record struct PlayerId(long Id);

public class PlayerIdGenerator()
{
	private long _counter = 0;

	public PlayerId NewId()
	{
		return new PlayerId(_counter++);
	}
}
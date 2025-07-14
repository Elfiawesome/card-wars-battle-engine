namespace Elfiawesome.CardWarsBattleEngine.Game.Battlefields;

public readonly record struct BattlefieldId(long Id);

public class BattlefieldIdGenerator()
{
	private long _counter = 0;

	public BattlefieldId NewId()
	{
		return new BattlefieldId(_counter++);
	}
}
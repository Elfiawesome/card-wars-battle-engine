namespace CardWars.BattleEngine.Core.States;

public class Player(PlayerId id)
{
	public readonly PlayerId Id = id;
}

public readonly record struct PlayerId(long Value);
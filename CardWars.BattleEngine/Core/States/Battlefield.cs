namespace CardWars.BattleEngine.Core.States;

public class Battlefield(BattlefieldId id)
{
	public readonly BattlefieldId Id = id;
}

public readonly record struct BattlefieldId(long Value);
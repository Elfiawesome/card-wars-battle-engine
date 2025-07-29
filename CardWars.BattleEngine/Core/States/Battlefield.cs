namespace CardWars.BattleEngine.Core.States;

public class Battlefield(BattlefieldId id)
{
	public readonly BattlefieldId Id = id;
	public List<UnitSlotId> UnitSlots { get; } = new();
}

public readonly record struct BattlefieldId(long Value);
namespace CardWars.BattleEngine.State;

public class UnitCard(UnitCardId id) : EntityState<UnitCardId>(id)
{
}

public record struct UnitCardId(Guid Id);
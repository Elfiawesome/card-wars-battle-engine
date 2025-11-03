namespace CardWars.BattleEngine.Entity;

public class UnitCard(EntityService service, UnitId id) : Card<UnitId>(service, id)
{
	
}

public record struct UnitId(Guid Id);
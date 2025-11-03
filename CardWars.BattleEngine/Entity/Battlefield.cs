namespace CardWars.BattleEngine.Entity;

public class Battlefield(EntityService service, BattlefieldId id) : Entity<BattlefieldId>(service, id)
{
}

public record struct BattlefieldId(Guid Id);
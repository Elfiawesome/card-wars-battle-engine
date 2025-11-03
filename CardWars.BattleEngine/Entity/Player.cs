namespace CardWars.BattleEngine.Entity;

public class Player(EntityService service, PlayerId id) : Entity<PlayerId>(service, id)
{
	public string Name = "Default Name";
}

public record struct PlayerId(Guid Id);
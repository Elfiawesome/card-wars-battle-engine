namespace CardWars.BattleEngine.Entity;

public class SpellCard(EntityService service, SpellCardId id) : Card<SpellCardId>(service, id)
{
	public string Name { get; set; } = "Default Name";
	public string FlavourText { get; set; } = "Default Description";
	public int PointCost { get; set; } = 0;
}

public record struct SpellCardId(Guid Id);
namespace CardWars.BattleEngine.Entity;

public class SpellCard(SpellCardId id) : Card<SpellCardId>(id)
{
	public string Name { get; set; } = "Default Name";
	public string FlavourText { get; set; } = "Default Description";
	public int PointCost { get; set; } = 0;
}

public record struct SpellCardId(Guid Id);
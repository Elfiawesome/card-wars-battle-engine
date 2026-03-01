using CardWars.BattleEngine.State;

namespace CardWars.BattleEngine.Data;

public class CardDefinition()
{
	public string Name { get; set; } = "";
	public int Pt { get; set; } = 0;
	public int Hp { get; set; } = 0;
	public int Atk { get; set; } = 0;

	public List<AbilityDefinition> Abilities { get; set; } = [];
}

public record struct AbilityDefinition()
{
	public string Description { get; set; } = "";
	public BehaviourPointer Behaviour { get; set; }
}
using CardWars.BattleEngine.State;

namespace CardWars.BattleEngine.Vanilla.Entity;

public class GenericCard(EntityId id) : IEntity
{
	public EntityId Id { get; init; } = id;
	public EntityId? OwnerPlayerId { get; set; }
	public EntityId? OwnerUnitSlotId { get; set; }
	public bool IsPlayed => (OwnerPlayerId == null) && (OwnerUnitSlotId != null);

	public string Name { get; set; } = "";
	public int Pt { get; set; } = 0;
	public int Hp { get; set; } = 0;
	public int Atk { get; set; } = 0;
	
	public List<CardAbility> Abilities { get; set; } = [];
	// Used primarily by spells (for custom spells)
	// Units will all use the same deploy behaviour of summoning a unit
	public BehaviourPointer? DeployBehaviour { get; set; }

	public int BehaviourPriority => 0;
	public List<BehaviourPointer> GetBehaviours() => [.. Abilities.Select(a => a.Behaviour)];
}

public record struct CardAbility()
{
	public string Description { get; set; } = "";
	public BehaviourPointer Behaviour { get; set; }
}
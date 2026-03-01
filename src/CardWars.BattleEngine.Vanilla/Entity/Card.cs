using CardWars.BattleEngine.Data;
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

	public Dictionary<string, object> CustomData = [];

	public List<AbilityDefinition> Abilities { get; set; } = [];
	// Special behaviour for card functionality like listening to use card and deploying the card onto battlefield
	public List<BehaviourPointer> IntrinsicBehaviour { get; set; } = [];

	public int BehaviourPriority => 0;
	public List<BehaviourPointer> GetBehaviours() => [.. Abilities.Select(a => a.Behaviour), .. IntrinsicBehaviour];
}
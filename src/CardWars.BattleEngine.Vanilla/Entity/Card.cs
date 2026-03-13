using CardWars.BattleEngine.State;
using CardWars.Core.Data;

namespace CardWars.BattleEngine.Vanilla.Entity;

// TODO: Do the datatag here
[DataTagType()]
public class GenericCard(EntityId id) : IEntity
{
	[DataTag] public EntityId Id { get; init; } = id;
	[DataTag] public EntityId? OwnerPlayerId { get; set; }
	[DataTag] public EntityId? OwnerUnitSlotId { get; set; }
	public bool IsPlayed => (OwnerPlayerId == null) && (OwnerUnitSlotId != null);

	[DataTag] public CompoundTag Data { get; set; } = new();

	public string CardType => Data.GetString("card_type");
	
	// --- Base Card Data ---
	public string Name => Data.GetString("name");

	// --- Unit Specific Data ---
	public int Pt => Data.GetInt("pt");
	public int PtMax => Data.GetInt("pt_max");
	public int Hp => Data.GetInt("hp");
	public int HpMax => Data.GetInt("hp_max");
	public int Atk => Data.GetInt("atk");
	public int AtkMax => Data.GetInt("atk_max");
	public int Charge => Data.GetInt("charge"); // Amount of attacks per unit
	public int ChargeMax => Data.GetInt("charge_max");
	public ListTag? SpAtk => Data.GetList("sp_atk"); // idk todo later


	// --- Hero Specific Data ---
	public int Hrt => Data.GetInt("hrt");

	public int BehaviourPriority => Data.GetInt("behaviour_priority");

	public List<BehaviourPointer> GetBehaviours()
	{
		var result = new List<BehaviourPointer>();

		if (Data.GetList("abilities") is { } abilities)
		{
			foreach (var item in abilities.Items.OfType<CompoundTag>())
			{
				if (item.GetCompound("behaviour") is { } bTag)
					result.Add(DataTagMapper.FromTag<BehaviourPointer>(bTag));// <- this feels and looks wrong? TODO: fix
			}
		}

		if (Data.GetList("intrinsic_behaviours") is { } intrinsics)
		{
			foreach (var item in intrinsics.Items.OfType<CompoundTag>())
				result.Add(DataTagMapper.FromTag<BehaviourPointer>(item)); // <- this feels and looks wrong?
		}

		return result;
	}
}
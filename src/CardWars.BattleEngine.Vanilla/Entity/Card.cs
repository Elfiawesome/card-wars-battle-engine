using CardWars.BattleEngine.State;
using CardWars.Core.Data;

namespace CardWars.BattleEngine.Vanilla.Entity;

// TODO: Do the datatag here
[DataTagType()]
public class GenericCard(EntityId id) : IEntity
{
	public EntityId Id { get; init; } = id;
	public EntityId? OwnerPlayerId { get; set; }
	public EntityId? OwnerUnitSlotId { get; set; }
	public bool IsPlayed => (OwnerPlayerId == null) && (OwnerUnitSlotId != null);

	public CompoundTag Data { get; set; } = new();

	public string Name => Data.GetString("name");
	public int Pt => Data.GetInt("pt");
	public int Hp => Data.GetInt("hp");
	public int Atk => Data.GetInt("atk");

	public int BehaviourPriority => Data.GetInt("behaviour_priority");

	public List<BehaviourPointer> GetBehaviours()
	{
		var result = new List<BehaviourPointer>();

		if (Data.GetList("abilities") is { } abilities)
		{
			foreach (var item in abilities.Items.OfType<CompoundTag>())
			{
				if (item.GetCompound("behaviour") is { } bTag)
					result.Add(DataTagSerializer.Deserialize<BehaviourPointer>(bTag));// <- this feels and looks wrong? TODO: fix
			}
		}

		if (Data.GetList("intrinsic_behaviours") is { } intrinsics)
		{
			foreach (var item in intrinsics.Items.OfType<CompoundTag>())
				result.Add(DataTagSerializer.Deserialize<BehaviourPointer>(item)); // <- this feels and looks wrong?
		}

		return result;
	}
}
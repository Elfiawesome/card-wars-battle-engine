using CardWars.Core.Data.Tags;

namespace CardWars.BattleEngine.Behaviour.DataDriven.Objects;

public sealed class TagObjectAccessor : IObjectAccessor
{
	public bool TryGetMember(object target, string member, out DataTag? value)
	{
		switch (target)
		{
			case CompoundTag compound:
				value = compound.Get(member);
				return value != null;
			case ListTag list when int.TryParse(member, out var index):
				value = list.Get(index);
				return value != null;
			default:
				value = null;
				return false;
		}
	}

	public bool TrySetMember(object target, string member, DataTag value)
	{
		switch (target)
		{
			case CompoundTag compound:
				compound.Set(member, value);
				return true;
			case ListTag list when int.TryParse(member, out var index):
				list.Set(index, value);
				return true;
			default:
				return false;
		}
	}

	public bool TryGetIndex(object target, DataTag index, out DataTag? value)
	{
		switch (target, index)
		{
			case (CompoundTag compound, StringTag key):
				value = compound.Get(key.Value);
				return value != null;
			case (ListTag list, IntTag intTag):
				value = list.Get(intTag.Value);
				return value != null;
			default:
				value = null;
				return false;
		}
	}

	public bool TrySetIndex(object target, DataTag index, DataTag value)
	{
		switch (target, index)
		{
			case (CompoundTag compound, StringTag key):
				compound.Set(key.Value, value);
				return true;
			case (ListTag list, IntTag intTag):
				if (intTag.Value < 0 || intTag.Value >= list.Count) return false;
				list.Set(intTag.Value, value);
				return true;
			default:
				return false;
		}
	}
}

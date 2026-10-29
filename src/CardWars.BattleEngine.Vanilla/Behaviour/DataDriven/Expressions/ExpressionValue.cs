using System.Globalization;
using CardWars.Core.Data.Serialization.Json;
using CardWars.Core.Data.Tags;

namespace CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Expressions;

internal static class ExpressionValue
{
	public static bool TryGetNumber(DataTag tag, out double value)
	{
		switch (tag)
		{
			case IntTag intTag:
				value = intTag.Value;
				return true;
			case FloatTag floatTag:
				value = floatTag.Value;
				return true;
			default:
				value = 0;
				return false;
		}
	}

	public static bool PrefersFloat(DataTag left, DataTag right)
		=> left is FloatTag || right is FloatTag;

	public static DataTag Number(double value, bool forceFloat)
		=> forceFloat ? new FloatTag((float)value) : new IntTag((int)value);

	public static bool AreEqual(DataTag? left, DataTag? right)
	{
		if (left is null or NullTag) return right is null or NullTag;
		if (right is null or NullTag) return false;
		if (TryGetNumber(left, out var leftNumber) && TryGetNumber(right, out var rightNumber))
			return leftNumber == rightNumber;

		return (left, right) switch
		{
			(StringTag leftString, StringTag rightString) => leftString.Value == rightString.Value,
			(BoolTag leftBool, BoolTag rightBool) => leftBool.Value == rightBool.Value,
			(GuidTag leftGuid, GuidTag rightGuid) => leftGuid.Value == rightGuid.Value,
			(CompoundTag leftCompound, CompoundTag rightCompound) => CompoundEquals(leftCompound, rightCompound),
			(ListTag leftList, ListTag rightList) => ListEquals(leftList, rightList),
			(ObjectRefTag leftObject, ObjectRefTag rightObject) => Equals(leftObject.Value, rightObject.Value),
			_ => false
		};
	}

	public static int? Compare(DataTag left, DataTag right)
	{
		if (TryGetNumber(left, out var leftNumber) && TryGetNumber(right, out var rightNumber))
			return leftNumber.CompareTo(rightNumber);

		if (left is StringTag leftString && right is StringTag rightString)
			return string.CompareOrdinal(leftString.Value, rightString.Value);

		return null;
	}

	public static string ToText(DataTag tag) => tag switch
	{
		NullTag => "",
		IntTag intTag => intTag.Value.ToString(CultureInfo.InvariantCulture),
		FloatTag floatTag => floatTag.Value.ToString(CultureInfo.InvariantCulture),
		BoolTag boolTag => boolTag.Value ? "true" : "false",
		StringTag stringTag => stringTag.Value,
		GuidTag guidTag => guidTag.Value.ToString(),
		CompoundTag or ListTag => DataTagSerializer.Serialize(tag),
		ObjectRefTag objectTag => objectTag.Value.ToString() ?? "",
		_ => tag.ToString() ?? ""
	};

	private static bool CompoundEquals(CompoundTag left, CompoundTag right)
	{
		if (left.Count != right.Count) return false;
		foreach (var (key, leftValue) in left.Entries)
		{
			if (!right.Entries.TryGetValue(key, out var rightValue)) return false;
			if (!AreEqual(leftValue, rightValue)) return false;
		}

		return true;
	}

	private static bool ListEquals(ListTag left, ListTag right)
	{
		if (left.Count != right.Count) return false;
		for (var i = 0; i < left.Count; i++)
		{
			if (!AreEqual(left.Get(i), right.Get(i))) return false;
		}

		return true;
	}
}

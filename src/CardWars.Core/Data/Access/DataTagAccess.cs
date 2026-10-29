using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Global;
using CardWars.Core.Data.Mapping;
using CardWars.Core.Data.Tags;

namespace CardWars.Core.Data.Access;

public static class DataTagAccess
{
	private static readonly ConcurrentDictionary<Type, PropertyInfo[]> Properties = new();
	private static readonly ConcurrentDictionary<Type, PropertyInfo[]> Indexers = new();

	public static bool TryGetMember(object target, string member, out DataTag? value)
	{
		if (target is IDataTagAccessible accessible && accessible.TryGetMember(member, out value)) return true;
		if (target is DataTag tag && TryGetTagMember(tag, member, out value)) return true;

		if (member == DataTagMapper.TypeDiscriminatorKey)
		{
			var typeId = DataTagTypeRegistry.GetTypeId(target.GetType());
			if (typeId is not null)
			{
				value = new StringTag(typeId);
				return true;
			}
		}

		var property = FindProperty(target.GetType(), member);
		if (property is not null)
		{
			value = ToAccessTag(property.GetValue(target), property.PropertyType);
			return true;
		}

		if (TryGetDataBag(target, out var data) && data is { } bag && TryGetDataEntry(bag, member, out value)) return true;
		if (TryGetIndexerMember(target, member, out value)) return true;

		value = null;
		return false;
	}

	public static bool TrySetMember(object target, string member, DataTag value)
	{
		if (target is IDataTagAccessible accessible && accessible.TrySetMember(member, value)) return true;

		switch (target)
		{
			case CompoundTag compound:
				compound.Set(member, value);
				return true;
			case ListTag list when int.TryParse(member, out var index):
				if (index < 0 || index >= list.Count) return false;
				list.Set(index, value);
				return true;
		}

		var property = FindProperty(target.GetType(), member);
		if (property is { CanWrite: true })
		{
			property.SetValue(target, DataTagMapper.FromValueTag(value, property.PropertyType));
			return true;
		}

		if (TryGetDataBag(target, out var data) && data is { } bag)
		{
			bag.Set(member, value);
			return true;
		}

		return TrySetIndexerMember(target, member, value);
	}

	public static bool TryGetIndex(object target, DataTag index, out DataTag? value)
	{
		if (target is IDataTagAccessible accessible && accessible.TryGetIndex(index, out value)) return true;

		switch (target)
		{
			case CompoundTag compound when index is StringTag key:
				if (!compound.Has(key.Value)) break;
				value = compound.Get(key.Value) ?? new NullTag();
				return true;
			case ListTag list when index is IntTag intTag:
				if (intTag.Value < 0 || intTag.Value >= list.Count) break;
				value = list.Get(intTag.Value) ?? new NullTag();
				return true;
			case IDictionary dictionary:
				var dictionaryKey = ConvertToType(index, GetDictionaryKeyType(target.GetType()));
				if (dictionaryKey is null || !dictionary.Contains(dictionaryKey)) break;
				value = ToAccessTag(dictionary[dictionaryKey], GetDictionaryValueType(target.GetType()));
				return true;
			case IList list when index is IntTag intTag:
				if (intTag.Value < 0 || intTag.Value >= list.Count) break;
				value = ToAccessTag(list[intTag.Value], typeof(object));
				return true;
		}

		if (TryGetIndexer(target, index, out value)) return true;

		value = null;
		return false;
	}

	public static bool TrySetIndex(object target, DataTag index, DataTag value)
	{
		if (target is IDataTagAccessible accessible && accessible.TrySetIndex(index, value)) return true;

		switch (target)
		{
			case CompoundTag compound when index is StringTag key:
				compound.Set(key.Value, value);
				return true;
			case ListTag list when index is IntTag intTag:
				if (intTag.Value < 0 || intTag.Value >= list.Count) return false;
				list.Set(intTag.Value, value);
				return true;
			case IDictionary dictionary:
				var dictionaryKey = ConvertToType(index, GetDictionaryKeyType(target.GetType()));
				if (dictionaryKey is null) return false;
				dictionary[dictionaryKey] = DataTagMapper.FromValueTag(value, GetDictionaryValueType(target.GetType()));
				return true;
			case IList list when index is IntTag intTag:
				if (intTag.Value < 0 || intTag.Value >= list.Count) return false;
				list[intTag.Value] = DataTagMapper.FromValueTag(value, typeof(object));
				return true;
		}

		return TrySetIndexer(target, index, value);
	}

	private static bool TryGetTagMember(DataTag target, string member, out DataTag? value)
	{
		switch (target)
		{
			case CompoundTag compound:
				if (!compound.Has(member)) break;
				value = compound.Get(member) ?? new NullTag();
				return true;
			case ListTag list when int.TryParse(member, out var index):
				if (index < 0 || index >= list.Count) break;
				value = list.Get(index) ?? new NullTag();
				return true;
		}

		value = null;
		return false;
	}

	private static PropertyInfo? FindProperty(Type type, string member)
	{
		var normalized = Normalize(member);
		foreach (var property in GetProperties(type))
		{
			var key = property.GetCustomAttribute<DataTagAttribute>()?.Key ?? property.Name;
			if (Normalize(key) == normalized || Normalize(property.Name) == normalized)
				return property;
		}

		return null;
	}

	private static PropertyInfo[] GetProperties(Type type)
		=> Properties.GetOrAdd(type, static t => t
			.GetProperties(BindingFlags.Public | BindingFlags.Instance)
			.Where(property => property.GetIndexParameters().Length == 0)
			.ToArray());

	private static PropertyInfo[] GetIndexers(Type type)
		=> Indexers.GetOrAdd(type, static t => t
			.GetProperties(BindingFlags.Public | BindingFlags.Instance)
			.Where(property => property.GetIndexParameters().Length == 1)
			.ToArray());

	private static bool TryGetIndexerMember(object target, string member, out DataTag? value)
	{
		foreach (var indexer in GetIndexers(target.GetType()))
		{
			var parameter = indexer.GetIndexParameters()[0];
			var converted = ConvertToType(new StringTag(member), parameter.ParameterType);
			if (converted is null) continue;

			value = ToAccessTag(indexer.GetValue(target, [converted]), indexer.PropertyType);
			return true;
		}

		value = null;
		return false;
	}

	private static bool TrySetIndexerMember(object target, string member, DataTag value)
	{
		foreach (var indexer in GetIndexers(target.GetType()))
		{
			if (!indexer.CanWrite) continue;
			var parameter = indexer.GetIndexParameters()[0];
			var converted = ConvertToType(new StringTag(member), parameter.ParameterType);
			if (converted is null) continue;
			indexer.SetValue(target, DataTagMapper.FromValueTag(value, indexer.PropertyType), [converted]);
			return true;
		}

		return false;
	}

	private static bool TryGetIndexer(object target, DataTag index, out DataTag? value)
	{
		foreach (var indexer in GetIndexers(target.GetType()))
		{
			var parameter = indexer.GetIndexParameters()[0];
			var converted = ConvertToType(index, parameter.ParameterType);
			if (converted is null) continue;

			value = ToAccessTag(indexer.GetValue(target, [converted]), indexer.PropertyType);
			return true;
		}

		value = null;
		return false;
	}

	private static bool TrySetIndexer(object target, DataTag index, DataTag value)
	{
		foreach (var indexer in GetIndexers(target.GetType()))
		{
			if (!indexer.CanWrite) continue;
			var parameter = indexer.GetIndexParameters()[0];
			var converted = ConvertToType(index, parameter.ParameterType);
			if (converted is null) continue;
			indexer.SetValue(target, DataTagMapper.FromValueTag(value, indexer.PropertyType), [converted]);
			return true;
		}

		return false;
	}

	private static bool TryGetDataBag(object target, out CompoundTag? data)
	{
		var property = target.GetType().GetProperty("Data", BindingFlags.Public | BindingFlags.Instance);
		if (property?.GetValue(target) is CompoundTag compound)
		{
			data = compound;
			return true;
		}

		data = null;
		return false;
	}

	private static bool TryGetDataEntry(CompoundTag data, string member, out DataTag? value)
	{
		if (data.Has(member))
		{
			value = data.Get(member) ?? new NullTag();
			return true;
		}

		var normalized = Normalize(member);
		foreach (var (key, entry) in data.Entries)
		{
			if (Normalize(key) != normalized) continue;
			value = entry;
			return true;
		}

		value = null;
		return false;
	}

	private static object? ConvertToType(DataTag tag, Type targetType)
	{
		if (targetType == typeof(string) && tag is StringTag stringTag) return stringTag.Value;
		return DataTagMapper.FromValueTag(tag, targetType);
	}

	private static Type GetDictionaryKeyType(Type type)
		=> GetDictionaryInterface(type)?.GetGenericArguments()[0] ?? typeof(object);

	private static Type GetDictionaryValueType(Type type)
		=> GetDictionaryInterface(type)?.GetGenericArguments()[1] ?? typeof(object);

	private static Type? GetDictionaryInterface(Type type)
	{
		if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IDictionary<,>)) return type;
		return type.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>));
	}

	private static DataTag ToAccessTag(object? value, Type declaredType)
	{
		if (value is null) return new NullTag();
		if (value is DataTag tag) return tag;
		if (value is string or int or float or double or long or bool or Guid)
			return DataTagMapper.ToValueTag(value, declaredType) ?? new NullTag();
		if (declaredType.IsEnum) return new StringTag(value.ToString() ?? "");
		if (DataTagMapper.HasConverter(declaredType) || DataTagMapper.HasConverter(value.GetType()))
			return DataTagMapper.ToValueTag(value, declaredType) ?? new NullTag();
		if (value is IDictionary) return new ObjectRefTag(value);
		if (value is IEnumerable) return DataTagMapper.ToValueTag(value, declaredType) ?? new NullTag();
		return new ObjectRefTag(value);
	}

	private static string Normalize(string value) => value.Replace("_", "").ToLowerInvariant();
}

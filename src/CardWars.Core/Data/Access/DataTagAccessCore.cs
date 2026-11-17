using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Tags;

namespace CardWars.Core.Data.Access;

/// <summary>
/// Instance-aware member and index access over arbitrary objects and <see cref="DataTag"/>s.
/// Introduced in Phase 1: resolves the polymorphic discriminator key, the type id, and
/// converters from a <see cref="DataContext"/> instead of static process-global state.
///
/// The static <see cref="DataTagAccess"/> class is a temporary Phase 1 shim over
/// <see cref="DataContext.Default"/> that delegates here.
///
/// The property/indexer reflection caches below stay static on purpose: they are keyed
/// solely by <see cref="Type"/> and carry no game or session meaning (AGENT.md allows
/// caches keyed only by Type).
/// </summary>
internal static class DataTagAccessCore
{
	private static readonly ConcurrentDictionary<Type, PropertyInfo[]> Properties = new();
	private static readonly ConcurrentDictionary<Type, PropertyInfo[]> Indexers = new();

	public static bool TryGetMember(DataContext ctx, object target, string member, out DataTag? value)
	{
		if (target is IDataTagAccessible accessible && accessible.TryGetMember(member, out value)) return true;
		if (target is DataTag tag && TryGetTagMember(tag, member, out value)) return true;

		if (member == ctx.TypeDiscriminatorKey)
		{
			var typeId = ctx.GetTypeId(target.GetType());
			if (typeId is not null)
			{
				value = new StringTag(typeId);
				return true;
			}
		}

		var property = FindProperty(target.GetType(), member);
		if (property is not null)
		{
			value = ToAccessTag(ctx, property.GetValue(target), property.PropertyType);
			return true;
		}

		if (TryGetDataBag(target, out var data) && data is { } bag && TryGetDataEntry(bag, member, out value)) return true;
		if (TryGetIndexerMember(ctx, target, member, out value)) return true;

		value = null;
		return false;
	}

	public static bool TrySetMember(DataContext ctx, object target, string member, DataTag value)
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
			property.SetValue(target, ctx.FromValueTag(value, property.PropertyType));
			return true;
		}

		if (TryGetDataBag(target, out var data) && data is { } bag)
		{
			bag.Set(member, value);
			return true;
		}

		return TrySetIndexerMember(ctx, target, member, value);
	}

	public static bool TryGetIndex(DataContext ctx, object target, DataTag index, out DataTag? value)
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
				var dictionaryKey = ConvertToType(ctx, index, GetDictionaryKeyType(target.GetType()));
				if (dictionaryKey is null || !dictionary.Contains(dictionaryKey)) break;
				value = ToAccessTag(ctx, dictionary[dictionaryKey], GetDictionaryValueType(target.GetType()));
				return true;
			case IList list when index is IntTag intTag:
				if (intTag.Value < 0 || intTag.Value >= list.Count) break;
				value = ToAccessTag(ctx, list[intTag.Value], typeof(object));
				return true;
		}

		if (TryGetIndexer(ctx, target, index, out value)) return true;

		value = null;
		return false;
	}

	public static bool TrySetIndex(DataContext ctx, object target, DataTag index, DataTag value)
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
				var dictionaryKey = ConvertToType(ctx, index, GetDictionaryKeyType(target.GetType()));
				if (dictionaryKey is null) return false;
				dictionary[dictionaryKey] = ctx.FromValueTag(value, GetDictionaryValueType(target.GetType()));
				return true;
			case IList list when index is IntTag intTag:
				if (intTag.Value < 0 || intTag.Value >= list.Count) return false;
				list[intTag.Value] = ctx.FromValueTag(value, typeof(object));
				return true;
		}

		return TrySetIndexer(ctx, target, index, value);
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

	private static bool TryGetIndexerMember(DataContext ctx, object target, string member, out DataTag? value)
	{
		foreach (var indexer in GetIndexers(target.GetType()))
		{
			var parameter = indexer.GetIndexParameters()[0];
			var converted = ConvertToType(ctx, new StringTag(member), parameter.ParameterType);
			if (converted is null) continue;

			value = ToAccessTag(ctx, indexer.GetValue(target, [converted]), indexer.PropertyType);
			return true;
		}

		value = null;
		return false;
	}

	private static bool TrySetIndexerMember(DataContext ctx, object target, string member, DataTag value)
	{
		foreach (var indexer in GetIndexers(target.GetType()))
		{
			if (!indexer.CanWrite) continue;
			var parameter = indexer.GetIndexParameters()[0];
			var converted = ConvertToType(ctx, new StringTag(member), parameter.ParameterType);
			if (converted is null) continue;
			indexer.SetValue(target, ctx.FromValueTag(value, indexer.PropertyType), [converted]);
			return true;
		}

		return false;
	}

	private static bool TryGetIndexer(DataContext ctx, object target, DataTag index, out DataTag? value)
	{
		foreach (var indexer in GetIndexers(target.GetType()))
		{
			var parameter = indexer.GetIndexParameters()[0];
			var converted = ConvertToType(ctx, index, parameter.ParameterType);
			if (converted is null) continue;

			value = ToAccessTag(ctx, indexer.GetValue(target, [converted]), indexer.PropertyType);
			return true;
		}

		value = null;
		return false;
	}

	private static bool TrySetIndexer(DataContext ctx, object target, DataTag index, DataTag value)
	{
		foreach (var indexer in GetIndexers(target.GetType()))
		{
			if (!indexer.CanWrite) continue;
			var parameter = indexer.GetIndexParameters()[0];
			var converted = ConvertToType(ctx, index, parameter.ParameterType);
			if (converted is null) continue;
			indexer.SetValue(target, ctx.FromValueTag(value, indexer.PropertyType), [converted]);
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

	private static object? ConvertToType(DataContext ctx, DataTag tag, Type targetType)
	{
		if (targetType == typeof(string) && tag is StringTag stringTag) return stringTag.Value;
		return ctx.FromValueTag(tag, targetType);
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

	private static DataTag ToAccessTag(DataContext ctx, object? value, Type declaredType)
	{
		if (value is null) return new NullTag();
		if (value is DataTag tag) return tag;
		if (value is string or int or float or double or long or bool or Guid)
			return ctx.ToValueTag(value, declaredType) ?? new NullTag();
		if (declaredType.IsEnum) return new StringTag(value.ToString() ?? "");
		if (ctx.HasConverter(declaredType) || ctx.HasConverter(value.GetType()))
			return ctx.ToValueTag(value, declaredType) ?? new NullTag();
		if (value is IDictionary) return new ObjectRefTag(value);
		if (value is IEnumerable) return ctx.ToValueTag(value, declaredType) ?? new NullTag();
		return new ObjectRefTag(value);
	}

	private static string Normalize(string value) => value.Replace("_", "").ToLowerInvariant();
}

using System.Collections;
using System.Reflection;
using CardWars.BattleEngine.State;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Mapping;
using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;

namespace CardWars.BattleEngine.Behaviour.DataDriven.Objects;

public sealed class ReflectionObjectAccessor : IObjectAccessor
{
	public bool TryGetMember(object target, string member, out DataTag? value)
	{
		var property = FindProperty(target.GetType(), member);
		if (property != null)
		{
			value = ToTag(property.GetValue(target), property.PropertyType);
			return value != null;
		}

		if (TryGetDataBag(target, out var data) && TryGetDataEntry(data, member, out value))
			return true;

		value = null;
		return false;
	}

	public bool TrySetMember(object target, string member, DataTag value)
	{
		var property = FindProperty(target.GetType(), member);
		if (property is { CanWrite: true })
		{
			var converted = DataTagMapper.FromValueTag(value, property.PropertyType);
			if (converted == null) return false;
			property.SetValue(target, converted);
			return true;
		}

		if (TryGetDataBag(target, out var data))
		{
			data.Set(member, value);
			return true;
		}

		return false;
	}

	public bool TryGetIndex(object target, DataTag index, out DataTag? value)
	{
		if (target is IDictionary dictionary)
		{
			var key = ConvertToType(index, GetDictionaryKeyType(target.GetType()));
			if (key == null || !dictionary.Contains(key))
			{
				value = null;
				return false;
			}

			value = ToTag(dictionary[key], GetDictionaryValueType(target.GetType()));
			return value != null;
		}

		if (target is IList list && index is IntTag intTag)
		{
			if (intTag.Value < 0 || intTag.Value >= list.Count)
			{
				value = null;
				return false;
			}

			value = ToTag(list[intTag.Value], typeof(object));
			return value != null;
		}

		value = null;
		return false;
	}

	public bool TrySetIndex(object target, DataTag index, DataTag value)
	{
		if (target is IDictionary dictionary)
		{
			var key = ConvertToType(index, GetDictionaryKeyType(target.GetType()));
			if (key == null) return false;
			dictionary[key] = DataTagMapper.FromValueTag(value, GetDictionaryValueType(target.GetType()));
			return true;
		}

		if (target is IList list && index is IntTag intTag)
		{
			if (intTag.Value < 0 || intTag.Value >= list.Count) return false;
			var converted = DataTagMapper.FromValueTag(value, typeof(object));
			list[intTag.Value] = converted;
			return true;
		}

		return false;
	}

	private static PropertyInfo? FindProperty(Type type, string member)
	{
		var normalized = Normalize(member);
		foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
		{
			if (property.GetIndexParameters().Length > 0) continue;
			var key = property.GetCustomAttribute<DataTagAttribute>()?.Key ?? property.Name;
			if (Normalize(key) == normalized || Normalize(property.Name) == normalized)
				return property;
		}

		return null;
	}

	private static bool TryGetDataBag(object target, out CompoundTag data)
	{
		var property = target.GetType().GetProperty("Data", BindingFlags.Public | BindingFlags.Instance);
		if (property?.GetValue(target) is CompoundTag compound)
		{
			data = compound;
			return true;
		}

		data = null!;
		return false;
	}

	private static bool TryGetDataEntry(CompoundTag data, string member, out DataTag? value)
	{
		if (data.Has(member))
		{
			value = data.Get(member);
			return value != null;
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
		if (targetType == typeof(string) && tag is StringTag textTag) return textTag.Value;
		if (targetType == typeof(EntityId)) return tag switch
		{
			GuidTag guidTag => new EntityId(guidTag.Value),
			StringTag stringTag when Guid.TryParse(stringTag.Value, out var guid) => new EntityId(guid),
			_ => (EntityId?)null
		};
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

	private static DataTag? ToTag(object? value, Type declaredType)
	{
		if (value == null) return null;
		if (value is DataTag tag) return tag;
		if (value is EntityId entityId) return new GuidTag(entityId.Value);
		if (value is ResourceId resourceId) return new StringTag(resourceId.ToString());
		if (value is string or int or float or double or long or bool or Guid)
			return DataTagMapper.ToValueTag(value, declaredType);
		if (declaredType.IsEnum) return new StringTag(value.ToString() ?? "");
		if (value is IEnumerable) return DataTagMapper.ToValueTag(value, declaredType);
		return new ObjectRefTag(value);
	}

	private static string Normalize(string value) => value.Replace("_", "").ToLowerInvariant();
}

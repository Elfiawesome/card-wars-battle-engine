using System.Collections;
using System.Reflection;
using CardWars.Core.Data.Converters;
using CardWars.Core.Data.Global;
using CardWars.Core.Data.Tags;

namespace CardWars.Core.Data.Mapping.Internal;

internal static class TagSerializer
{
	public static CompoundTag SerializeObject(object obj, Type declaredType, bool includeType = true)
	{
		if (obj is CompoundTag existing) return (CompoundTag)existing.Clone();

		var compound = new CompoundTag();
		var runtimeType = obj.GetType();

		if (includeType)
		{
			var runtimeTypeId = DataTagTypeRegistry.GetTypeId(runtimeType);
			if (runtimeTypeId != null)
			{
				compound.Set(DataTagMapper.TypeDiscriminatorKey, runtimeTypeId);
			}
		}

		var metadata = TypeMetadataCache.Get(declaredType);
		foreach (var prop in metadata.Properties)
		{
			if (!prop.PropertyInfo.CanRead) continue;

			var val = prop.PropertyInfo.GetValue(obj);
			if (val == null) continue;

			var tag = SerializeValue(val, prop.PropertyInfo.PropertyType, prop.Converter);
			if (tag != null)
			{
				compound.Set(prop.TagKey, tag);
			}
		}

		return compound;
	}

	public static DataTag? SerializeValue(object? value, Type declaredType, IDataTagConverter? propertyConverter = null)
	{
		if (value == null) return null;
		if (value is DataTag directTag) return directTag.Clone();

		var actualType = Nullable.GetUnderlyingType(declaredType) ?? declaredType;

		// 1. Property or Type Converters
		if (propertyConverter != null) return propertyConverter.ToTag(value);
		if (DataTagMapper.TryGetConverter(actualType, out var typeConverter) ||
			DataTagMapper.TryGetConverter(value.GetType(), out typeConverter))
		{
			return typeConverter.ToTag(value);
		}

		// 2. Dictionaries (Checked BEFORE generic IEnumerable)
		if (value is IDictionary dict)
		{
			return SerializeDictionary(dict);
		}

		// 3. Primitives
		switch (value)
		{
			case int i: return new IntTag(i);
			case float f: return new FloatTag(f);
			case double d: return new FloatTag((float)d);
			case long l: return new IntTag((int)l);
			case bool b: return new BoolTag(b);
			case string s: return new StringTag(s);
			case Guid g: return new GuidTag(g);
		}

		// 4. Enums
		if (actualType.IsEnum)
			return new StringTag(value.ToString()!);

		// 5. Collections
		if (value is IEnumerable list)
			return SerializeList(list, actualType);

		// 6. Complex objects / Single-value wrappers
		return SerializeComplexValue(value);
	}

	private static DataTag SerializeDictionary(IDictionary dict)
	{
		var dictType = dict.GetType();
		var keyType = dictType.IsGenericType ? dictType.GetGenericArguments()[0] : typeof(object);

		// String-keyed: CompoundTag
		if (keyType == typeof(string))
		{
			var compound = new CompoundTag();
			foreach (DictionaryEntry entry in dict)
			{
				if (entry.Key is string keyStr)
				{
					var valTag = SerializeValue(entry.Value, entry.Value?.GetType() ?? typeof(object));
					if (valTag != null) compound.Set(keyStr, valTag);
				}
			}
			return compound;
		}

		// Object-keyed: ListTag of { "k": ..., "v": ... }
		var listTag = new ListTag();
		foreach (DictionaryEntry entry in dict)
		{
			var kTag = SerializeValue(entry.Key, entry.Key?.GetType() ?? typeof(object));
			var vTag = SerializeValue(entry.Value, entry.Value?.GetType() ?? typeof(object));

			if (kTag != null && vTag != null)
			{
				listTag.Add(new CompoundTag().Set("k", kTag).Set("v", vTag));
			}
		}
		return listTag;
	}

	private static ListTag SerializeList(IEnumerable list, Type collectionType)
	{
		var elemType = GetElementType(collectionType) ?? typeof(object);
		var tagList = new ListTag();
		foreach (var item in list)
		{
			if (item == null) continue;
			var tag = SerializeValue(item, elemType);
			if (tag != null) tagList.Add(tag);
		}
		return tagList;
	}

	private static DataTag SerializeComplexValue(object value)
	{
		var type = value.GetType();

		// Single-value wrapper (e.g. EntityId wrapping Guid)
		var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
		if (props.Length == 1 && props[0].PropertyType == typeof(Guid))
			return new GuidTag((Guid)props[0].GetValue(value)!);

		if (type.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static, [typeof(string)]) != null)
			return new StringTag(value.ToString() ?? string.Empty);

		return SerializeObject(value, type);
	}

	public static Type? GetElementType(Type collectionType)
	{
		if (collectionType.IsArray) return collectionType.GetElementType();
		if (collectionType.IsGenericType) return collectionType.GetGenericArguments().FirstOrDefault();
		return collectionType.GetInterfaces()
			.FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
			?.GetGenericArguments().FirstOrDefault();
	}
}
using System.Collections;
using System.Reflection;
using CardWars.Core.Data.Global;
using CardWars.Core.Data.Tags;

namespace CardWars.Core.Data.Mapping.Internal;

internal static class TagDeserializer
{
	public static object? Deserialize(DataTag? tag, Type targetType)
	{
		if (tag == null) return GetDefault(targetType);
		if (typeof(DataTag).IsAssignableFrom(targetType)) return tag.Clone();

		var actualType = Nullable.GetUnderlyingType(targetType) ?? targetType;

		// 1. Dynamic object resolution
		if (actualType == typeof(object)) return DeserializeDynamic(tag);

		// 2. Custom converters
		if (DataTagMapper.TryGetConverter(actualType, out var converter))
			return converter.FromTag(tag);

		// 3. Dictionaries
		if (typeof(IDictionary).IsAssignableFrom(actualType))
			return DeserializeDictionary(tag, actualType);

		// 4. Tolerant primitive parsing (from Senior's implementation)
		if (actualType == typeof(int)) return tag switch
		{
			IntTag it => it.Value,
			FloatTag ft => (int)ft.Value,
			StringTag st when int.TryParse(st.Value, out var v) => v,
			_ => 0
		};
		if (actualType == typeof(float)) return tag switch
		{
			FloatTag ft => ft.Value,
			IntTag it => (float)it.Value,
			StringTag st when float.TryParse(st.Value, out var v) => v,
			_ => 0f
		};
		if (actualType == typeof(double)) return tag switch
		{
			FloatTag ft => (double)ft.Value,
			IntTag it => (double)it.Value,
			StringTag st when double.TryParse(st.Value, out var v) => v,
			_ => 0.0
		};
		if (actualType == typeof(long)) return tag switch
		{
			IntTag it => (long)it.Value,
			StringTag st when long.TryParse(st.Value, out var v) => v,
			_ => 0L
		};
		if (actualType == typeof(string)) return tag switch
		{
			StringTag st => st.Value,
			IntTag it => it.Value.ToString(),
			FloatTag ft => ft.Value.ToString(),
			BoolTag bt => bt.Value.ToString(),
			GuidTag gt => gt.Value.ToString(),
			_ => null
		};
		if (actualType == typeof(bool)) return tag switch
		{
			BoolTag bt => bt.Value,
			IntTag it => it.Value != 0,
			StringTag st when bool.TryParse(st.Value, out var v) => v,
			_ => false
		};
		if (actualType == typeof(Guid)) return tag switch
		{
			GuidTag gt => gt.Value,
			StringTag st when Guid.TryParse(st.Value, out var g) => g,
			_ => Guid.Empty
		};

		// 5. Enums
		if (actualType.IsEnum && tag is StringTag es)
		{
			return Enum.TryParse(actualType, es.Value, ignoreCase: true, out var ev) ? ev : GetDefault(actualType);
		}

		// 6. Parseables & Single-value wrappers
		if (tag is StringTag stringTag)
		{
			var parseMethod = actualType.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static, [typeof(string)]);
			if (parseMethod != null) return parseMethod.Invoke(null, [stringTag.Value]);
		}
		if (tag is GuidTag guidTag)
		{
			var ctor = actualType.GetConstructor([typeof(Guid)]);
			if (ctor != null) return ctor.Invoke([guidTag.Value]);
		}

		// 7. Collections
		if (tag is ListTag listTag && IsCollectionType(actualType))
			return DeserializeList(listTag, actualType);

		// 8. Compounds
		if (tag is CompoundTag compound)
			return DeserializeCompound(compound, actualType);

		return GetDefault(targetType);
	}

	private static object? DeserializeDynamic(DataTag tag) => tag switch
	{
		IntTag it => it.Value,
		FloatTag ft => ft.Value,
		BoolTag bt => bt.Value,
		StringTag st => st.Value,
		GuidTag gt => gt.Value,
		CompoundTag ct => DeserializeCompound(ct, typeof(object)),
		ListTag lt => DeserializeList(lt, typeof(List<object>)),
		_ => tag
	};

	private static object DeserializeCompound(CompoundTag compound, Type targetType)
	{
		// Polymorphic resolution
		var concreteType = targetType;
		var typeId = compound.GetString(DataTagMapper.TypeDiscriminatorKey);
		if (!string.IsNullOrEmpty(typeId))
		{
			var resolved = DataTagTypeRegistry.GetType(typeId);
			if (resolved != null && (targetType == typeof(object) || targetType.IsAssignableFrom(resolved)))
			{
				concreteType = resolved;
			}
		}
		else if (targetType == typeof(object))
		{
			return DeserializeDictionary(compound, typeof(Dictionary<string, object>));
		}

		if (concreteType.IsInterface || concreteType.IsAbstract)
		{
			throw new InvalidOperationException($"Cannot deserialize abstract/interface '{concreteType.Name}' without a valid registered type discriminator.");
		}

		var meta = TypeMetadataCache.Get(concreteType);
		object instance;

		// Constructor instantiation
		if (meta.Constructor != null && meta.ConstructorParams.Length > 0)
		{
			var args = new object?[meta.ConstructorParams.Length];
			for (int i = 0; i < meta.ConstructorParams.Length; i++)
			{
				var param = meta.ConstructorParams[i];
				var paramTag = compound.Get(param.TagKey);

				if (paramTag != null)
					args[i] = Deserialize(paramTag, param.ParamType);
				else if (param.HasDefault)
					args[i] = param.DefaultValue;
				else
					args[i] = GetDefault(param.ParamType);
			}
			instance = meta.Constructor.Invoke(args);
		}
		else
		{
			instance = meta.Constructor?.Invoke(null) ?? Activator.CreateInstance(concreteType)!;
		}

		// Property population (Includes Senior's read-only collection populator)
		foreach (var prop in meta.Properties)
		{
			if (prop.IsConstructorBound) continue;

			var propTag = compound.Get(prop.TagKey);
			if (propTag == null) continue;

			if (prop.PropertyInfo.CanWrite)
			{
				var val = prop.Converter != null
					? prop.Converter.FromTag(propTag)
					: Deserialize(propTag, prop.PropertyInfo.PropertyType);

				prop.PropertyInfo.SetValue(instance, val);
			}
			else if (propTag is ListTag listTag && IsCollectionType(prop.PropertyInfo.PropertyType))
			{
				var existing = prop.PropertyInfo.GetValue(instance);
				PopulateExistingCollection(existing, listTag, prop.PropertyInfo.PropertyType);
			}
		}

		return instance;
	}

	private static object DeserializeDictionary(DataTag tag, Type targetType)
	{
		Type keyType = typeof(object);
		Type valType = typeof(object);

		if (targetType.IsGenericType)
		{
			var args = targetType.GetGenericArguments();
			keyType = args[0];
			valType = args[1];
		}

		var concreteDictType = targetType.IsInterface || targetType.IsAbstract
			? typeof(Dictionary<,>).MakeGenericType(keyType, valType)
			: targetType;

		var dict = (IDictionary)Activator.CreateInstance(concreteDictType)!;

		if (tag is CompoundTag compound)
		{
			foreach (var (key, valTag) in compound.Entries)
			{
				if (key == DataTagMapper.TypeDiscriminatorKey) continue;
				var k = keyType == typeof(string) || keyType == typeof(object) ? key : Deserialize(new StringTag(key), keyType);
				dict[k!] = Deserialize(valTag, valType);
			}
		}
		else if (tag is ListTag listTag)
		{
			for (int i = 0; i < listTag.Count; i++)
			{
				if (listTag.Get(i) is CompoundTag pairTag)
				{
					var k = Deserialize(pairTag.Get("k"), keyType);
					var v = Deserialize(pairTag.Get("v"), valType);
					if (k != null) dict[k] = v;
				}
			}
		}

		return dict;
	}

	private static object? DeserializeList(ListTag listTag, Type targetType)
	{
		var elemType = TagSerializer.GetElementType(targetType) ?? typeof(object);

		if (targetType.IsArray)
		{
			var array = Array.CreateInstance(elemType, listTag.Count);
			for (int i = 0; i < listTag.Count; i++)
			{
				array.SetValue(Deserialize(listTag.Get(i), elemType), i);
			}
			return array;
		}

		var genericDef = targetType.IsGenericType ? targetType.GetGenericTypeDefinition() : null;
		if (genericDef == typeof(List<>) || genericDef == typeof(IList<>) || genericDef == typeof(IReadOnlyList<>) || genericDef == typeof(IEnumerable<>))
		{
			var listType = typeof(List<>).MakeGenericType(elemType);
			var list = (IList)Activator.CreateInstance(listType, listTag.Count)!;
			foreach (var item in listTag.Items)
			{
				list.Add(Deserialize(item, elemType));
			}
			return list;
		}

		if (genericDef == typeof(HashSet<>) || genericDef == typeof(ISet<>))
		{
			var setType = typeof(HashSet<>).MakeGenericType(elemType);
			var set = Activator.CreateInstance(setType)!;
			var addMethod = setType.GetMethod("Add");
			foreach (var item in listTag.Items)
			{
				var val = Deserialize(item, elemType);
				if (val != null) addMethod?.Invoke(set, [val]);
			}
			return set;
		}

		return null;
	}

	private static void PopulateExistingCollection(object? existing, ListTag listTag, Type collectionType)
	{
		if (existing == null) return;
		var elemType = TagSerializer.GetElementType(collectionType) ?? typeof(object);

		if (existing is IList list)
		{
			foreach (var item in listTag.Items)
				list.Add(Deserialize(item, elemType));
			return;
		}

		var addMethod = existing.GetType().GetMethod("Add");
		if (addMethod != null)
		{
			foreach (var item in listTag.Items)
			{
				var val = Deserialize(item, elemType);
				if (val != null) addMethod.Invoke(existing, [val]);
			}
		}
	}

	private static bool IsCollectionType(Type type)
	{
		if (type == typeof(string)) return false;
		if (type.IsArray) return true;
		return typeof(IEnumerable).IsAssignableFrom(type);
	}

	private static object? GetDefault(Type type) => type.IsValueType ? Activator.CreateInstance(type) : null;
}
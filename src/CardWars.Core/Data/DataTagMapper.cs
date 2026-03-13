using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;

namespace CardWars.Core.Data;

public static class DataTagMapper
{
	private static readonly ConcurrentDictionary<Type, TypeMeta> _metaCache = new();
	private static readonly ConcurrentDictionary<Type, IDataTagConverter?> _converterCache = new();

	// ======================== PUBLIC API ========================

	/// <summary>Converts an object to a CompoundTag.</summary>
	public static CompoundTag ToTag(object obj)
	{
		if (obj is CompoundTag existing)
			return (CompoundTag)existing.Clone();

		var tag = new CompoundTag();
		var type = obj.GetType();

		var typeId = DataTagTypeRegistry.GetTypeId(type);
		if (typeId != null)
			tag.Set("_type", typeId);

		var meta = GetOrBuildMeta(type);
		foreach (var prop in meta.Properties)
		{
			var value = prop.PropertyInfo.GetValue(obj);
			if (value == null) continue;

			var dataTag = ObjectToTag(value, prop.PropertyInfo.PropertyType);
			if (dataTag != null)
				tag.Set(prop.Key, dataTag);
		}

		return tag;
	}

	/// <summary>Converts a CompoundTag to T.</summary>
	public static T FromTag<T>(CompoundTag tag) => (T)FromTag(tag, typeof(T));

	/// <summary>Deserialize a CompoundTag to the specified type.</summary>
	public static object FromTag(CompoundTag tag, Type targetType)
	{
		var resolvedType = ResolveType(tag, targetType);
		var meta = GetOrBuildMeta(resolvedType);

		object instance;

		// --- Constructor-based (records, primary constructors) ---
		if (meta.Constructor != null && meta.ConstructorParams.Length > 0)
		{
			var args = new object?[meta.ConstructorParams.Length];
			for (int i = 0; i < meta.ConstructorParams.Length; i++)
			{
				var param = meta.ConstructorParams[i];
				var dataTag = tag.Get(param.Key);

				if (dataTag != null)
					args[i] = TagToObject(dataTag, param.ParamType);
				else if (param.HasDefault)
					args[i] = param.DefaultValue;
				else
					args[i] = GetDefault(param.ParamType);
			}
			instance = meta.Constructor.Invoke(args);
		}
		else
		{
			// --- Parameterless constructor ---
			instance = meta.Constructor?.Invoke(null)
				?? Activator.CreateInstance(resolvedType)
				?? throw new InvalidOperationException(
					$"Cannot create instance of {resolvedType.Name}: no suitable constructor found.");
		}

		// --- Set non-constructor-bound properties ---
		foreach (var prop in meta.Properties)
		{
			if (prop.IsConstructorBound) continue;

			var dataTag = tag.Get(prop.Key);
			if (dataTag == null) continue;

			if (prop.PropertyInfo.CanWrite)
			{
				prop.PropertyInfo.SetValue(instance, TagToObject(dataTag, prop.PropertyInfo.PropertyType));
			}
			else if (dataTag is ListTag listTag && IsCollectionType(prop.PropertyInfo.PropertyType))
			{
				var existing = prop.PropertyInfo.GetValue(instance);
				PopulateExistingCollection(existing, listTag, prop.PropertyInfo.PropertyType);
			}
		}

		return instance;
	}

	// ======================== CONVERSION ========================

	/// <summary>Convert a C# value to a DataTag.</summary>
	public static DataTag? ObjectToTag(object? value, Type declaredType)
	{
		if (value == null) return null;

		var actualType = Nullable.GetUnderlyingType(declaredType) ?? declaredType;

		// Custom converter (check declared type first, then runtime type)
		var converter = GetConverter(actualType) ?? GetConverter(value.GetType());
		if (converter != null) return converter.ToTag(value);

		// DataTag passthrough
		if (value is DataTag dt) return dt.Clone();

		// Built-in primitives
		switch (value)
		{
			case int v: return new IntTag(v);
			case float v: return new FloatTag(v);
			case double v: return new FloatTag((float)v);
			case long v: return new IntTag((int)v);
			case string v: return new StringTag(v);
			case bool v: return new BoolTag(v);
			case Guid v: return new GuidTag(v);
		}

		// Enums → string
		if (actualType.IsEnum)
			return new StringTag(value.ToString()!);

		// Collections → ListTag
		if (value is IEnumerable enumerable)
			return EnumerableToListTag(enumerable, actualType);

		// Complex object → CompoundTag (recursive)
		return ToTag(value);
	}

	/// <summary>Convert a DataTag back to a C# value.</summary>
	public static object? TagToObject(DataTag tag, Type targetType)
	{
		var actualType = Nullable.GetUnderlyingType(targetType) ?? targetType;

		// Custom converter
		var converter = GetConverter(actualType);
		if (converter != null) return converter.FromTag(tag);

		// DataTag passthrough
		if (typeof(DataTag).IsAssignableFrom(actualType))
			return tag.Clone();

		// Built-in primitives (with cross-type tolerance)
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

		// Enums
		if (actualType.IsEnum && tag is StringTag es)
			return Enum.TryParse(actualType, es.Value, ignoreCase: true, out var ev)
				? ev
				: GetDefault(actualType);

		// Collections
		if (tag is ListTag lt && IsCollectionType(actualType))
			return ListTagToNewCollection(lt, actualType);

		// Complex object
		if (tag is CompoundTag ct)
			return FromTag(ct, actualType);

		return GetDefault(targetType);
	}

	// ======================== COLLECTIONS ========================

	private static ListTag EnumerableToListTag(IEnumerable enumerable, Type collectionType)
	{
		var elementType = GetElementType(collectionType) ?? typeof(object);
		var listTag = new ListTag();
		foreach (var item in enumerable)
		{
			if (item == null) continue;
			var itemTag = ObjectToTag(item, elementType);
			if (itemTag != null) listTag.Add(itemTag);
		}
		return listTag;
	}

	private static object? ListTagToNewCollection(ListTag listTag, Type targetType)
	{
		var elementType = GetElementType(targetType);
		if (elementType == null) return null;

		// List<T> and list-like interfaces
		if (targetType.IsGenericType)
		{
			var genericDef = targetType.GetGenericTypeDefinition();

			if (genericDef == typeof(List<>)
				|| genericDef == typeof(IList<>)
				|| genericDef == typeof(IReadOnlyList<>)
				|| genericDef == typeof(IEnumerable<>)
				|| genericDef == typeof(ICollection<>)
				|| genericDef == typeof(IReadOnlyCollection<>))
			{
				var listType = typeof(List<>).MakeGenericType(elementType);
				var list = (IList)Activator.CreateInstance(listType)!;
				foreach (var item in listTag.Items)
				{
					list.Add(TagToObject(item, elementType));
				}
				return list;
			}

			if (genericDef == typeof(HashSet<>) || genericDef == typeof(ISet<>))
			{
				var setType = typeof(HashSet<>).MakeGenericType(elementType);
				var set = Activator.CreateInstance(setType);
				var addMethod = setType.GetMethod("Add");
				foreach (var item in listTag.Items)
				{
					var val = TagToObject(item, elementType);
					if (val != null) addMethod?.Invoke(set, [val]);
				}
				return set;
			}
		}

		// Arrays
		if (targetType.IsArray)
		{
			var arr = Array.CreateInstance(elementType, listTag.Count);
			for (int i = 0; i < listTag.Count; i++)
			{
				arr.SetValue(TagToObject(listTag.Items[i], elementType), i);
			}
			return arr;
		}

		return null;
	}

	private static void PopulateExistingCollection(object? existing, ListTag listTag, Type collectionType)
	{
		if (existing == null) return;
		var elementType = GetElementType(collectionType) ?? typeof(object);

		// IList covers List<T>
		if (existing is IList list)
		{
			foreach (var item in listTag.Items)
				list.Add(TagToObject(item, elementType));
			return;
		}

		// HashSet<T> or anything with an Add method
		var addMethod = existing.GetType().GetMethod("Add");
		if (addMethod != null)
		{
			foreach (var item in listTag.Items)
			{
				var val = TagToObject(item, elementType);
				if (val != null) addMethod.Invoke(existing, [val]);
			}
		}
	}

	// ======================== TYPE RESOLUTION ========================

	private static Type ResolveType(CompoundTag tag, Type declaredType)
	{
		var typeIdStr = tag.GetString("_type");
		if (!string.IsNullOrEmpty(typeIdStr))
		{
			var resolved = DataTagTypeRegistry.GetType(typeIdStr);
			if (resolved != null && declaredType.IsAssignableFrom(resolved))
				return resolved;
		}

		if (declaredType.IsInterface || declaredType.IsAbstract)
		{
			throw new InvalidOperationException(
				$"Cannot deserialize '{declaredType.Name}': " +
				$"no valid '_type' discriminator found in tag. " +
				$"(got '{typeIdStr ?? "<missing>"}')");
		}

		return declaredType;
	}

	// ======================== CONVERTER CACHE ========================

	private static IDataTagConverter? GetConverter(Type type)
	{
		return _converterCache.GetOrAdd(type, t =>
		{
			var attr = t.GetCustomAttribute<DataTagConverterAttribute>();
			if (attr == null) return null;
			return (IDataTagConverter)Activator.CreateInstance(attr.ConverterType)!;
		});
	}

	// ======================== META CACHE ========================

	private static TypeMeta GetOrBuildMeta(Type type)
		=> _metaCache.GetOrAdd(type, BuildMeta);

	private static TypeMeta BuildMeta(Type type)
	{
		// 1. Gather [DataTag] properties
		var allProps = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
		var taggedProps = new List<PropertyMeta>();

		foreach (var prop in allProps)
		{
			var attr = prop.GetCustomAttribute<DataTagAttribute>();
			if (attr == null || attr.Ignore) continue;

			var key = attr.Key ?? ToSnakeCase(prop.Name);
			taggedProps.Add(new PropertyMeta(prop, key, IsConstructorBound: false));
		}

		// 2. Find best constructor: prefer the one with the most parameters
		//    where every parameter matches a tagged property key or has a default value
		ConstructorInfo? bestCtor = null;
		ConstructorParamMeta[] bestCtorParams = [];

		var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

		foreach (var ctor in constructors.OrderByDescending(c => c.GetParameters().Length))
		{
			var parameters = ctor.GetParameters();
			if (parameters.Length == 0) continue;

			var paramMetas = new List<ConstructorParamMeta>();
			bool allMatch = true;

			foreach (var param in parameters)
			{
				// TODO: Use the var attr = param.GetCustomAttribute<DataTagAttribute>(); instead???
				var paramKey = ToSnakeCase(param.Name ?? "");
				var matchingProp = taggedProps.FirstOrDefault(p => p.Key == paramKey);

				if (matchingProp != null)
				{
					paramMetas.Add(new ConstructorParamMeta(
						paramKey, param.ParameterType,
						param.HasDefaultValue, param.HasDefaultValue ? param.DefaultValue : null));
				}
				else if (param.HasDefaultValue)
				{
					paramMetas.Add(new ConstructorParamMeta(
						paramKey, param.ParameterType,
						true, param.DefaultValue));
				}
				else
				{
					allMatch = false;
					break;
				}
			}

			if (allMatch)
			{
				bestCtor = ctor;
				bestCtorParams = paramMetas.ToArray();
				break; // sorted descending, first full match is best
			}
		}

		// Fall back to parameterless constructor
		bestCtor ??= constructors.FirstOrDefault(c => c.GetParameters().Length == 0);

		// 3. Mark properties bound to the chosen constructor
		if (bestCtorParams.Length > 0)
		{
			var ctorKeys = new HashSet<string>(bestCtorParams.Select(p => p.Key));
			taggedProps = taggedProps
				.Select(p => p with { IsConstructorBound = ctorKeys.Contains(p.Key) })
				.ToList();
		}

		return new TypeMeta(taggedProps.ToArray(), bestCtor, bestCtorParams);
	}

	// ======================== HELPERS ========================

	internal static string ToSnakeCase(string name)
	{
		if (string.IsNullOrEmpty(name)) return name;

		var sb = new StringBuilder(name.Length + 4);
		for (int i = 0; i < name.Length; i++)
		{
			var c = name[i];
			if (i > 0 && char.IsUpper(c))
			{
				bool prevIsLower = char.IsLower(name[i - 1]);
				bool nextIsLower = i + 1 < name.Length && char.IsLower(name[i + 1]);
				if (prevIsLower || nextIsLower)
					sb.Append('_');
			}
			sb.Append(char.ToLowerInvariant(c));
		}
		return sb.ToString();
	}

	private static object? GetDefault(Type type)
		=> type.IsValueType ? Activator.CreateInstance(type) : null;

	private static bool IsCollectionType(Type type)
	{
		if (type == typeof(string)) return false;
		if (type.IsArray) return true;
		if (!type.IsGenericType)
			return typeof(IEnumerable).IsAssignableFrom(type);

		var def = type.GetGenericTypeDefinition();
		return def == typeof(List<>)
			|| def == typeof(HashSet<>)
			|| def == typeof(IList<>)
			|| def == typeof(IReadOnlyList<>)
			|| def == typeof(IEnumerable<>)
			|| def == typeof(ICollection<>)
			|| def == typeof(IReadOnlyCollection<>)
			|| def == typeof(ISet<>);
	}

	private static Type? GetElementType(Type collectionType)
	{
		if (collectionType.IsArray)
			return collectionType.GetElementType();
		if (collectionType.IsGenericType)
			return collectionType.GetGenericArguments().FirstOrDefault();
		// Check implemented interfaces for IEnumerable<T>
		var enumInterface = collectionType.GetInterfaces()
			.FirstOrDefault(i => i.IsGenericType
				&& i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
		return enumInterface?.GetGenericArguments().FirstOrDefault();
	}

	// ======================== INTERNAL TYPES ========================

	private record TypeMeta(
		PropertyMeta[] Properties,
		ConstructorInfo? Constructor,
		ConstructorParamMeta[] ConstructorParams
	);

	private record PropertyMeta(
		PropertyInfo PropertyInfo,
		string Key,
		bool IsConstructorBound
	);

	private record ConstructorParamMeta(
		string Key,
		Type ParamType,
		bool HasDefault,
		object? DefaultValue
	);
}
using System.Collections.Concurrent;
using System.Reflection;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Converters;
using CardWars.Core.Data.Mapping.Internal;
using CardWars.Core.Data.Tags;

namespace CardWars.Core.Data.Mapping;

public static class DataTagMapper
{
	/// <summary>
	/// The discriminator key used for polymorphic types.
	/// Defaulted to "_type" to match your senior's convention.
	/// </summary>
	public static string TypeDiscriminatorKey { get; set; } = "_type";

	private static readonly ConcurrentDictionary<Type, IDataTagConverter> _globalConverters = new();

	// ======================== PUBLIC SERIALIZATION ========================

	public static CompoundTag ToTag(object obj, bool includeType = true)
	{
		ArgumentNullException.ThrowIfNull(obj);
		return TagSerializer.SerializeObject(obj, obj.GetType(), includeType);
	}

	public static CompoundTag ToTagAs<T>(T obj, bool includeType = true)
	{
		ArgumentNullException.ThrowIfNull(obj);
		return TagSerializer.SerializeObject(obj, typeof(T), includeType);
	}

	public static CompoundTag ToTagAs(object obj, Type declaredType, bool includeType = true)
	{
		ArgumentNullException.ThrowIfNull(obj);
		ArgumentNullException.ThrowIfNull(declaredType);
		return TagSerializer.SerializeObject(obj, declaredType, includeType);
	}

	// ======================== PUBLIC DESERIALIZATION ========================

	public static T FromTag<T>(CompoundTag tag)
	{
		ArgumentNullException.ThrowIfNull(tag);
		return (T)TagDeserializer.Deserialize(tag, typeof(T))!;
	}

	public static object? FromTag(CompoundTag tag, Type targetType)
	{
		ArgumentNullException.ThrowIfNull(tag);
		ArgumentNullException.ThrowIfNull(targetType);
		return TagDeserializer.Deserialize(tag, targetType);
	}

	// ======================== CONVERTERS ========================

	public static void RegisterConverter<T>(DataTagConverter<T> converter) => _globalConverters[typeof(T)] = converter;
	public static void RegisterConverter(Type type, IDataTagConverter converter) => _globalConverters[type] = converter;

	internal static bool TryGetConverter(Type type, out IDataTagConverter converter)
	{
		if (_globalConverters.TryGetValue(type, out converter!)) return true;

		var attr = type.GetCustomAttribute<DataTagConverterAttribute>();
		if (attr != null)
		{
			converter = TypeMetadataCache.GetConverterInstance(attr.ConverterType);
			return true;
		}

		return false;
	}
}
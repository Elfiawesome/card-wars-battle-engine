using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Context;
using CardWars.Core.Data.Converters;
using CardWars.Core.Data.Mapping.Internal;
using CardWars.Core.Data.Tags;

namespace CardWars.Core.Data.Mapping;

internal sealed class DataTagMapper
{
	private readonly DataTagTypeRegistry _types;
	private readonly TypeMetadataCache _metadata;
	private readonly TagSerializer _serializer;
	private readonly TagDeserializer _deserializer;
	private readonly ConcurrentDictionary<Type, IDataTagConverter> _converters = new();

	internal DataTagMapper(DataTagTypeRegistry types, TypeMetadataCache metadata)
	{
		_types = types;
		_metadata = metadata;
		_serializer = new TagSerializer(this);
		_deserializer = new TagDeserializer(this);
	}

	public string TypeDiscriminatorKey { get; set; } = "_type";

	internal DataTagTypeRegistry Types => _types;
	internal TypeMetadataCache Metadata => _metadata;

	public CompoundTag ToTag(object obj, bool includeType = true)
	{
		ArgumentNullException.ThrowIfNull(obj);
		return _serializer.SerializeObject(obj, obj.GetType(), includeType);
	}

	public CompoundTag ToTagAs<T>(T obj, bool includeType = true)
	{
		ArgumentNullException.ThrowIfNull(obj);
		return _serializer.SerializeObject(obj, typeof(T), includeType);
	}

	public CompoundTag ToTagAs(object obj, Type declaredType, bool includeType = true)
	{
		ArgumentNullException.ThrowIfNull(obj);
		ArgumentNullException.ThrowIfNull(declaredType);
		return _serializer.SerializeObject(obj, declaredType, includeType);
	}

	public T FromTag<T>(CompoundTag tag)
	{
		ArgumentNullException.ThrowIfNull(tag);
		var value = _deserializer.Deserialize(tag, typeof(T))
			?? throw new InvalidOperationException($"Could not deserialize [{typeof(T).Name}].");
		return (T)value;
	}

	public object? FromTag(CompoundTag tag, Type targetType)
	{
		ArgumentNullException.ThrowIfNull(tag);
		ArgumentNullException.ThrowIfNull(targetType);
		return _deserializer.Deserialize(tag, targetType);
	}

	public DataTag? ToValueTag(object? value, Type? declaredType = null)
	{
		if (value is null) return null;
		return _serializer.SerializeValue(value, declaredType ?? value.GetType());
	}

	public object? FromValueTag(DataTag? tag, Type targetType)
		=> _deserializer.Deserialize(tag, targetType);

	public void RegisterConverter<T>(DataTagConverter<T> converter) => _converters[typeof(T)] = converter;

	public void RegisterConverter(Type type, IDataTagConverter converter) => _converters[type] = converter;

	public bool HasConverter(Type type) => TryGetConverter(type, out _);

	internal bool TryGetConverter(Type type, [NotNullWhen(true)] out IDataTagConverter? converter)
	{
		converter = null;
		if (_converters.TryGetValue(type, out var found))
		{
			converter = found;
			return true;
		}

		var attr = type.GetCustomAttribute<DataTagConverterAttribute>();
		if (attr is not null)
		{
			converter = _metadata.GetConverterInstance(attr.ConverterType);
			return true;
		}

		return false;
	}
}

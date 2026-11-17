using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using CardWars.Core.Data.Access;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Converters;
using CardWars.Core.Data.Mapping.Internal;
using CardWars.Core.Data.Serialization.Json;
using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;
using CardWars.Core.Utilities;

namespace CardWars.Core.Data;

/// <summary>
/// The per-instance owner of the data layer's mutable state, introduced in Phase 1
/// of the Card Wars v2 overhaul (doc.md §5). A <see cref="DataContext"/> owns the
/// id&lt;-&gt;Type maps, the converter map, and the polymorphic discriminator key as
/// instance state, so a session or mod can hold its own isolated context instead of
/// reading and writing process-wide static state (AGENT.md R1).
///
/// During Phase 1 the former static classes (DataTagTypeRegistry, DataTagMapper,
/// DataTagSerializer, DataTagAccess) are kept as TEMPORARY compatibility shims that
/// delegate to <see cref="Default"/>, so no caller outside Core changes yet. Later
/// phases thread a specific context through owners and remove those shims.
///
/// Reflection-only caches (TypeMetadataCache; the property/indexer caches below) stay
/// static on purpose: they are keyed solely by <see cref="Type"/> and carry no game or
/// session meaning (AGENT.md allows caches keyed only by Type).
/// </summary>
public sealed class DataContext
{
	/// <summary>
	/// Shared default context that the Phase 1 static facades delegate to. Preserves the
	/// previous process-global behavior until callers are migrated to owned contexts.
	/// </summary>
	public static DataContext Default { get; } = new();

	// ---- Type map (formerly DataTagTypeRegistry's statics) ----
	private readonly ConcurrentDictionary<string, Type> _idToType = new();
	private readonly ConcurrentDictionary<Type, string> _typeToId = new();

	// ---- Converters (formerly DataTagMapper._globalConverters) ----
	private readonly ConcurrentDictionary<Type, IDataTagConverter> _converters = new();

	// ---- JSON transport (formerly DataTagSerializer's static options) ----
	private readonly JsonSerializerOptions _jsonOptions;

	/// <summary>
	/// The discriminator key used for polymorphic types. Defaulted to "_type".
	/// </summary>
	public string TypeDiscriminatorKey { get; set; } = "_type";

	public DataContext()
	{
		_jsonOptions = new JsonSerializerOptions();
		_jsonOptions.Converters.Add(new DataTagJsonConverter());
	}

	// ======================== TYPE REGISTRY ========================

	/// <summary>
	/// Scans an assembly for all types decorated with [DataTagType] and registers them.
	/// Call this when loading mod assemblies.
	/// </summary>
	public void ScanAssembly(Assembly assembly)
	{
		foreach (var type in assembly.GetTypes())
		{
			var attr = type.GetCustomAttribute<DataTagTypeAttribute>();
			if (attr is not null)
			{
				if (attr.Id is null)
				{
					Register(type, ResourceId.Vanilla(NamingUtils.ToSnakeCase(type.Name)).ToString());
				}
				else
				{
					Register(type, attr.Id.ToString() ?? "unknown:unknown");
				}
			}
		}
	}

	public void Register<T>(string id) => Register(typeof(T), id);

	public void Register(Type type, string id)
	{
		_idToType[id] = type;
		_typeToId[type] = id;
	}

	public Type? GetType(string typeId)
		=> _idToType.GetValueOrDefault(typeId);

	public string? GetTypeId(Type type)
		=> _typeToId.GetValueOrDefault(type);

	public void Clear()
	{
		_idToType.Clear();
		_typeToId.Clear();
	}

	// ======================== CONVERTERS ========================

	public void RegisterConverter<T>(DataTagConverter<T> converter) => _converters[typeof(T)] = converter;
	public void RegisterConverter(Type type, IDataTagConverter converter) => _converters[type] = converter;

	public bool HasConverter(Type type) => TryGetConverter(type, out _);

	public bool TryGetConverter(Type type, out IDataTagConverter converter)
	{
		if (_converters.TryGetValue(type, out converter!)) return true;

		var attr = type.GetCustomAttribute<DataTagConverterAttribute>();
		if (attr is not null)
		{
			converter = TypeMetadataCache.GetConverterInstance(attr.ConverterType);
			return true;
		}

		return false;
	}

	// ======================== MAPPING (OBJECT <-> TAG) ========================

	public CompoundTag ToTag(object obj, bool includeType = true)
	{
		ArgumentNullException.ThrowIfNull(obj);
		return TagSerializer.SerializeObject(this, obj, obj.GetType(), includeType);
	}

	public CompoundTag ToTagAs<T>(T obj, bool includeType = true)
	{
		ArgumentNullException.ThrowIfNull(obj);
		return TagSerializer.SerializeObject(this, obj, typeof(T), includeType);
	}

	public CompoundTag ToTagAs(object obj, Type declaredType, bool includeType = true)
	{
		ArgumentNullException.ThrowIfNull(obj);
		ArgumentNullException.ThrowIfNull(declaredType);
		return TagSerializer.SerializeObject(this, obj, declaredType, includeType);
	}

	public T FromTag<T>(CompoundTag tag)
	{
		ArgumentNullException.ThrowIfNull(tag);
		return (T)TagDeserializer.Deserialize(this, tag, typeof(T))!;
	}

	public object? FromTag(CompoundTag tag, Type targetType)
	{
		ArgumentNullException.ThrowIfNull(tag);
		ArgumentNullException.ThrowIfNull(targetType);
		return TagDeserializer.Deserialize(this, tag, targetType);
	}

	public DataTag? ToValueTag(object? value, Type? declaredType = null)
	{
		if (value is null) return null;
		return TagSerializer.SerializeValue(this, value, declaredType ?? value.GetType());
	}

	public object? FromValueTag(DataTag? tag, Type targetType)
		=> TagDeserializer.Deserialize(this, tag, targetType);

	// ======================== JSON SERIALIZATION ========================

	public JsonSerializerOptions JsonSerializerOptions => _jsonOptions;

	public T? Deserialize<T>(string json)
		where T : DataTag
	{
		var tag = JsonSerializer.Deserialize<DataTag>(json, _jsonOptions);
		if (tag is null && string.Equals(json.Trim(), "null", StringComparison.Ordinal))
			tag = new NullTag();
		return tag is T typed ? typed : null;
	}

	public string Serialize(DataTag tag)
		=> JsonSerializer.Serialize(tag, _jsonOptions);

	// ======================== MEMBER / INDEX ACCESS ========================

	public bool TryGetMember(object target, string member, out DataTag? value)
		=> DataTagAccessCore.TryGetMember(this, target, member, out value);

	public bool TrySetMember(object target, string member, DataTag value)
		=> DataTagAccessCore.TrySetMember(this, target, member, value);

	public bool TryGetIndex(object target, DataTag index, out DataTag? value)
		=> DataTagAccessCore.TryGetIndex(this, target, index, out value);

	public bool TrySetIndex(object target, DataTag index, DataTag value)
		=> DataTagAccessCore.TrySetIndex(this, target, index, value);
}

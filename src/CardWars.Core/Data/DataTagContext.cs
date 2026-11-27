using System.Reflection;
using CardWars.Core.Data.Access;
using CardWars.Core.Data.Context;
using CardWars.Core.Data.Converters;
using CardWars.Core.Data.Mapping;
using CardWars.Core.Data.Mapping.Internal;
using CardWars.Core.Data.Serialization.Json;
using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;

namespace CardWars.Core.Data;

public sealed class DataTagContext
{
	private readonly DataTagTypeRegistry _types;
	private readonly TypeMetadataCache _metadata = new();
	private readonly DataTagMapper _mapper;
	private readonly DataTagAccess _access;
	private readonly string _defaultNamespace;

	public DataTagContext(string defaultNamespace)
	{
		_defaultNamespace = defaultNamespace;
		_types = new DataTagTypeRegistry(defaultNamespace);
		_mapper = new DataTagMapper(_types, _metadata);
		_mapper.RegisterConverter(typeof(ResourceId), new ResourceIdTagConverter(defaultNamespace));
		_access = new DataTagAccess(_mapper);
	}

	public ResourceId ParseId(string id) => ResourceId.Parse(id, _defaultNamespace);

	public string TypeDiscriminatorKey
	{
		get => _mapper.TypeDiscriminatorKey;
		set => _mapper.TypeDiscriminatorKey = value;
	}

	public void ScanLoadedAssemblies() => _types.ScanLoadedAssemblies();

	public void ScanAssembly(Assembly assembly) => _types.ScanAssembly(assembly);

	public void Register<T>(string id) => _types.Register<T>(id);

	public void Register(Type type, string id) => _types.Register(type, id);

	public Type? GetType(string typeId) => _types.GetType(typeId);

	public string? GetTypeId(Type type) => _types.GetTypeId(type);

	public void RegisterConverter<T>(DataTagConverter<T> converter) => _mapper.RegisterConverter(converter);

	public void RegisterConverter(Type type, IDataTagConverter converter) => _mapper.RegisterConverter(type, converter);

	public bool HasConverter(Type type) => _mapper.HasConverter(type);

	public CompoundTag ToTag(object obj, bool includeType = true) => _mapper.ToTag(obj, includeType);

	public CompoundTag ToTagAs<T>(T obj, bool includeType = true) => _mapper.ToTagAs(obj, includeType);

	public CompoundTag ToTagAs(object obj, Type declaredType, bool includeType = true)
		=> _mapper.ToTagAs(obj, declaredType, includeType);

	public T FromTag<T>(CompoundTag tag) => _mapper.FromTag<T>(tag);

	public object? FromTag(CompoundTag tag, Type targetType) => _mapper.FromTag(tag, targetType);

	public DataTag? ToValueTag(object? value, Type? declaredType = null)
		=> _mapper.ToValueTag(value, declaredType);

	public object? FromValueTag(DataTag? tag, Type targetType)
		=> _mapper.FromValueTag(tag, targetType);

	public bool TryGetMember(object target, string member, out DataTag? value)
		=> _access.TryGetMember(target, member, out value);

	public bool TrySetMember(object target, string member, DataTag value)
		=> _access.TrySetMember(target, member, value);

	public bool TryGetIndex(object target, DataTag index, out DataTag? value)
		=> _access.TryGetIndex(target, index, out value);

	public bool TrySetIndex(object target, DataTag index, DataTag value)
		=> _access.TrySetIndex(target, index, value);

	public string Serialize(DataTag tag) => DataTagJson.Serialize(tag);

	public T? Deserialize<T>(string json) where T : DataTag => DataTagJson.Deserialize<T>(json);
}

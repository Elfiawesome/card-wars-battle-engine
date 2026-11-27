using System.Collections.Concurrent;
using System.Reflection;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;
using CardWars.Core.Utilities;

namespace CardWars.Core.Data.Context;

internal sealed class DataTagTypeRegistry
{
	private readonly ConcurrentDictionary<string, Type> _idToType = new();
	private readonly ConcurrentDictionary<Type, string> _typeToId = new();
	private readonly string _defaultNamespace;

	public DataTagTypeRegistry(string defaultNamespace)
	{
		_defaultNamespace = defaultNamespace;
	}

	public void ScanLoadedAssemblies()
	{
		var coreAssembly = typeof(DataTag).Assembly;
		var coreName = coreAssembly.GetName().Name;

		foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
		{
			if (assembly != coreAssembly && !assembly.GetReferencedAssemblies().Any(reference => reference.Name == coreName))
				continue;

			ScanAssembly(assembly);
		}
	}

	public void ScanAssembly(Assembly assembly)
	{
		foreach (var type in assembly.GetTypes())
		{
			var attr = type.GetCustomAttribute<DataTagTypeAttribute>();
			if (attr is null) continue;

			if (attr.Id is null)
			{
				Register(type, new ResourceId(_defaultNamespace, NamingUtils.ToSnakeCase(type.Name)).ToString());
			}
			else
			{
				Register(type, ResourceId.Parse(attr.Id, _defaultNamespace).ToString());
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
}

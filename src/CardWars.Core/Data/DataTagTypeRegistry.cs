using System.Collections.Concurrent;
using System.Reflection;
using CardWars.Core.Registry;

namespace CardWars.Core.Data;

public static class DataTagTypeRegistry
{
	private static readonly ConcurrentDictionary<string, Type> _idToType = new();
	private static readonly ConcurrentDictionary<Type, string> _typeToId = new();

	/// <summary>
	/// Scans an assembly for all types decorated with [DataTagType] and registers them.
	/// Call this when loading mod assemblies.
	/// </summary>
	public static void ScanAssembly(Assembly assembly)
	{
		foreach (var type in assembly.GetTypes())
		{
			var attr = type.GetCustomAttribute<DataTagTypeAttribute>();
			if (attr != null)
			{
				if (attr.Id == null)
				{
					Register(type, ResourceId.Vanilla(DataTagSerializer.ToSnakeCase(type.Name)).ToString());
				}
				else
				{
					Register(type, attr.Id.ToString() ?? "unknown:unknown");
				}
			}
		}
	}

	public static void Register<T>(string id) => Register(typeof(T), id);

	public static void Register(Type type, string id)
	{
		_idToType[id] = type;
		_typeToId[type] = id;
	}

	public static Type? GetType(string typeId)
		=> _idToType.GetValueOrDefault(typeId);

	public static string? GetTypeId(Type type)
		=> _typeToId.GetValueOrDefault(type);

	public static void Clear()
	{
		_idToType.Clear();
		_typeToId.Clear();
	}
}
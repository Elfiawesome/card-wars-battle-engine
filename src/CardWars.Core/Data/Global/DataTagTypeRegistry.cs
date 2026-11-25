using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;
using CardWars.Core.Utilities;

namespace CardWars.Core.Data.Global;

// TODO: Replace this process-global registry with a session-owned catalog to satisfy the no-static philosophy.
public static class DataTagTypeRegistry
{
	private static readonly ConcurrentDictionary<string, Type> _idToType = new();
	private static readonly ConcurrentDictionary<Type, string> _typeToId = new();

	public static void ScanLoadedAssemblies()
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

	/// <summary>
	/// Scans an assembly for all types decorated with [DataTagType] and registers them.
	/// Call this when loading mod assemblies.
	/// </summary>
	public static void ScanAssembly(Assembly assembly)
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
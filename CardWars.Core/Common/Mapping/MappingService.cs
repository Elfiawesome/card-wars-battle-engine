using System.Reflection;

namespace CardWars.Core.Common.Mapping;

[AttributeUsage(AttributeTargets.Property)]
public class PropertyMappingAttribute(string Name = "") : Attribute
{
	public string Name = Name;
}

public class MappingService
{
	public Dictionary<Type, Dictionary<string, PropertyInfo>> _mappings = [];
	public void Register<T>()
		where T : notnull
	{
		var type = typeof(T);
		Dictionary<string, PropertyInfo> propertyMap = [];
		var properties = type.GetProperties();
		foreach (var prop in properties)
		{
			if (prop.GetCustomAttribute<PropertyMappingAttribute>() is { } attr)
			{
				if (propertyMap.ContainsKey(attr.Name))
				{
					throw new InvalidOperationException($"Duplicate mapping name '{attr.Name}' found on type '{type.Name}'.");
				}
				propertyMap[attr.Name] = prop;
			}
		}
		_mappings[type] = propertyMap;
	}

	public void SetValue<T>(T instance, string mappedName, object? value)
		where T : notnull
	{
		if (!_mappings.TryGetValue(typeof(T), out var map) || !map.TryGetValue(mappedName, out var prop))
		{
			throw new KeyNotFoundException($"No property mapped to '{mappedName}' found for type '{typeof(T).Name}'. Did you call Register<T>?");
		}

		if (prop.CanWrite)
		{
			prop.SetValue(instance, value);
		}
	}

	public TValue GetValue<TTarget, TValue>(TTarget instance, string mappedName)
		where TTarget : notnull
	{
		var type = typeof(TTarget);

		if (!_mappings.TryGetValue(type, out var map) ||
			!map.TryGetValue(mappedName, out var prop))
		{
			throw new KeyNotFoundException($"Mapping '{mappedName}' not found on '{type.Name}'.");
		}

		object? rawValue = prop.GetValue(instance);
		if (rawValue is null) { return default!; }
		if (rawValue is not TValue) { return default!; }
		
		return (TValue)rawValue;
	}
}
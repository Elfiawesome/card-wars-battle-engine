using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace CardWars.Core.Common.Mapping;

[AttributeUsage(AttributeTargets.Property)]
public class PropertyMappingAttribute(string Name = "") : Attribute
{
	public string Name = Name;
}

public class MappingService
{
	public Dictionary<Type, Dictionary<string, PropertyInfo>> _mappings = [];

	// Debug Print
	public void Print()
	{
		foreach (var item in _mappings)
		{
			var type = item.Key;
			var mapping = item.Value;

			Console.WriteLine($" --- {type.Name} ---");
			foreach (var _m in mapping)
			{
				Console.WriteLine($" -> [{_m.Key}] as `{_m.Value.Name} | {_m.Value.PropertyType.Name}`");
			}
		}
	}

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
				var mappingName = attr.Name;
				if (mappingName == "")
				{
					mappingName = prop.Name;
					// TODO: Haha funnny regex in middle of the code
					mappingName = Regex.Replace(
						Regex.Replace(mappingName, "([a-z0-9])([A-Z])", "$1_$2"),
						"([A-Z])([A-Z][a-z])", "$1_$2"
					).ToLower(CultureInfo.InvariantCulture);
				}

				if (propertyMap.ContainsKey(mappingName))
				{
					throw new InvalidOperationException($"Duplicate mapping name '{mappingName}' found on type '{type.Name}'.");
				}
				propertyMap[mappingName] = prop;
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

	public TValue GetValue<TValue>(object instance, string mappedName)
	{
		var type = instance.GetType();

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
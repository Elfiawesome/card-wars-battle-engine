using System.Collections.Concurrent;
using System.Reflection;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Converters;
using CardWars.Core.Utilities;

namespace CardWars.Core.Data.Mapping.Internal;

internal sealed record PropertyMetadata(
	PropertyInfo PropertyInfo,
	string TagKey,
	IDataTagConverter? Converter,
	bool IsConstructorBound
);

internal sealed record ConstructorParamMetadata(
	string TagKey,
	Type ParamType,
	bool HasDefault,
	object? DefaultValue
);

internal sealed record TypeMetadata(
	Type Type,
	PropertyMetadata[] Properties,
	ConstructorInfo? Constructor,
	ConstructorParamMetadata[] ConstructorParams
);

internal static class TypeMetadataCache
{
	private static readonly ConcurrentDictionary<Type, TypeMetadata> _metadataCache = new();
	private static readonly ConcurrentDictionary<Type, IDataTagConverter> _converterInstances = new();

	public static TypeMetadata Get(Type type) => _metadataCache.GetOrAdd(type, static t => BuildMetadata(t));

	public static IDataTagConverter GetConverterInstance(Type converterType) =>
		_converterInstances.GetOrAdd(converterType, ct => (IDataTagConverter)Activator.CreateInstance(ct)!);

	private static TypeMetadata BuildMetadata(Type type)
	{
		var propList = new List<PropertyMetadata>();
		var seenProps = new HashSet<string>();

		// Correctly traverse interface hierarchies
		var propertyInfos = type.IsInterface
			? new[] { type }.Concat(type.GetInterfaces()).SelectMany(i => i.GetProperties(BindingFlags.Public | BindingFlags.Instance))
			: type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

		foreach (var prop in propertyInfos)
		{
			if (!seenProps.Add(prop.Name)) continue;

			var dataAttr = prop.GetCustomAttribute<DataTagAttribute>();
			if (dataAttr == null || dataAttr.Ignore) continue;

			var tagKey = !string.IsNullOrEmpty(dataAttr.Key) ? dataAttr.Key : NamingUtils.ToSnakeCase(prop.Name);

			IDataTagConverter? converter = null;
			var convAttr = prop.GetCustomAttribute<DataTagConverterAttribute>();
			if (convAttr != null)
			{
				converter = GetConverterInstance(convAttr.ConverterType);
			}

			propList.Add(new PropertyMetadata(prop, tagKey, converter, IsConstructorBound: false));
		}

		// Constructor Matching (Senior's approach: pre-bind matching properties)
		ConstructorInfo? bestCtor = null;
		ConstructorParamMetadata[] bestCtorParams = [];

		var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
		foreach (var ctor in constructors.OrderByDescending(c => c.GetParameters().Length))
		{
			var parameters = ctor.GetParameters();
			if (parameters.Length == 0) continue;

			var paramMetas = new List<ConstructorParamMetadata>();
			bool allMatch = true;

			foreach (var param in parameters)
			{
				var paramAttr = param.GetCustomAttribute<DataTagAttribute>();
				PropertyMetadata? matchingProp;
				string paramKey;

				if (paramAttr?.Key != null)
				{
					paramKey = paramAttr.Key;
					matchingProp = propList.FirstOrDefault(p => p.TagKey == paramKey);
				}
				else
				{
					matchingProp = propList.FirstOrDefault(p =>
						string.Equals(p.PropertyInfo.Name, param.Name, StringComparison.OrdinalIgnoreCase));
					paramKey = matchingProp?.TagKey ?? NamingUtils.ToSnakeCase(param.Name ?? "");
				}

				if (matchingProp != null)
				{
					paramMetas.Add(new ConstructorParamMetadata(paramKey, param.ParameterType, param.HasDefaultValue, param.HasDefaultValue ? param.DefaultValue : null));
				}
				else if (param.HasDefaultValue)
				{
					paramMetas.Add(new ConstructorParamMetadata(paramKey, param.ParameterType, true, param.DefaultValue));
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
				break;
			}
		}

		bestCtor ??= constructors.FirstOrDefault(c => c.GetParameters().Length == 0);

		// Flag properties that are bound to the constructor so we don't double-set them
		if (bestCtorParams.Length > 0)
		{
			var ctorKeys = new HashSet<string>(bestCtorParams.Select(p => p.TagKey));
			propList = propList
				.Select(p => p with { IsConstructorBound = ctorKeys.Contains(p.TagKey) })
				.ToList();
		}

		return new TypeMetadata(type, propList.ToArray(), bestCtor, bestCtorParams);
	}
}
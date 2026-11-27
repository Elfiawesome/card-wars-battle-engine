using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Converters;
using CardWars.Core.Data.Tags;

namespace CardWars.Core.Registry;

[DataTagConverter(typeof(ResourceIdTagConverter))]
public readonly record struct ResourceId(string Namespace, string Path)
{
	public static readonly ResourceId Empty = new("", "");

	public static ResourceId Parse(string id) => Parse(id, string.Empty);

	public static ResourceId Parse(string id, string defaultNamespace)
	{
		var parts = id.Split(':', 2);
		return parts.Length == 2
			? new ResourceId(parts[0], parts[1])
			: new ResourceId(defaultNamespace, parts[0]);
	}

	public override string ToString()
		=> string.IsNullOrEmpty(Namespace) ? Path : $"{Namespace}:{Path}";

	public string ToFlatString()
		=> string.IsNullOrEmpty(Namespace)
			? Path.Replace('/', '_')
			: $"{Namespace}-{Path.Replace('/', '_')}";

	public bool IsEmpty => string.IsNullOrEmpty(Namespace) && string.IsNullOrEmpty(Path);

	public static implicit operator ResourceId(string value) { return Parse(value); }
	public static implicit operator string(ResourceId value) { return value.ToString(); }
}

public class ResourceIdTagConverter(string? defaultNamespace = null) : DataTagConverter<ResourceId>
{
	protected override DataTag? ConvertTo(ResourceId value)
		=> value.IsEmpty ? null : new StringTag(value.ToString());

	protected override ResourceId ConvertFrom(DataTag tag) => tag switch
	{
		StringTag s => ResourceId.Parse(s.Value, defaultNamespace ?? string.Empty),
		_ => ResourceId.Empty
	};
}

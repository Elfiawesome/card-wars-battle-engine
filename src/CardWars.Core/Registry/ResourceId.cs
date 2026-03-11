using CardWars.Core.Data;

namespace CardWars.Core.Registry;

[DataTagConverter(typeof(ResourceIdTagConverter))]
public readonly record struct ResourceId(string Namespace, string Path)
{
	public static readonly ResourceId Empty = new("", "");

	public static ResourceId Vanilla(string path) => new("cardwars", path);

	public static ResourceId Parse(string id)
	{
		var parts = id.Split(':', 2);
		return parts.Length == 2
			? new ResourceId(parts[0], parts[1])
			: new ResourceId("cardwars", parts[0]);
	}

	public override string ToString() => $"{Namespace}:{Path}";

	public bool IsEmpty => string.IsNullOrEmpty(Namespace) && string.IsNullOrEmpty(Path);

	public static implicit operator ResourceId(string value) { return Parse(value); }
}

public class ResourceIdTagConverter : DataTagConverter<ResourceId>
{
	protected override DataTag? ConvertTo(ResourceId value)
		=> value.IsEmpty ? null : new StringTag(value.ToString());

	protected override ResourceId ConvertFrom(DataTag tag) => tag switch
	{
		StringTag s => ResourceId.Parse(s.Value),
		_ => ResourceId.Empty
	};
}
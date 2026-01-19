namespace CardWars.BattleEngine.Core.Registry;

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
}
using CardWars.Core.Data.Attributes;
using CardWars.Core.Registry;

namespace CardWars.Vanilla.Shared;

[DataTagType("player/profile")]
public sealed class PlayerProfile
{
	[DataTag] public List<ResourceId> CurrentDeck { get; set; } = [];
	[DataTag] public List<ResourceId> Heroes { get; set; } = [];

	public PlayerProfile Copy() => new()
	{
		CurrentDeck = [.. CurrentDeck],
		Heroes = [.. Heroes]
	};
}

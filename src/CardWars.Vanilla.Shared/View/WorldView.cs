using CardWars.Core.Data;
using CardWars.Core.Registry;

namespace CardWars.Vanilla.Shared.View;

public record class WorldView
{
	[DataTag] public ResourceId WorldId { get; init; }
	[DataTag] public List<ResourceId> WarpOptions { get; init; } = [];
	[DataTag] public List<PlayerView> Players { get; init; } = [];
}

public record class PlayerView
{
	[DataTag] public Guid Id { get; init; }
	[DataTag] public string Username { get; init; } = "";
	[DataTag] public float X { get; init; }
	[DataTag] public float Y { get; init; }
}

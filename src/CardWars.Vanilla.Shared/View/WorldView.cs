using CardWars.Core.Data;

namespace CardWars.Vanilla.Shared.View;

public record class WorldView
{
	[DataTag] public List<PlayerView> Players { get; init; } = [];
}

public record class PlayerView
{
	[DataTag] public Guid Id { get; init; }
}
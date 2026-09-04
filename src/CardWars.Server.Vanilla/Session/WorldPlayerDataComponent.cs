using CardWars.Core.Data;
using CardWars.Server.Session;

namespace CardWars.Server.Vanilla.Session;

[DataTagType()]
public class WorldPlayerDataComponent : IPlayerDataComponent
{
	// Data relating to world
	[DataTag] public float X { get; set; } = 0;
	[DataTag] public float Y { get; set; } = 0;
	[DataTag] public float Z { get; set; } = 0;
}
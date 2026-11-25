using CardWars.Core.Data.Attributes;
using CardWars.Server.Session;
using CardWars.Vanilla.Shared;

namespace CardWars.Server.Vanilla.Session;

[DataTagType("player_data/profile")]
public class ProfilePlayerDataComponent : IPlayerDataComponent
{
	[DataTag] public PlayerProfile? Profile { get; set; }
}

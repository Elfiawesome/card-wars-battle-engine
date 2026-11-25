using CardWars.Core.Data.Attributes;
using CardWars.Core.Registry;
using CardWars.Server.Session;

namespace CardWars.Server.Vanilla.Session;

[DataTagType("battle/player_data")]
public class BattlePlayerDataComponent : IPlayerDataComponent
{
	[DataTag] public ResourceId LoadoutId { get; set; } = ResourceId.Empty;
}

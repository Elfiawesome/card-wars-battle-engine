using CardWars.Vanilla.Shared;

namespace CardWars.Server.Vanilla;

public enum ProfileAuthority { Server, Client }

public sealed class PlayerProfileRegistry
{
	public PlayerProfile? Starter { get; set; }
	public ProfileAuthority Authority { get; set; } = ProfileAuthority.Server;
}

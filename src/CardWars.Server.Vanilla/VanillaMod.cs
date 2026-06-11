using CardWars.Server.Vanilla.Network.Packet;
using CardWars.ModLoader;
using CardWars.Core.Logging;
using CardWars.Core.Data;

namespace CardWars.Server.Vanilla;

public class VanillaMod : IServerMod
{
	public void OnLoad(ServerRegistry registry, List<ModContentResult> modContents)
	{
		registry.PacketHandlers.Register(new C2S_CustomModPacketHandler());
		registry.PacketHandlers.Register(new C2S_PlayerJoinedRequestResponsePacketHandler());

		RegisterWorldDefinitions(registry, modContents);
	}

	public void RegisterWorldDefinitions(ServerRegistry registry, List<ModContentResult> modContents)
	{
		foreach (var content in modContents)
		{
			switch (content.Category)
			{
				case ["worlds"]:
					var worldDataTag = content.ReadAs<CompoundTag>();
					if (worldDataTag == null) continue;
					Logger.Info("Registered World: " + content.Id.ToString());
					registry.WorldDefinitions.Register(content.Id, worldDataTag);
					break;
			}
		}
	}
}
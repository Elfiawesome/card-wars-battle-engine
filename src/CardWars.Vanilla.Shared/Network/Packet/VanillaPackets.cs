using CardWars.Core.Data;
using CardWars.Core.Network.Packet;

namespace CardWars.Vanilla.Shared.Network.Packet;

public class S2C_PlayerJoinedRequestPacket : IPacket
{
	[DataTag] public string ServerGreetingMessage { get; set; } = "Hello";
	[DataTag] public string Version => "TODO";
}

public class C2S_PlayerJoinedRequestResponsePacket : IPacket
{
	[DataTag] public string ClientGreetingMessage { get; set; } = "Hello"; // Debug Testing
	[DataTag] public required string Username { get; set; }
}

public class S2C_InstanceChangedPacket : IPacket
{
	[DataTag] public Guid InstanceId { get; set; }
	[DataTag] public string InstanceType { get; set; } = "";
}

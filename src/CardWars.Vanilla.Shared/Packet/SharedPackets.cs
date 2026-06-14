using CardWars.Core.Data;
using CardWars.Core.Network.Packet;

namespace CardWars.Vanilla.Shared.Packet;

[DataTagType()]
public class S2C_PlayerJoinedRequestPacket : IPacket
{
	[DataTag] public string ServerGreetingMessage { get; set; } = "Hello";
	[DataTag] public string Version => "TODO";
}

[DataTagType()]
public class C2S_PlayerJoinedRequestResponsePacket : IPacket
{
	[DataTag] public string ClientGreetingMessage { get; set; } = "Hello"; // Debug Testing
	[DataTag] public required string Username { get; set; }
}

[DataTagType()]
public class S2C_InstanceChangedPacket : IPacket
{
	[DataTag] public Guid InstanceId { get; set; }
	[DataTag] public string InstanceType { get; set; } = "";
}

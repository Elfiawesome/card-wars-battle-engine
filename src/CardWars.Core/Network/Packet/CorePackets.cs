using CardWars.Core.Data;

namespace CardWars.Core.Network.Packet;


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




// Modded Packets
public class S2C_CustomModPacket : IPacket
{
	[DataTag] CompoundTag Data { get; set; } = new();
};

public class C2S_CustomModPacket : IPacket
{
	[DataTag] CompoundTag Data { get; set; } = new();
};
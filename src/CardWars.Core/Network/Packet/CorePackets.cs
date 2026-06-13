using CardWars.Core.Data;

namespace CardWars.Core.Network.Packet;

// Generic modded packet escape hatches - mods use these
// when they don't need their own typed packet types.

public class S2C_CustomModPacket : IPacket
{
	[DataTag] public CompoundTag Data { get; set; } = new();
};

public class C2S_CustomModPacket : IPacket
{
	[DataTag] public CompoundTag Data { get; set; } = new();
};

using CardWars.Core.Data;

namespace CardWars.Core.Network.Packet;

[DataTagType()]
public class S2C_CustomModPacket : IPacket
{
	[DataTag] public CompoundTag Data { get; set; } = new();
};

[DataTagType()]
public class C2S_CustomModPacket : IPacket
{
	[DataTag] public CompoundTag Data { get; set; } = new();
};

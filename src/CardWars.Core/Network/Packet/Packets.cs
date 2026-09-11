using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Tags;

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

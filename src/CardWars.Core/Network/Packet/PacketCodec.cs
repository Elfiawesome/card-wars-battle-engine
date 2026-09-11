using CardWars.Core.Data.Mapping;
using CardWars.Core.Data.Serialization.Json;
using CardWars.Core.Data.Tags;

namespace CardWars.Core.Network.Packet;

public static class PacketCodec
{
	public static string Encode(IPacket packet)
		=> DataTagSerializer.Serialize(DataTagMapper.ToTag(packet));

	public static IPacket? Decode(string json)
		=> DataTagMapper.FromTag<IPacket>(DataTagSerializer.Deserialize<CompoundTag>(json)!);
}
using CardWars.Core.Data;

namespace CardWars.Core.Network.Packet;

public static class PacketCodec
{
	public static string Encode(IPacket packet)
		=> DataTagSerializer.Serialize(DataTagMapper.ToTag(packet));

	public static IPacket? Decode(string json)
		=> DataTagMapper.FromTag<IPacket>(DataTagSerializer.Deserialize<CompoundTag>(json)!);
}
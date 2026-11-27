using CardWars.Core.Data;
using CardWars.Core.Data.Tags;

namespace CardWars.Core.Network.Packet;

public sealed class PacketCodec(DataTagContext dataTags)
{
	public string Encode(IPacket packet)
		=> dataTags.Serialize(dataTags.ToTag(packet));

	public IPacket? Decode(string json)
	{
		var tag = dataTags.Deserialize<CompoundTag>(json);
		return tag is null ? null : dataTags.FromTag<IPacket>(tag);
	}
}

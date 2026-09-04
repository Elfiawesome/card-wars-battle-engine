using CardWars.Core.Data;
using CardWars.Core.Network.Packet;
using CardWars.Core.Registry;
using CardWars.Vanilla.Shared.View;

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
public class S2C_ConnectionConfirmedPacket : IPacket
{
	[DataTag] public string Message { get; set; } = "Welcome!";
}

[DataTagType()]
public class S2C_EnterInstancePacket : IPacket
{
	[DataTag] public required Guid PlayerId { get; set; }
}

[DataTagType()]
public class S2C_LeaveInstancePacket : IPacket
{
	[DataTag] public required Guid PlayerId { get; set; }
}

[DataTagType()]
public class S2C_WorldInstanceSnapshot : IPacket
{
	[DataTag] public required WorldView WorldView { get; set; }
	[DataTag] public int Time { get; set; } = 0;
}

[DataTagType()]
public class C2S_MoveInputPacket : IPacket
{
	[DataTag] public float AxisX { get; set; } = 0;
	[DataTag] public float AxisY { get; set; } = 0;
}

[DataTagType()]
public class C2S_DEBUG_WarpRequestPacket : IPacket
{
	[DataTag] public ResourceId TargetWorld { get; set; }
}
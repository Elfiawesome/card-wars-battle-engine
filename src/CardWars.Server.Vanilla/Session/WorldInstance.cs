using CardWars.Core.Data;
using CardWars.Core.Network.Packet;
using CardWars.Core.Registry;
using CardWars.Server.Packet;
using CardWars.Server.Session;
using CardWars.Vanilla.Shared.Packet;
using CardWars.Vanilla.Shared.View;

namespace CardWars.Server.Vanilla.Session;

[DataTagType()]
public class WorldInstance : ServerInstance
{
	public override Guid InstanceId { get; set; }
	[DataTag] public override ResourceId InstanceProviderId { get; set; }

	[DataTag] public ResourceId WorldId { get; set; }
	[DataTag] public int DebugLifespan { get; set; } = 0;
	[DataTag] public CompoundTag Data { get; set; } = new(); // session related data
	public CompoundTag TemplateData { get; set; } = new(); // For reference only. We load the content/server/.../worlds/<file> here

	public override void HandlePacket(PacketContextServer context, IPacket packet)
	{
		switch (packet)
		{
			case C2S_MoveInputPacket moveInputPacket:
				var comp = context.PlayerSession.GetSetComponent<WorldPlayerDataComponent>(InstanceProviderId);
				comp.AxisX = moveInputPacket.AxisX;
				comp.AxisY = moveInputPacket.AxisY;
				break;
		}
	}

	public override void Tick(float deltaTime)
	{
		DebugLifespan++;

		var moved = false;
		foreach (var player in Players)
		{
			var comp = player.GetSetComponent<WorldPlayerDataComponent>(InstanceProviderId);
			if (comp.AxisX == 0f && comp.AxisY == 0f) continue;

			comp.X += comp.AxisX * comp.MoveSpeed * deltaTime;
			comp.Y += comp.AxisY * comp.MoveSpeed * deltaTime;
			moved = true;
		}

		if (moved)
			BroadcastSnapshot();
	}

	public WorldView GetWorldView()
	{
		return new()
		{
			WorldId = WorldId,
			WarpOptions = GetWarpOptions(),
			Players = [.. Players.Select(p =>
			{
				var comp = p.GetSetComponent<WorldPlayerDataComponent>(InstanceProviderId);
				return new PlayerView { Id = p.PlayerId, Username = p.Username, X = comp.X, Y = comp.Y };
			})]
		};
	}

	public void BroadcastSnapshot()
	{
		var view = GetWorldView();
		foreach (var player in Players)
			player.Connection.Send(new S2C_WorldInstanceSnapshot { WorldView = view });
	}

	public List<ResourceId> GetWarpOptions()
	{
		var options = new List<ResourceId>();
		if (TemplateData.GetList("warp_options_debug") is { } list)
		{
			foreach (var item in list.Items)
			{
				if (item is StringTag s)
					options.Add(ResourceId.Parse(s.Value));
			}
		}
		return options;
	}
}

using System.Linq;
using CardWars.Client.scenes.core.game_session;
using CardWars.Client.scripts.core.packet;
using CardWars.Core.Network.Packet;
using CardWars.Vanilla.Shared.Packet;
using Godot;

namespace CardWars.Client.scenes.vanilla.instance;

public partial class WorldInstance : ClientInstance
{
	public override void OnPacket(IPacket packet, PacketContextClient context)
	{
		switch (packet)
		{
			case S2C_WorldInstanceSnapshot worldInstanceSnapshot:
				context.Session.SetDebugWorld(worldInstanceSnapshot.WorldView.WorldId.ToString());
				context.Session.SetDebugPlayers(string.Join("\n", worldInstanceSnapshot.WorldView.Players.Select(p => $"{p.Username} ({p.X:0.0}, {p.Y:0.0})")));
				break;
		}
	}

	public override void _Input(InputEvent @event)
	{
		// C2S_MoveInputPacket
	}

	private Vector2 _axis;
	public override void _Process(double delta)
	{
		Vector2 newAxis = Vector2.Zero;
		if (Input.IsKeyPressed(Key.W) || Input.IsKeyPressed(Key.Up)) newAxis.Y = -1;
		if (Input.IsKeyPressed(Key.S) || Input.IsKeyPressed(Key.Down)) newAxis.Y = 1;
		if (Input.IsKeyPressed(Key.A) || Input.IsKeyPressed(Key.Left)) newAxis.X = -1;
		if (Input.IsKeyPressed(Key.D) || Input.IsKeyPressed(Key.Right)) newAxis.X = 1;

		if (newAxis.X != _axis.X || newAxis.Y != _axis.Y)
		{
			_axis.X = newAxis.X;
			_axis.Y = newAxis.Y;
			var packet = new C2S_MoveInputPacket { AxisX = _axis.X, AxisY = _axis.Y };
			SendPacket(packet);
		}
	}
}

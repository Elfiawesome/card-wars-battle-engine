using System.Collections.Generic;
using System.Linq;
using CardWars.Client.scenes.core.game_session;
using CardWars.Client.scripts.core.packet;
using CardWars.Core.Network.Packet;
using CardWars.Core.Registry;
using CardWars.Vanilla.Shared.Packet;
using Godot;

namespace CardWars.Client.scenes.vanilla.instance.world;

public partial class WorldInstance : ClientInstance
{
	private readonly List<ResourceId> _warpOtions = [];
	public override void OnPacket(IPacket packet, PacketContextClient context)
	{
		switch (packet)
		{
			case S2C_WorldInstanceSnapshot worldInstanceSnapshot:
				context.Session.SetDebugWorld(worldInstanceSnapshot.WorldView.WorldId.ToString());
				context.Session.SetDebugPlayers(string.Join("\n", worldInstanceSnapshot.WorldView.Players.Select(p => $"{p.Username} ({p.X:0.0}, {p.Y:0.0})")));

				_warpOtions.Clear();
				_warpOtions.AddRange(worldInstanceSnapshot.WorldView.WarpOptions);
				break;
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey inputEventKey)
		{
			if (inputEventKey.Pressed && inputEventKey.Keycode == Key.Q)
			{
				if (_warpOtions.Count > 0)
				{
					SendPacket(new C2S_DEBUG_WarpRequestPacket() { TargetWorld = _warpOtions.First() });
				}
			}
			if (inputEventKey.Pressed && inputEventKey.Keycode == Key.B)
			{
				SendPacket(new C2S_DEBUG_EnterBattle());
			}

		}
	}

	private Vector2 _axis = Vector2.Zero;
	public override void _Process(double delta)
	{
		Vector2 newAxis = Vector2.Zero;
		if (Input.IsKeyPressed(Key.W) || Input.IsKeyPressed(Key.Up)) newAxis.Y = -1;
		if (Input.IsKeyPressed(Key.S) || Input.IsKeyPressed(Key.Down)) newAxis.Y = 1;
		if (Input.IsKeyPressed(Key.A) || Input.IsKeyPressed(Key.Left)) newAxis.X = -1;
		if (Input.IsKeyPressed(Key.D) || Input.IsKeyPressed(Key.Right)) newAxis.X = 1;

		if (_axis != newAxis)
		{
			_axis = newAxis;
			var packet = new C2S_MoveInputPacket { AxisX = _axis.X, AxisY = _axis.Y };
			SendPacket(packet);
		}
	}
}

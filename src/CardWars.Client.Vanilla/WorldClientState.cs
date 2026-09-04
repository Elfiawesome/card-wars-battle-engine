using CardWars.Core.Registry;
using CardWars.Vanilla.Shared.Packet;
using CardWars.Vanilla.Shared.View;
using Godot;

namespace CardWars.Client.Vanilla;

public class WorldClientState
{
	public List<ResourceId> WarpOptions { get; private set; } = [];
	public int WarpIndex { get; set; } = 0;

	private float _moveInputX = 0f;
	private float _moveInputY = 0f;
	private float _lastAxisX = 0f;
	private float _lastAxisY = 0f;
	private bool _prevWarpKey = false;

	public void Hook(GameSession session)
	{
		session.OnProcess = () => PollInput(session);
	}

	public void OnSnapshot(WorldView view, GameSession session)
	{
		Hook(session);
		WarpOptions = view.WarpOptions;
		session.SetDebugWorld(view.WorldId.ToString());
		session.SetDebugPlayers(string.Join("\n", view.Players.Select(p => $"{p.Username} ({p.X:0.0}, {p.Y:0.0})")));
	}

	private void PollInput(GameSession session)
	{
		_moveInputX = (Input.IsKeyPressed(Key.D) ? 1f : 0f) - (Input.IsKeyPressed(Key.A) ? 1f : 0f);
		_moveInputY = (Input.IsKeyPressed(Key.S) ? 1f : 0f) - (Input.IsKeyPressed(Key.W) ? 1f : 0f);

		var warpDown = Input.IsKeyPressed(Key.E);
		var warpPressed = warpDown && !_prevWarpKey;
		_prevWarpKey = warpDown;

		if (_moveInputX != _lastAxisX || _moveInputY != _lastAxisY)
		{
			_lastAxisX = _moveInputX;
			_lastAxisY = _moveInputY;
			session.Connection?.Send(new C2S_MoveInputPacket { AxisX = _moveInputX, AxisY = _moveInputY });
		}

		if (warpPressed && WarpOptions.Count > 0)
		{
			var target = WarpOptions[WarpIndex % WarpOptions.Count];
			WarpIndex++;
			session.Connection?.Send(new C2S_DEBUG_WarpRequestPacket { TargetWorld = target });
		}
	}
}

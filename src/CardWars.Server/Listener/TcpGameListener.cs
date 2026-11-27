using System.Net;
using System.Net.Sockets;
using CardWars.Core.Logging;
using CardWars.Core.Network.Packet;
using CardWars.Core.Network.Transport;

namespace CardWars.Server.Listener;

public class TcpGameListener(PacketCodec codec, int port, Logger logger) : IListener
{
	public Action<IConnection>? OnNewConnection { get; set; }

	private TcpListener? _listener;
	private CancellationTokenSource? _cts;

	public void Start()
	{
		_listener = new TcpListener(IPAddress.Any, port);
		_listener.Start();
		_cts = new CancellationTokenSource();
		Task.Run(() => AcceptLoop(_cts.Token));
		logger.Info($"TcpGameListener: Listening on port {port}");
	}

	public void Stop()
	{
		_cts?.Cancel();
		_listener?.Stop();
	}

	private void AcceptLoop(CancellationToken token)
	{
		while (!token.IsCancellationRequested)
		{
			try
			{
				if (_listener is not { } listener) break;
				var client = listener.AcceptTcpClient();
				OnNewConnection?.Invoke(new TcpConnection(client, codec, logger));
			}
			catch when (token.IsCancellationRequested) { break; }
			catch (Exception ex) { logger.Error($"TCP accept failed: {ex.Message}"); }
		}
	}
}

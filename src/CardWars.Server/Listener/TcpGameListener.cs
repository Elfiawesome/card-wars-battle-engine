using System.Net;
using System.Net.Sockets;
using CardWars.Core.Logging;
using CardWars.Core.Network.Transport;

namespace CardWars.Server.Listener;

public class TcpGameListener(int port = 5060) : IListener
{
	public Action<IConnection>? OnNewConnection { get; set; }

	private TcpListener? _listener;
	private CancellationTokenSource? _cts;

	public void Start()
	{
		_listener = new TcpListener(IPAddress.Loopback, port);
		_listener.Start();
		_cts = new CancellationTokenSource();
		Task.Run(() => AcceptLoop(_cts.Token));
		Logger.Info($"TcpGameListener: Listening on port {port}");
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
				var client = _listener!.AcceptTcpClient();
				OnNewConnection?.Invoke(new TcpConnection(client));
			}
			catch when (token.IsCancellationRequested) { break; }
			catch (Exception ex) { Logger.Error($"TCP accept failed: {ex.Message}"); }
		}
	}
}

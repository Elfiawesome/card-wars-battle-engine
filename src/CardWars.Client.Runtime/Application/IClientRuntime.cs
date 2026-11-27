using System;
using CardWars.Core.Network.Transport;

namespace CardWars.Client.Runtime.Application;

public interface IClientRuntime
{
	Action<string>? OnDebugStatus { get; set; }
	Action<string>? OnDebugWorld { get; set; }
	Action<string>? OnDebugPlayers { get; set; }

	void Start();
	void Process();
	void Stop();
}

public interface IClientRuntimeFactory
{
	IClientRuntime Create(SessionContext context, string username, IConnection connection);
}

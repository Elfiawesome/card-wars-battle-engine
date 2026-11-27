using System;
using System.Collections.Generic;
using CardWars.Server;
using CardWars.Server.Listener;
using ServerCore = CardWars.Server.Server; // damn didnt know we can do this lol

namespace CardWars.Client.Runtime.Application;

public sealed class ServerRuntime
{
	private readonly SessionContext _context;
	private readonly IReadOnlyList<IServerMod> _mods;
	private readonly IReadOnlyList<IListener> _listeners;
	private bool _started;

	public ServerRuntime(
		SessionContext context,
		IReadOnlyList<IServerMod> mods,
		IReadOnlyList<IListener> listeners)
	{
		if (context.Storage is not { } storage)
			throw new InvalidOperationException("A server session requires session storage.");

		_context = context;
		_mods = mods;
		_listeners = listeners;

		Server = new ServerCore(storage, context.Logger);
		Server.Registry.RegisterExtension(context.BattleRegistry);
	}

	public ServerCore Server { get; }

	public void Start()
	{
		if (_started)
			throw new InvalidOperationException("Server runtime has already started.");

		// Load mods + content
		foreach (var mod in _mods)
			mod.OnLoad(Server, _context.Content.ServerData);

		Server.Start([.. _listeners]);
		_started = true;
	}

	public void Stop()
	{
		if (!_started) return;

		Server.Stop();
		_started = false;
	}
}

using CardWars.Client.Runtime.Application;
using CardWars.Client.Runtime.Profile;
using CardWars.Core.Network.Transport;
using CardWars.Core.Storage;
using Godot;

namespace CardWars.Client.scripts.core.application;

public sealed class ClientRuntimeFactory : IClientRuntimeFactory
{
	private readonly Node _root;
	private readonly GameApplication _app;

	public ClientRuntimeFactory(Node root, GameApplication app)
	{
		_root = root;
		_app = app;
	}

	public IClientRuntime Create(SessionContext context, string username, IConnection connection)
	{
		var mods = _app.Mods.LoadModEntry<IClientMod>();

		var profileRoot = new StoragePath(
			_app.Provider.Combine(_app.GamedataPath, ".userdata"),
			_app.Provider);
		var profiles = new ClientProfileStore(profileRoot, context.DataTags, context.Logger);

		return new ClientRuntime(_root, context, username, profiles, mods, connection);
	}
}

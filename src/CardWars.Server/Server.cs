using CardWars.Server.Listener;
using CardWars.Server.Transport;

namespace CardWars.Server;

public class Server
{
	public BattleEngine.BattleEngine BattleEngine { get; } = new();
	public ServerRegistry Registry = new();
	public List<IListener> Listeners = [];
	public Dictionary<Guid, IConnection> Connections = [];

	public void LoadMod(IServerMod mod)
	{
		mod.OnLoad(Registry);
	}

	public Server()
	{

	}

	public void PlayerJoin()
	{
		
	}
}
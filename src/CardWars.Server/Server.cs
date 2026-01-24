namespace CardWars.Server;

public class Server
{
	public BattleEngine.BattleEngine BattleEngine { get; } = new();
	public ServerRegistry Registry = new();

	public void LoadMod(IServerMod mod)
	{
		mod.OnLoad(Registry);
	}

	public Server()
	{

	}
}
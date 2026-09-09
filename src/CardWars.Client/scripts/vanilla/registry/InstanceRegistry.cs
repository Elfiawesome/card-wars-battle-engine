using CardWars.BattleEngine;
using CardWars.Client.scenes.core.game_session;
using CardWars.Client.scripts.core;
using CardWars.Client.scripts.core.registry;

namespace CardWars.Client.scripts.vanilla.registry;

public class InstanceSceneRegistry<TId>(ClientRegistry clientRegistry) : SceneRegistry<TId>
	where TId : notnull
{
	private ClientRegistry _clientRegistry = clientRegistry;

	public TInstance? InstantiateInstance<TInstance>(TId id)
		where TInstance : ClientInstance
	{
		var instance = Instantiate<TInstance>(id);
		if (instance == null) { return null; }
		instance.ClientRegistry = _clientRegistry;
		return instance;
	}
}
using System;
using System.Collections.Generic;
using CardWars.BattleEngine.State;
using CardWars.Client.scenes.cardwars.instance.battle;

namespace CardWars.Client.scripts.cardwars.registry;

public interface IEntityViewHandler<TEntity>
	where TEntity : class, IEntity
{
	public void Sync(BattleInstance instance, IReadOnlyCollection<TEntity> entities);
}

public class EntityViewHandlerRegistry
{
	private readonly List<Action<BattleInstance, GameState>> _handlers = [];

	public void Register<TEntity>(IEntityViewHandler<TEntity> handler)
		where TEntity : class, IEntity
	{
		_handlers.Add((instance, state) => handler.Sync(instance, [.. state.OfType<TEntity>()]));
	}

	public void Execute(BattleInstance instance, GameState state)
	{
		foreach (var handler in _handlers) handler.Invoke(instance, state);
	}
}

using System;
using CardWars.BattleEngine.State;
using CardWars.Client.scenes.vanilla.instance.battle;
using CardWars.Core.Logging;
using CardWars.Core.Registry;

namespace CardWars.Client.scripts.vanilla.registry;

public interface IEntityViewHandler<in TEntity>
	where TEntity : IEntity
{
	public void Sync(BattleInstance instance, TEntity entity);
}

public class EntityViewHandlerRegistry : Registry<Type, Action<BattleInstance, IEntity>>
{
	public void Register<TEntity>(IEntityViewHandler<TEntity> handler)
		where TEntity : IEntity
	{
		base.Register(typeof(TEntity), (instance, entity) =>
		{
			if (entity is TEntity typed) handler.Sync(instance, typed);
			else Log.Error($"Entity view handler for {typeof(TEntity)} received an entity of type {entity.GetType().Name}");
		});
	}

	public void Execute(BattleInstance instance, IEntity entity)
	{
		var handler = Get(entity.GetType());
		if (handler == null) { Log.Debug($"No entity view handler for {entity.GetType().Name}"); return; }
		handler.Invoke(instance, entity);
	}
}

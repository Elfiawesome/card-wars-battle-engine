namespace CardWars.BattleEngine;

public abstract class Service
{
	public IServiceContainer ServiceContainer;

	public Service(IServiceContainer container)
	{
		ServiceContainer = container;
	}
}
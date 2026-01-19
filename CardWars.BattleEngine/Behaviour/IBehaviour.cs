using CardWars.BattleEngine.Event;

namespace CardWars.BattleEngine.Behaviour;

// A behaviour (Similar to IEventHandler) takes any event and processes it
// It isn't strongly typed as IEventHandler
public interface IBehaviour
{
	public Type? ListeningEventType { get; set; }
	public int Priority { get; set; }
	public abstract void OnEvent(IEvent evnt);
}

// Needs a new naming...
// This is for any data-driven behaviour
public class DataDrivenBehaviour : IBehaviour
{
	public Type? ListeningEventType { get; set; }
	public int Priority { get; set; } = 0;

	public void OnEvent(IEvent evnt)
	{

	}
}
namespace CardWars.BattleEngine.Event;

public class EventService(BattleEngine engine)
{
	private readonly BattleEngine _engine = engine;

	public void Raise(IEvent evnt, EventResponse eventResponse)
	{
		foreach (var player in _engine.EntityService.Players)
		{
			// We do all the event handling here instead. Then the actual entity can just be data only, no logic there
		}
	}
}
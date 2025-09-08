using CardWars.BattleEngine.Core.States;

namespace CardWars.BattleEngine.Core.Events.EventContexts;

public class OnUnitSummonedEventContext : EventContext
{
	public UnitId SummonedUnitId;
	public PlayerId SummonerPlayerId;
};
using CardWars.BattleEngine.Core.States;

namespace CardWars.BattleEngine.Core.EventSystem.EventContexts;

public class OnUnitSummonedEventContext : EventContext
{
	public UnitId SummonedUnitId;
	public PlayerId SummonerPlayerId;
};
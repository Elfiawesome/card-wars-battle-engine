namespace CardWars.BattleEngine.Core.States.EventSystem.EventContexts;

public class OnUnitSummonedEventContext : EventContext
{
	public UnitId SummonedUnitId;
	public PlayerId SummonerPlayerId;
};
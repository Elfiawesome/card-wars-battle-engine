namespace CardWars.BattleEngine.Core.States;

public class AbilityState(GameState gameState, AbilityId id) : BaseState<AbilityId>(gameState, id)
{

}
public readonly record struct AbilityId(long Value) : IBaseId;
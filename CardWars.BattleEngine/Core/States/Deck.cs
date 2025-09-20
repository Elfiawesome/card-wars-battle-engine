namespace CardWars.BattleEngine.Core.States;

public class DeckState(GameState gameState, DeckId id) : BaseState<DeckId>(gameState, id)
{
	public List<string> Cards = [];
}
public readonly record struct DeckId(long Value) : IBaseId;

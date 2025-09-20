namespace CardWars.BattleEngine.Core.States;

public class PlayerState(GameState gameState, PlayerId id) : BaseState<PlayerId>(gameState, id)
{
	public string Name = "Default Player";
	public HashSet<DeckId> Decks = [];
	public List<UnitId> HandCards = [];
	public HashSet<BattlefieldId> ControllingBattlefields = [];
}
public readonly record struct PlayerId(long Value) : IBaseId;
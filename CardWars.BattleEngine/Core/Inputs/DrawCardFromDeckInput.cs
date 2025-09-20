using CardWars.BattleEngine.Core.States;

namespace CardWars.BattleEngine.Core.Inputs;

public record struct DrawCardFromDeckInputData(
	DeckId DeckId
) : IInputData;

using CardWars.BattleEngine.Core.States;

namespace CardWars.BattleEngine.Core.Actions.ActionHandlers;

public class IntstantiateDeckHandler : ActionHandler<IntstantiateDeckData>
{
	public override bool Handle(GameState gameState, IntstantiateDeckData actionData)
	{
		if (gameState.Decks.ContainsKey(actionData.DeckId)) { return false; }
		gameState.Decks[actionData.DeckId] = new(gameState, actionData.DeckId)
		{
			Cards = actionData.Cards
		};
		return true;
	}
}

public record struct IntstantiateDeckData(
	DeckId DeckId,
	List<string> Cards
) : IActionData;

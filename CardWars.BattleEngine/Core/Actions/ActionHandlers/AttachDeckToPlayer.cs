using CardWars.BattleEngine.Core.States;

namespace CardWars.BattleEngine.Core.Actions.ActionHandlers;

public class AttachDeckToPlayerHandler : ActionHandler<AttachDeckToPlayerData>
{
	public override bool Handle(GameState gameState, AttachDeckToPlayerData actionData)
	{
		if (gameState.Players.TryGetValue(actionData.PlayerId, out var player))
		{
			if (gameState.Decks.TryGetValue(actionData.DeckId, out var deck))
			{
				player.Decks.Add(deck.Id);
				return true;
			}
		}
		return false;
	}
}

public record struct AttachDeckToPlayerData(
	DeckId DeckId,
	PlayerId PlayerId
) : IActionData;

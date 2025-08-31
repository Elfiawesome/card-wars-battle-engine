using CardWars.BattleEngine.Core.States;

namespace CardWars.BattleEngine.Core.Actions.ActionHandlers;

public class AttachBattlefieldToPlayerHandler : ActionHandler<AttachBattlefieldToPlayerData>
{
	public override bool Handle(GameState gameState, AttachBattlefieldToPlayerData actionData)
	{
		if (gameState.Players.TryGetValue(actionData.PlayerId, out var player))
		{
			if (gameState.Battlefields.TryGetValue(actionData.BattlefieldId, out var battlefield))
			{
				player.ControllingBattlefields.Add(battlefield.Id);
				return true;
			}
		}
		return false;
	}
}

public record struct AttachBattlefieldToPlayerData(
	BattlefieldId BattlefieldId,
	PlayerId PlayerId
) : IActionData;

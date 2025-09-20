using CardWars.BattleEngine.Core.States;

namespace CardWars.BattleEngine.Core.Actions;

public record struct ActionContainer()
{
	required public IActionData Action;
	public List<PlayerId> Players = [];
}
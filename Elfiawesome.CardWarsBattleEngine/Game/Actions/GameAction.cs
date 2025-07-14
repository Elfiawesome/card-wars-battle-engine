namespace Elfiawesome.CardWarsBattleEngine.Game.Actions;

public abstract class GameAction
{
	public bool IsValid { get; private set; } = true;
	public abstract void Execute(CardWarsBattleEngine engine);
	public void InvalidateAction()
	{
		IsValid = false;
	}
}
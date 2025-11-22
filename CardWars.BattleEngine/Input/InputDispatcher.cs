using CardWars.Core.Common.Dispatching;

namespace CardWars.BattleEngine.Input;

public class InputDispatcher : RequestDispatcher<InputHandlerContext, IInput>
{
	public override void Register()
	{
		RegisterHandler(new EndTurnInputHandler());
		RegisterHandler(new DrawCardInputHandler());
	}
}
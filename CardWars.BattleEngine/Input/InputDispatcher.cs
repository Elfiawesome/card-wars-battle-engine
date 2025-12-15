using CardWars.BattleEngine.Input.Player;
using CardWars.BattleEngine.Input.Turn;
using CardWars.Core.Common.Dispatching;

namespace CardWars.BattleEngine.Input;

public class InputDispatcher : RequestDispatcher<InputHandlerContext, IInput>
{
	public override void Register()
	{
		RegisterHandler(new DrawCardInputHandler());
		RegisterHandler(new EndTurnInputHandler());
	}
}
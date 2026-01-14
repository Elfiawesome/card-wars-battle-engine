using CardWars.BattleEngine.Feature.DrawCard;
using CardWars.BattleEngine.Feature.EndTurn;
using CardWars.BattleEngine.Feature.IntendPlayCard;
using CardWars.Core.Common.Dispatching;

namespace CardWars.BattleEngine.Input;

public class InputDispatcher : RequestDispatcher<InputHandlerContext, IInput>
{
	public override void Register()
	{
		RegisterHandler(new DrawCardInputHandler());
		RegisterHandler(new IntendPlayCardInputHandler());
		RegisterHandler(new EndTurnInputHandler());
	}
}
using CardWars.Core.Common.Dispatching;

namespace CardWars.BattleEngine.Input;

public class InputDispatcher : RequestDispatcher<object, IInput, object>
{
	public override void Register()
	{
	}
}
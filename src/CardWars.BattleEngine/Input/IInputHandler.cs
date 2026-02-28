using CardWars.BattleEngine.Core.Request;

namespace CardWars.BattleEngine.Input;

public interface IInputHandler<TInput> : IRequestHandler<InputContext, TInput>
	where TInput : IInput;
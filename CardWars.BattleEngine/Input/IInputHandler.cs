using CardWars.BattleEngine.Core.Request;

namespace CardWars.BattleEngine.Input;

public interface IInputHandler<TInput> : IRequestHandler<Transaction, TInput>
	where TInput : IInput;
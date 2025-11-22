using CardWars.BattleEngine.Entity;
using CardWars.Core.Common.Dispatching;

namespace CardWars.BattleEngine.Input;

public interface IInputHandler<TInput> : IRequestHandler<InputHandlerContext, TInput>
	where TInput : IInput;

public record struct InputHandlerContext(BattleEngine Engine, PlayerId PlayerId);
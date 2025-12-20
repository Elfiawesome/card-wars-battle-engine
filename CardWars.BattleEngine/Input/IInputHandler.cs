using CardWars.BattleEngine.State.Entity;
using CardWars.Core.Common.Dispatching;

namespace CardWars.BattleEngine.Input;

public interface IInputHandler<TInput> : IRequestHandler<InputHandlerContext, TInput>
	where TInput : IInput;

public record struct InputHandlerContext(IServiceContainer ServiceContainer, PlayerId PlayerId);
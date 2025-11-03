using CardWars.BattleEngine.Entity;
using CardWars.Core.Common.Dispatching;

namespace CardWars.BattleEngine.Input;

public interface IInputHandler : IRequestHandler<InputHandlerContext, EndTurnInput>;

public record struct InputHandlerContext(BattleEngine Engine, PlayerId PlayerId);
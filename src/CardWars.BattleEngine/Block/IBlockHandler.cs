using CardWars.BattleEngine.State;
using CardWars.Core.Request;

namespace CardWars.BattleEngine.Block;

public interface IBlockHandler<TInput> : IRequestHandler<GameState, TInput>
	where TInput : IBlock;
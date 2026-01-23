using CardWars.BattleEngine.Core.Request;
using CardWars.BattleEngine.State;

namespace CardWars.BattleEngine.Block;

public interface IBlockHandler<TInput> : IRequestHandler<GameState, TInput>
	where TInput : IBlock;
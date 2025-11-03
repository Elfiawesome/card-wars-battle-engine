using CardWars.BattleEngine.Block;
using CardWars.Core.Common.Dispatching;

namespace CardWars.BattleEngine.Input;

public interface IBlockHandler<T> : IRequestHandler<BattleEngine, T, bool> where T : IBlock;

using CardWars.Core.Common.Dispatching;

namespace CardWars.BattleEngine.Block;

public interface IBlockHandler<T> : IRequestHandler<IServiceContainer, T, bool> where T : IBlock;

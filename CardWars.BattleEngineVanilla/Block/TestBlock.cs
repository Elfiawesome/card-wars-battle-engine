using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;

namespace CardWars.BattleEngineVanilla.Block;

public record struct TestBlock() : IBlock;

public class TestBlockHandler() : IBlockHandler<TestBlock>
{
	public void Handle(GameState context, TestBlock request)
	{
		
	}
}
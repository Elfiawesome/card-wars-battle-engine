using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Features;
using CardWars.Tests.Support;
using Xunit;
using CardWars.BattleEngine.Vanilla.State;

namespace CardWars.Tests.Vanilla;

public sealed class BattleLeaveTests
{
	[Fact]
	public void LeavingPlayer_IsDefeated_AndBattleEnds()
	{
		var battle = BattleTestHarness.Create();

		battle.Engine.HandleInput(EntityId.None, new PlayerLeftRequestInput(battle.Player1Id));

		Assert.True(battle.Player1.IsDefeated);
		Assert.False(battle.Player2.IsDefeated);
		Assert.True(battle.State.Ext.GetOrCreateExtension<CardWarsBattleState>().Outcome.IsGameOver);
	}
}

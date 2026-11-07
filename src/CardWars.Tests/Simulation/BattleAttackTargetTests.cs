using CardWars.BattleEngine;
using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.BattleEngine.Vanilla.Features;
using CardWars.Tests.Support;
using Xunit;

namespace CardWars.Tests.Simulation;

public sealed class BattleAttackTargetTests
{
	[Fact]
	public void TargetUnitEvent_Modification_ReachesTheActualAttack()
	{
		var battle = BattleTestHarness.Create();
		var attacker = battle.DrawAndPlay(battle.Player1Id, new UnitSlotPos(0, 0));

		var first = battle.DrawAndPlay(battle.Player2Id, new UnitSlotPos(-1, 0));
		var firstHp = first.Hp;
		var second = battle.DrawAndPlay(battle.Player2Id, new UnitSlotPos(0, 0));
		var secondHp = second.Hp;
		var secondSlot = battle.SlotOf(battle.Player2Id, new UnitSlotPos(0, 0));

		battle.Engine.Registry.EventHandlers.Register(new RedirectTargetUnitEventHandler(first.Id));
		battle.AdvanceToAttackingPhase();

		battle.Attack(battle.Player1Id, battle.SlotOf(battle.Player1Id, new UnitSlotPos(0, 0)), secondSlot);

		Assert.Equal(firstHp - attacker.Atk, first.Hp);
		Assert.Equal(secondHp, second.Hp);
	}

	private sealed class RedirectTargetUnitEventHandler(EntityId targetId) : IEventHandler<TargetUnitEvent>
	{
		public void Handle(Transaction context, TargetUnitEvent request)
		{
			request.TargetId = targetId;
			new TargetUnitEventHandler().Handle(context, request);
		}
	}
}

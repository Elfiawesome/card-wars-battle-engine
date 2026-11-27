using System;
using CardWars.BattleEngine;
using CardWars.Core.Registry;
using CardWars.Server.Vanilla.Session;
using CardWars.Tests.Support;
using Xunit;

namespace CardWars.Tests.Vanilla;

public sealed class BattlePersistenceTests
{
	[Fact]
	public void Snapshot_RoundTripsThroughBattleInstance()
	{
		var battle = BattleTestHarness.Create();
		var instance = new BattleInstance
		{
			InstanceId = Guid.NewGuid(),
			InstanceProviderId = ResourceId.Vanilla("battles/test"),
			Snapshot = battle.State.ToSnapshot()
		};

		var tag = TestBootstrap.DataTags.ToTag(instance, includeType: false);
		var restored = TestBootstrap.DataTags.FromTag<BattleInstance>(tag);

		Assert.NotNull(restored.Snapshot);
		Assert.Equal(instance.Snapshot.Entities.Count, restored.Snapshot.Entities.Count);

		restored.InitializeEngine(new BattleEngine.BattleEngine(new BattleEngineRegistry(
			TestBootstrap.DataTags,
			TestBootstrap.Logger)));

		var engine = Assert.IsType<BattleEngine.BattleEngine>(restored.Engine);
		Assert.Equal(instance.Snapshot.Entities.Count, engine.State.Entities.Count);
	}
}

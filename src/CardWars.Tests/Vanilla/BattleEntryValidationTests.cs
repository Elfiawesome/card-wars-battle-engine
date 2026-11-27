using CardWars.Core.Network.Packet;
using CardWars.Core.Network.Transport;
using CardWars.Server.Session;
using CardWars.Server.Vanilla.Session;
using CardWars.Tests.Support;
using CardWars.Vanilla.Shared;
using Xunit;

namespace CardWars.Tests.Vanilla;

public sealed class BattleEntryValidationTests
{
	[Fact]
	public void RejectsMissingAndEmptyProfiles()
	{
		var instance = new BattleInstance();
		var player = CreatePlayer();

		Assert.False(instance.CanAddPlayer(player, out var missingProfileReason));
		Assert.Contains("profile", missingProfileReason);

		SetProfile(player, new PlayerProfile
		{
			Heroes = [TestCards.CaptainHero]
		});
		Assert.False(instance.CanAddPlayer(player, out var missingDeckReason));
		Assert.Contains("deck", missingDeckReason);

		SetProfile(player, new PlayerProfile
		{
			CurrentDeck = [TestCards.CobCannon]
		});
		Assert.False(instance.CanAddPlayer(player, out var missingHeroesReason));
		Assert.Contains("heroes", missingHeroesReason);

		SetProfile(player, new PlayerProfile
		{
			CurrentDeck = [TestCards.CobCannon],
			Heroes = [TestCards.CaptainHero]
		});
		Assert.True(instance.CanAddPlayer(player, out _));
	}

	private static PlayerSession CreatePlayer()
		=> new()
		{
			PlayerId = Guid.NewGuid(),
			Connection = new StubConnection()
		};

	private static void SetProfile(PlayerSession player, PlayerProfile profile)
		=> player.GetSetComponent<ProfilePlayerDataComponent>(SharedIds.PlayerData.Profile).Profile = profile;

	private sealed class StubConnection : IConnection
	{
		public bool IsConnected => true;
		public void Send(IPacket packet) { }
		public bool TryReceive(out IPacket? packet) { packet = null; return false; }
		public void Disconnect() { }
		public void Dispose() { }
	}
}

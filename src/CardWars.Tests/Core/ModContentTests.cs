using CardWars.Core.Storage;
using CardWars.Tests.Support;
using Xunit;

namespace CardWars.Tests.Core;

public sealed class ModContentTests
{
	[Fact]
	public void GetContent_ReturnsDataAndAssetsForRequestedRoots()
	{
		var provider = new MemoryFileProvider();
		provider.AddFile("mods/test/mod.json", """{"id":"test"}""");
		provider.AddFile("mods/test/content/server/worlds/city.json", "{}");
		provider.AddFile("mods/test/content/client/config.json", "{}");
		provider.AddFile("mods/test/content/battle/cards/units/hero.json", "{}");
		provider.AddFile("mods/test/content/battle/assets/texture/cards/hero.png", "image");

		var loader = new CardWars.ModLoader.ModRepository([new StoragePath("mods/test", provider)], TestBootstrap.DataTags, TestBootstrap.Logger);
		loader.Setup();

		var server = loader.GetContent("server");
		Assert.Equal("test:worlds/city", Assert.Single(server.Data).Id.ToString());

		var client = loader.GetContent("client");
		Assert.Equal("test:config", Assert.Single(client.Data).Id.ToString());

		var battle = loader.GetContent("battle", "assets");
		var battleData = Assert.Single(battle.Data);
		Assert.Equal("test:cards/units/hero", battleData.Id.ToString());
		Assert.Equal(["cards", "units"], battleData.Category);

		var battleAsset = Assert.Single(battle.Assets);
		Assert.Equal("test:texture/cards/hero", battleAsset.Id.ToString());
	}
}

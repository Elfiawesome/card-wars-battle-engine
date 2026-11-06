using CardWars.BattleEngine;
using CardWars.BattleEngine.Vanilla;
using CardWars.BattleEngine.Vanilla.Rules;
using CardWars.Core.Registry;
using CardWars.Core.Storage;
using CardWars.ModLoader;
using CardWars.Tests.Support;
using Xunit;

namespace CardWars.Tests.Vanilla;

public sealed class HeroLoseConditionTests
{
	[Fact]
	public void HeroRegistration_InjectsDefaultLoseConditionPointer()
	{
		var provider = new MemoryFileProvider();
		provider.AddFile("cards/heroes/test_hero.json", """
			{
				"name": "Test Hero",
				"hrt": 5
			}
			""");

		var registry = new BattleEngineRegistry();
		var content = new ModContentResult
		{
			Id = ResourceId.Vanilla("cards/heroes/test_hero"),
			FilePath = new StoragePath("cards/heroes/test_hero.json", provider),
			Category = ["cards", "heroes"]
		};

		new VanillaMod().OnLoad(registry, [content]);

		var definition = registry.CardDefinitions.Get(content.Id)
			?? throw new InvalidOperationException("Hero definition was not registered");
		var pointer = definition.GetCompound("lose_condition")
			?? throw new InvalidOperationException("Default lose condition was not injected");

		Assert.Equal(HeroHrtLoseCondition.Id.ToString(), pointer.GetString("resource"));
	}
}

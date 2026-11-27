using CardWars.Tests.Support;
using CardWars.BattleEngine.Behaviour.DataDriven;
using CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Instructions;

using CardWars.Core.Data.Mapping;
using CardWars.Core.Data.Serialization.Json;
using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;
using Xunit;

namespace CardWars.Tests.Core;

public sealed class DefinitionSerializationTests
{
	[Fact]
	public void BehaviourDefinition_RoundTripsAllInstructionAndExpressionKinds()
	{
		var definition = new BehaviourDefinition(
			[
				new EventHook(
					ResourceId.Vanilla("test/event"),
					[
						new DeclarationInstruction(
							ResourceId.Vanilla("value"),
							new AddExpression(
								new LiteralExpression(new IntTag(2)),
								new MultiplyExpression(
									new LiteralExpression(new IntTag(3)),
									new LiteralExpression(new IntTag(4)))),
							IsGlobal: true),
						new IfInstruction(
							new LiteralExpression(new BoolTag(true)),
							[new LogInstruction(new VariableExpression(ResourceId.Vanilla("value")))],
							[]),
						new ForEachInstruction(
							ResourceId.Vanilla("item"),
							new ListExpression([new LiteralExpression(new IntTag(1))]),
							[new SetInstruction(ResourceId.Vanilla("value"), new VariableExpression(ResourceId.Vanilla("item")))])
					])
			],
			[
				new InputHook(ResourceId.Vanilla("test/input"), [new WaitInstruction()])
			])
		{
			Priority = 7
		};

		var tag = TestBootstrap.DataTags.ToTag(definition);
		var json = TestBootstrap.DataTags.Serialize(tag);
		var restoredTag = TestBootstrap.DataTags.Deserialize<CompoundTag>(json) ?? new CompoundTag();
		var restored = TestBootstrap.DataTags.FromTag<BehaviourDefinition>(restoredTag);

		Assert.Equal(7, restored.Priority);
		var eventHook = Assert.Single(restored.OnEvent ?? []);
		Assert.Equal(ResourceId.Vanilla("test/event"), eventHook.EventType);

		var instructions = eventHook.Instructions ?? [];
		Assert.Collection(
			instructions,
			item =>
			{
				var declaration = Assert.IsType<DeclarationInstruction>(item);
				Assert.Equal(ResourceId.Vanilla("value"), declaration.Name);
				Assert.True(declaration.IsGlobal);
				Assert.IsType<AddExpression>(declaration.Value);
			},
			item => Assert.IsType<IfInstruction>(item),
			item => Assert.IsType<ForEachInstruction>(item));

		var inputHook = Assert.Single(restored.OnInput ?? []);
		Assert.Equal(ResourceId.Vanilla("test/input"), inputHook.InputType);
		Assert.IsType<WaitInstruction>(Assert.Single(inputHook.Instructions ?? []));
	}

	[Fact]
	public void ConcreteScriptTypes_AreRegistered()
	{
		Assert.Equal(typeof(IfInstruction), TestBootstrap.DataTags.GetType("cardwars:script/instruction/if"));
		Assert.Equal(typeof(AddExpression), TestBootstrap.DataTags.GetType("cardwars:script/expression/add"));
		Assert.Equal(typeof(BehaviourDefinition), TestBootstrap.DataTags.GetType("cardwars:script/behaviour"));
	}
}

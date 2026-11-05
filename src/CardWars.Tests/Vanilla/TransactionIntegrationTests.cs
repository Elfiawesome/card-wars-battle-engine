using CardWars.BattleEngine.Behaviour.DataDriven;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla;
using CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Instructions;
using CardWars.Core.Data.Mapping;
using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;
using CardWars.Tests.Support;
using Xunit;

namespace CardWars.Tests.Vanilla;

public sealed class TransactionIntegrationTests
{
	[Fact]
	public void BattleEngine_InstantiatesDataDrivenBehaviourFromPointer()
	{
		var engine = new BattleEngine.BattleEngine();
		new VanillaMod().OnLoad(engine.Registry, []);
		engine.Registry.InputHandlers.Register(new TestEventInputHandler());
		engine.Registry.EventHandlers.Register(new TestEventHandler());

		var definition = new BehaviourDefinition(
			[
				new EventHook(
					ResourceId.Vanilla("test/event"),
					[
						new DeclarationInstruction(
							ResourceId.Vanilla("event_value"),
							new MemberExpression(new VariableExpression(ScriptBindings.Event), "value"),
							IsGlobal: true)
					])
			]);

		var pointer = new BehaviourPointer(BehaviourDefinition: DataTagMapper.ToTag(definition));
		var entity = new TestEntity
		{
			Id = EntityId.New(),
			Hp = 10,
			Behaviours = [pointer]
		};
		engine.State.Add(entity);

		engine.HandleInput(EntityId.None, new TestEventInput());

		Assert.True(engine.State.GlobalScope.Variables.TryGetValue(ResourceId.Vanilla("event_value"), out var value));
		Assert.Equal(9, Assert.IsType<IntTag>(value).Value);
	}
}

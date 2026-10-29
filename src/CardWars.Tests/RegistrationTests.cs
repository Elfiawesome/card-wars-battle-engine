using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Behaviour.DataDriven.Instructions;
using CardWars.BattleEngine.Vanilla;
using Xunit;

namespace CardWars.Tests;

public sealed class RegistrationTests
{
	[Fact]
	public void EveryConcreteExpression_HasARegisteredHandler()
	{
		var registry = TestSupport.CreateRegistry();
		var expressionTypes = typeof(VanillaMod).Assembly.GetTypes()
			.Where(type => !type.IsAbstract && !type.IsInterface && typeof(IExpression).IsAssignableFrom(type));

		foreach (var type in expressionTypes)
		{
			Assert.True(
				registry.DataDriven.ExpressionHandlers.Get(type) != null,
				$"Missing expression handler for [{type.Name}]");
		}
	}

	[Fact]
	public void EveryConcreteInstruction_HasARegisteredHandler()
	{
		var registry = TestSupport.CreateRegistry();
		var instructionTypes = typeof(VanillaMod).Assembly.GetTypes()
			.Where(type => !type.IsAbstract && !type.IsInterface && typeof(IInstruction).IsAssignableFrom(type));

		foreach (var type in instructionTypes)
		{
			Assert.True(
				registry.DataDriven.InstructionHandlers.Get(type) != null,
				$"Missing instruction handler for [{type.Name}]");
		}
	}
}

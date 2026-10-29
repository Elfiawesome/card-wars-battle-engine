using CardWars.BattleEngine;
using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Behaviour.DataDriven.Functions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Expressions;
using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;
using Xunit;

namespace CardWars.Tests;

public sealed class ExpressionOperatorTests
{
	private static ScriptRuntimeContext CreateRuntime(out BattleEngineRegistry registry)
	{
		registry = TestSupport.CreateRegistry();
		return TestSupport.CreateRuntime(registry, new GameState(), EntityId.None);
	}

	[Fact]
	public void Arithmetic_PreservesNumericTypesAndConcatenatesStrings()
	{
		var runtime = CreateRuntime(out _);

		Assert.Equal(5, Assert.IsType<IntTag>(runtime.Evaluate(Add(2, 3))).Value);
		Assert.Equal(5f, Assert.IsType<FloatTag>(runtime.Evaluate(Add(2f, 3))).Value);
		Assert.Equal("x1", Assert.IsType<StringTag>(runtime.Evaluate(new AddExpression(
			new LiteralExpression(new StringTag("x")),
			new LiteralExpression(new IntTag(1))))).Value);

		Assert.Equal(1, Assert.IsType<IntTag>(runtime.Evaluate(new SubtractExpression(
			new LiteralExpression(new IntTag(4)),
			new LiteralExpression(new IntTag(3))))).Value);
		Assert.Equal(12, Assert.IsType<IntTag>(runtime.Evaluate(new MultiplyExpression(
			new LiteralExpression(new IntTag(3)),
			new LiteralExpression(new IntTag(4))))).Value);
		Assert.Equal(2, Assert.IsType<IntTag>(runtime.Evaluate(new DivideExpression(
			new LiteralExpression(new IntTag(8)),
			new LiteralExpression(new IntTag(4))))).Value);
		Assert.Equal(1, Assert.IsType<IntTag>(runtime.Evaluate(new ModuloExpression(
			new LiteralExpression(new IntTag(7)),
			new LiteralExpression(new IntTag(3))))).Value);
	}

	[Fact]
	public void Arithmetic_InvalidOperationsReturnNull()
	{
		var runtime = CreateRuntime(out _);

		Assert.IsType<NullTag>(runtime.Evaluate(new DivideExpression(
			new LiteralExpression(new IntTag(1)),
			new LiteralExpression(new IntTag(0)))));
		Assert.IsType<NullTag>(runtime.Evaluate(new ModuloExpression(
			new LiteralExpression(new IntTag(1)),
			new LiteralExpression(new IntTag(0)))));
		Assert.IsType<NullTag>(runtime.Evaluate(new SubtractExpression(
			new LiteralExpression(new StringTag("x")),
			new LiteralExpression(new IntTag(1)))));
	}

	[Fact]
	public void Comparisons_CompareNumbersStringsAndTruthiness()
	{
		var runtime = CreateRuntime(out _);

		Assert.True(Assert.IsType<BoolTag>(runtime.Evaluate(new EqualsExpression(
			new LiteralExpression(new IntTag(2)),
			new LiteralExpression(new FloatTag(2f))))).Value);
		Assert.True(Assert.IsType<BoolTag>(runtime.Evaluate(new NotEqualsExpression(
			new LiteralExpression(new StringTag("a")),
			new LiteralExpression(new StringTag("b"))))).Value);
		Assert.True(Assert.IsType<BoolTag>(runtime.Evaluate(new LessThanExpression(
			new LiteralExpression(new IntTag(1)),
			new LiteralExpression(new IntTag(2))))).Value);
		Assert.True(Assert.IsType<BoolTag>(runtime.Evaluate(new LessThanOrEqualExpression(
			new LiteralExpression(new IntTag(2)),
			new LiteralExpression(new IntTag(2))))).Value);
		Assert.True(Assert.IsType<BoolTag>(runtime.Evaluate(new GreaterThanExpression(
			new LiteralExpression(new IntTag(3)),
			new LiteralExpression(new IntTag(2))))).Value);
		Assert.True(Assert.IsType<BoolTag>(runtime.Evaluate(new GreaterThanOrEqualExpression(
			new LiteralExpression(new IntTag(2)),
			new LiteralExpression(new IntTag(2))))).Value);
	}

	[Fact]
	public void LogicalAndConditional_UseDataTagTruthiness()
	{
		var runtime = CreateRuntime(out _);

		Assert.False(Assert.IsType<BoolTag>(runtime.Evaluate(new AndExpression(
			Lit(new IntTag(0)),
			Lit(new IntTag(1))))).Value);
		Assert.True(Assert.IsType<BoolTag>(runtime.Evaluate(new OrExpression(
			Lit(new StringTag("")),
			Lit(new StringTag("x"))))).Value);
		Assert.True(Assert.IsType<BoolTag>(runtime.Evaluate(new NotExpression(
			Lit(new BoolTag(false))))).Value);

		Assert.Equal(1, Assert.IsType<IntTag>(runtime.Evaluate(new ConditionalExpression(
			Lit(new BoolTag(true)),
			Lit(new IntTag(1)),
			Lit(new IntTag(2))))).Value);
		Assert.Equal(2, Assert.IsType<IntTag>(runtime.Evaluate(new ConditionalExpression(
			Lit(new BoolTag(false)),
			Lit(new IntTag(1)),
			Lit(new IntTag(2))))).Value);

		Assert.Equal(3, Assert.IsType<IntTag>(runtime.Evaluate(new CoalesceExpression(
			Lit(new NullTag()),
			Lit(new IntTag(3))))).Value);
		Assert.Equal(4, Assert.IsType<IntTag>(runtime.Evaluate(new CoalesceExpression(
			Lit(new IntTag(4)),
			Lit(new IntTag(3))))).Value);
	}

	[Fact]
	public void Negate_HandlesNumbersAndRejectsOtherTypes()
	{
		var runtime = CreateRuntime(out _);

		Assert.Equal(-2, Assert.IsType<IntTag>(runtime.Evaluate(new NegateExpression(
			Lit(new IntTag(2))))).Value);
		Assert.Equal(-2f, Assert.IsType<FloatTag>(runtime.Evaluate(new NegateExpression(
			Lit(new FloatTag(2f))))).Value);
		Assert.IsType<NullTag>(runtime.Evaluate(new NegateExpression(
			Lit(new StringTag("x")))));
	}

	[Fact]
	public void CallExpression_InvokesRegisteredFunction()
	{
		var runtime = CreateRuntime(out var registry);
		registry.DataDriven.Functions.Register(ResourceId.Vanilla("test/sum"), new SumFunction());

		var result = runtime.Evaluate(new CallExpression(
			ResourceId.Vanilla("test/sum"),
			new Dictionary<string, IExpression>
			{
				["left"] = Lit(new IntTag(2)),
				["right"] = Lit(new IntTag(3))
			}));

		Assert.Equal(5, Assert.IsType<IntTag>(result).Value);
	}

	private static AddExpression Add(int left, int right)
		=> new(Lit(new IntTag(left)), Lit(new IntTag(right)));

	private static AddExpression Add(float left, int right)
		=> new(Lit(new FloatTag(left)), Lit(new IntTag(right)));

	private static LiteralExpression Lit(DataTag tag) => new(tag);

	private sealed class SumFunction : IDataDrivenFunction
	{
		public DataTag Invoke(ScriptRuntimeContext context, IReadOnlyList<DataTag> arguments)
			=> new IntTag(arguments.OfType<IntTag>().Sum(tag => tag.Value));
	}
}

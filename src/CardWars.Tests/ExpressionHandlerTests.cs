using CardWars.BattleEngine.Behaviour.DataDriven;
using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Expressions;
using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;
using Xunit;

namespace CardWars.Tests;

public sealed class ExpressionHandlerTests
{
	[Fact]
	public void LiteralAndVariableExpressions_ReturnValues()
	{
		var registry = TestSupport.CreateRegistry();
		var state = new GameState();
		var runtime = TestSupport.CreateRuntime(registry, state, EntityId.None);

		var literal = runtime.Evaluate(new LiteralExpression(new IntTag(5)));
		Assert.Equal(5, Assert.IsType<IntTag>(literal).Value);

		runtime.DeclareVariable(ResourceId.Vanilla("x"), new StringTag("hello"));
		var variable = runtime.Evaluate(new VariableExpression(ResourceId.Vanilla("x")));
		Assert.Equal("hello", Assert.IsType<StringTag>(variable).Value);
	}

	[Fact]
	public void MemberAndIndexExpressions_ReadTagsAndObjects()
	{
		var registry = TestSupport.CreateRegistry();
		var state = new GameState();
		var entity = TestSupport.AddEntity(state, hp: 17);
		entity.Data.Set("mp", 4);
		state.Turn = new TurnState([], [], 0, 3, TurnPhase.Attacking);

		var runtime = TestSupport.CreateRuntime(registry, state, entity.Id);
		runtime.DeclareVariable(ScriptBindings.Owner, new ObjectRefTag(entity));
		runtime.DeclareVariable(ScriptBindings.Self, new ObjectRefTag(entity));
		runtime.DeclareVariable(ScriptBindings.State, new ObjectRefTag(state));
		runtime.DeclareVariable(ScriptBindings.Vars, new ObjectRefTag(runtime));
		runtime.DeclareVariable(ResourceId.Vanilla("custom"), new IntTag(8));

		var hp = runtime.Evaluate(new MemberExpression(new VariableExpression(ScriptBindings.Owner), "hp"));
		Assert.Equal(17, Assert.IsType<IntTag>(hp).Value);

		var mp = runtime.Evaluate(new MemberExpression(
			new MemberExpression(new VariableExpression(ScriptBindings.Self), "data"),
			"mp"));
		Assert.Equal(4, Assert.IsType<IntTag>(mp).Value);

		var turnNumber = runtime.Evaluate(new MemberExpression(
			new MemberExpression(new VariableExpression(ScriptBindings.State), "turn"),
			"turn_number"));
		Assert.Equal(3, Assert.IsType<IntTag>(turnNumber).Value);

		var entityById = runtime.Evaluate(new IndexExpression(
			new VariableExpression(ScriptBindings.State),
			new LiteralExpression(new GuidTag(entity.Id.Value))));
		var entityHp = runtime.Evaluate(new MemberExpression(new LiteralExpression(entityById), "hp"));
		Assert.Equal(17, Assert.IsType<IntTag>(entityHp).Value);

		var varsValue = runtime.Evaluate(new MemberExpression(new VariableExpression(ScriptBindings.Vars), "custom"));
		Assert.Equal(8, Assert.IsType<IntTag>(varsValue).Value);
	}

	[Fact]
	public void ListAndObjectExpressions_BuildContainers()
	{
		var registry = TestSupport.CreateRegistry();
		var state = new GameState();
		var runtime = TestSupport.CreateRuntime(registry, state, EntityId.None);

		var list = runtime.Evaluate(new ListExpression(
			[new LiteralExpression(new IntTag(1)), new LiteralExpression(new IntTag(2))]));
		var listTag = Assert.IsType<ListTag>(list);
		Assert.Equal(2, listTag.Count);
		Assert.Equal(2, Assert.IsType<IntTag>(listTag.Get(1)).Value);

		var obj = runtime.Evaluate(new ObjectExpression(new Dictionary<string, IExpression>
		{
			["name"] = new LiteralExpression(new StringTag("unit")),
			["hp"] = new LiteralExpression(new IntTag(6))
		}));
		var compound = Assert.IsType<CompoundTag>(obj);
		Assert.Equal("unit", compound.GetString("name"));
		Assert.Equal(6, compound.GetInt("hp"));
	}

	[Fact]
	public void ConstructExpression_CreatesRegisteredTypes()
	{
		var registry = TestSupport.CreateRegistry();
		var state = new GameState();
		var runtime = TestSupport.CreateRuntime(registry, state, EntityId.None);

		var constructed = runtime.Evaluate(new ConstructExpression(
			ResourceId.Vanilla("test/block"),
			new Dictionary<string, IExpression> { ["value"] = new LiteralExpression(new IntTag(7)) }));

		var block = runtime.ResolveObject<TestBlock>(constructed);
		Assert.NotNull(block);
		Assert.Equal(7, block.Value);
	}

	[Fact]
	public void UnknownVariableAndMember_ReturnNullTag()
	{
		var registry = TestSupport.CreateRegistry();
		var state = new GameState();
		var runtime = TestSupport.CreateRuntime(registry, state, EntityId.None);

		var unknownVariable = runtime.Evaluate(new VariableExpression(ResourceId.Vanilla("missing")));
		Assert.IsType<NullTag>(unknownVariable);

		var unknownMember = runtime.Evaluate(new MemberExpression(
			new LiteralExpression(new CompoundTag()),
			"missing"));
		Assert.IsType<NullTag>(unknownMember);
	}
}

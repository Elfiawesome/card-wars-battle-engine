using CardWars.BattleEngine.Behaviour.DataDriven;
using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Expressions;
using CardWars.Core.Data.Mapping;
using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;

namespace CardWars.BattleEngine.Vanilla.Targeting;

public static class UnitTargetFilter
{
	public static IExpression Create()
	{
		var candidate = new VariableExpression(TargetBindings.Candidate);
		var card = new VariableExpression(TargetBindings.Card);
		var state = new VariableExpression(ScriptBindings.State);

		// TODO: We need a new expression for entity checking since DataTagMapper.TypeDiscriminatorKey can change to anything and break this...
		var isUnitSlot = new EqualsExpression(
			new MemberExpression(candidate, DataTagMapper.TypeDiscriminatorKey),
			new LiteralExpression(new StringTag(ResourceId.Vanilla("battle/entity/unit_slot").ToString())));

		var isEmpty = new EqualsExpression(
			new MemberExpression(candidate, "holding_card_id"),
			new LiteralExpression(new NullTag()));

		var ownerPlayer = new IndexExpression(
			state,
			new MemberExpression(card, "owner_player_id"));
		var ownerBattlefield = new IndexExpression(
			new MemberExpression(ownerPlayer, "battlefield_ids"),
			new LiteralExpression(new IntTag(0)));
		var sameOwner = new EqualsExpression(
			new MemberExpression(candidate, "owner_battlefield_id"),
			ownerBattlefield);

		return new AndExpression(new AndExpression(isUnitSlot, isEmpty), sameOwner);
	}
}

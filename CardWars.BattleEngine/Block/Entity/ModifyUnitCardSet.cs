using CardWars.BattleEngine.Entity;
using CardWars.BattleEngine.Input;

namespace CardWars.BattleEngine.Block.Entity;

public record ModifyUnitCardSetBlock(
	UnitCardId UnitCardId,
	string? Name = null,
	string? FlavourText = null,
	int? PointCost = null,
	int? Hp = null,
	int? Atk = null
) : IBlock;

public class ModifyUnitCardSetBlockHandler : IBlockHandler<ModifyUnitCardSetBlock>
{
	public bool Handle(BattleEngine context, ModifyUnitCardSetBlock request)
	{
		if (!context.EntityService.UnitCards.TryGetValue(request.UnitCardId, out var unitCard)) { return false; }
		if (unitCard == null) { return false; }

		unitCard.Name = request.Name ?? unitCard.Name;
		unitCard.FlavourText = request.FlavourText ?? unitCard.FlavourText;
		unitCard.PointCost = request.PointCost ?? unitCard.PointCost;
		unitCard.Hp = request.Hp ?? unitCard.Hp;
		unitCard.Atk = request.Atk ?? unitCard.Atk;
		
		return true;
	}
}
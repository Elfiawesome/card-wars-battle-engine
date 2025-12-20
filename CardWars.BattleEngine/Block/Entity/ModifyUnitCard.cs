using CardWars.BattleEngine.State.Entity;

namespace CardWars.BattleEngine.Block.Entity;

public record struct ModifyUnitCardBlock(
	UnitCardId UnitCardId
) : IBlock;

public enum ModifyOperation
{
	NumberSet,
	NumberAdd,
	NumberMultiply


};

public record struct ModifyStatData(
	string StatName,
	string Value,
	ModifyOperation ModifyOperation
);

public class ModifyUnitCardBlockHandler : IBlockHandler<ModifyUnitCardBlock>
{
	public bool Handle(IServiceContainer service, ModifyUnitCardBlock request)
	{
		if (!service.State.UnitCards.TryGetValue(request.UnitCardId, out var deck)) { return false; }
		if (deck == null) { return false; }



		return true;
	}
}
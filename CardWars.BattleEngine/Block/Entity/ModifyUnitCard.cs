
using CardWars.BattleEngine.State;

namespace CardWars.BattleEngine.Block.Entity;

public record struct ModifyUnitCardBlock(
	UnitCardId UnitCardId,
	NumberModifyType HpModifyType = NumberModifyType.Set,
	int? Hp = null,
	
	NumberModifyType AtkModifyType = NumberModifyType.Set,
	int? Atk = null
) : IBlock;

public enum NumberModifyType
{
	Add,
	Set
}

public class ModifyUnitCardBlockHandler : IBlockHandler<ModifyUnitCardBlock>
{
	public bool Handle(IServiceContainer service, ModifyUnitCardBlock request)
	{
		if (!service.State.UnitCards.TryGetValue(request.UnitCardId, out var unitCard)) { return false; }
		if (unitCard == null) { return false; }

		switch (request.HpModifyType)
		{
			case NumberModifyType.Add:
				unitCard.Hp += request.Hp ?? 0;
				break;
			case NumberModifyType.Set:
				unitCard.Hp = request.Hp ?? 0;
				break;
		}

		return true;
	}
}
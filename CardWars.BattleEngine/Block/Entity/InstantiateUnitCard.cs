using CardWars.BattleEngine.State.Entity;

namespace CardWars.BattleEngine.Block.Entity;

public record struct InstantiateUnitCardBlock(
	UnitCardId UnitCardId
) : IBlock;

[BlockHandlerRegistry]
public class InstantiateUnitCardBlockHandler : IBlockHandler<InstantiateUnitCardBlock>
{
	public bool Handle(IServiceContainer service, InstantiateUnitCardBlock request)
	{
		if (service.State.UnitCards.ContainsKey(request.UnitCardId)) { return false; }
		service.State.UnitCards[request.UnitCardId] = new UnitCard(request.UnitCardId);
		return true;
	}
}

using CardWars.BattleEngine.State;

namespace CardWars.BattleEngine.Block.Entity;

public record struct ModifyUnitCardHpModifyBlock(
	UnitCardId UnitCardId,
	StatLayerId StatLayerId,
	StatLayer StatLayer,
	ModifyOp Op = ModifyOp.Add
) : IBlock;

public enum ModifyOp { None = 0, Add, Set }
public enum CompositeValueType { MaxValue, Value }

public class ModifyUnitCardHpModifyBlockHandler : IBlockHandler<ModifyUnitCardHpModifyBlock>
{
	public bool Handle(IServiceContainer service, ModifyUnitCardHpModifyBlock request)
	{
		if (!service.State.UnitCards.TryGetValue(request.UnitCardId, out var unitCard)) { return false; }
		if (unitCard == null) { return false; }

		var layer = unitCard.Hp.Layers.Find((s) => s.Id == request.StatLayerId);

		switch (request.Op)
		{
			case ModifyOp.None:
				break;
			case ModifyOp.Add:
				layer.Value += request.StatLayer.Value;
				layer.MaxValue += request.StatLayer.MaxValue;
				layer.Name += request.StatLayer.Name;
				break;
			case ModifyOp.Set:
				layer.Value = request.StatLayer.Value;
				layer.MaxValue = request.StatLayer.MaxValue;
				layer.Name = request.StatLayer.Name;
				break;
		}

		return true;
	}
}
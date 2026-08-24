using CardWars.Core.Data;
using CardWars.Server.Session;

namespace CardWars.Server.Vanilla.Session;

public class BattleInstanceProvider : ServerInstanceProvider<BattleInstance, Guid>
{
	protected override BattleInstance Create(Guid battleId)
		=> new BattleInstance { InstanceId = battleId };

	protected override BattleInstance Deserialize(CompoundTag data, Guid id)
		=> DataTagMapper.FromTag<BattleInstance>(data);

	protected override CompoundTag Serialize(BattleInstance instance)
		=> DataTagMapper.ToTag(instance, false);

	protected override Guid Parse(string id) => Guid.Parse(id);
}

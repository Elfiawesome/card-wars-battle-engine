using CardWars.Core.Data.Tags;

namespace CardWars.BattleEngine.Behaviour.DataDriven.Objects;

public interface IObjectAccessor
{
	bool TryGetMember(object target, string member, out DataTag? value);
	bool TrySetMember(object target, string member, DataTag value);
	bool TryGetIndex(object target, DataTag index, out DataTag? value);
	bool TrySetIndex(object target, DataTag index, DataTag value);
}

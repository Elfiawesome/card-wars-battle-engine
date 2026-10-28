using CardWars.BattleEngine.State;
using CardWars.Core.Data.Tags;

namespace CardWars.BattleEngine.Behaviour.DataDriven.Objects;

public sealed class GameStateObjectAccessor : IObjectAccessor
{
	public bool TryGetMember(object target, string member, out DataTag? value)
	{
		value = null;
		return false;
	}

	public bool TrySetMember(object target, string member, DataTag value) => false;

	public bool TryGetIndex(object target, DataTag index, out DataTag? value)
	{
		if (target is not GameState state)
		{
			value = null;
			return false;
		}

		var entityId = ToEntityId(index);
		var entity = state.Get(entityId);
		value = entity == null ? null : new ObjectRefTag(entity);
		return entity != null;
	}

	public bool TrySetIndex(object target, DataTag index, DataTag value) => false;

	private static EntityId ToEntityId(DataTag index) => index switch
	{
		GuidTag guidTag => new EntityId(guidTag.Value),
		StringTag stringTag when Guid.TryParse(stringTag.Value, out var guid) => new EntityId(guid),
		_ => EntityId.None
	};
}

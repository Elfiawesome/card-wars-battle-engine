using CardWars.BattleEngine.State;
using CardWars.Core.Data.Tags;

namespace CardWars.Client.scripts.vanilla.animation;

internal static class AnimationHelpers
{
	public static EntityId GetEntityId(CompoundTag data, string key)
		=> data.Get<GuidTag>(key) is { } tag ? new EntityId(tag.Value) : EntityId.None;
}

using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Mapping;
using CardWars.Core.Data.Tags;
using CardWars.Core.Logging;
using CardWars.Core.Registry;

namespace CardWars.BattleEngine.Vanilla.Loadouts;

[DataTagType("battle/loadout")]
public sealed class PlayerLoadout
{
	public static readonly ResourceId DefaultId = ResourceId.Vanilla("loadouts/default");

	[DataTag] public List<ResourceId> Deck { get; set; } = [];
	[DataTag] public List<ResourceId> Heroes { get; set; } = [];
	[DataTag] public List<UnitSlotPos> UnitSlots { get; set; } = [];
	[DataTag] public ResourceId IntroductionAnimation { get; set; } = ResourceId.Empty;

	public static PlayerLoadout? Get(CardWarsDefinitions? definitions, ResourceId id)
	{
		if (definitions is null) return null;

		var loadoutId = id.IsEmpty ? DefaultId : id;
		if (definitions.LoadoutDefinitions.Get(loadoutId) is not CompoundTag tag) return null;

		try
		{
			return DataTagMapper.FromTag<PlayerLoadout>(tag);
		}
		catch (Exception ex)
		{
			Log.Warn($"Could not parse player loadout [{loadoutId}]: {ex.Message}");
			return null;
		}
	}
}

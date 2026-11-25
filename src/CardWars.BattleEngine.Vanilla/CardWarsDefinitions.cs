using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;

namespace CardWars.BattleEngine.Vanilla;

public sealed class CardWarsDefinitions
{
	public Registry<ResourceId, CompoundTag> CardDefinitions { get; } = new();
	public Registry<ResourceId, CompoundTag> AttackDefinitions { get; } = new();
	public Registry<ResourceId, CompoundTag> RuleDefinitions { get; } = new();
	public Registry<ResourceId, CompoundTag> AbilityTypeDefinitions { get; } = new();
	public Registry<ResourceId, CompoundTag> LoadoutDefinitions { get; } = new();
}

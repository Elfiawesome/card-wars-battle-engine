using CardWars.Core.Data;
using CardWars.Core.Registry;

namespace CardWars.BattleEngine.State;

public record struct LayoutState(
	[property: DataTag] ResourceId Layout,
	[property: DataTag] CompoundTag LayoutConfig
)
{
	public LayoutState Copy() => new()
	{
		Layout = this.Layout,
		LayoutConfig = (CompoundTag)this.LayoutConfig.Clone() // Is this okay to cast?
	};
};
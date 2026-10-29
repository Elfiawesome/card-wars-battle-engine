using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;

namespace CardWars.BattleEngine.Behaviour.DataDriven.Runtime;

public sealed class ScriptScope(
	Dictionary<ResourceId, DataTag>? variables = null,
	HashSet<ResourceId>? constants = null)
{
	[DataTag] public Dictionary<ResourceId, DataTag> Variables { get; } = variables ?? [];
	[DataTag] public HashSet<ResourceId> Constants { get; } = constants ?? [];

	public void Clear()
	{
		Variables.Clear();
		Constants.Clear();
	}
}

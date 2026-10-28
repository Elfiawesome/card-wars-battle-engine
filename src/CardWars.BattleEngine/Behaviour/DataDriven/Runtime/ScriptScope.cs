using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;

namespace CardWars.BattleEngine.Behaviour.DataDriven.Runtime;

public sealed class ScriptScope
{
	public Dictionary<ResourceId, DataTag> Variables { get; } = [];
	public HashSet<ResourceId> Constants { get; } = [];
}

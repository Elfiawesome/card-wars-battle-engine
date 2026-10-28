using CardWars.BattleEngine.Behaviour.DataDriven.Instructions;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Registry;

namespace CardWars.BattleEngine.Behaviour.DataDriven;

[DataTagType("script/event_hook")]
public record struct EventHook(
	[property: DataTag] ResourceId EventType,
	[property: DataTag] List<IInstruction>? Instructions = null
);

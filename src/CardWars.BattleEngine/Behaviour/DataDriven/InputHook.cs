using CardWars.BattleEngine.Behaviour.DataDriven.Instructions;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Registry;

namespace CardWars.BattleEngine.Behaviour.DataDriven;

[DataTagType("script/input_hook")]
public record struct InputHook(
	[property: DataTag] ResourceId InputType,
	[property: DataTag] List<IInstruction>? Instructions = null
);

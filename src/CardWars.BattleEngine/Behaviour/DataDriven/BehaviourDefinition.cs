using CardWars.Core.Data.Attributes;

namespace CardWars.BattleEngine.Behaviour.DataDriven;

[DataTagType("script/behaviour")]
public record struct BehaviourDefinition(
	[property: DataTag] List<EventHook>? OnEvent = null,
	[property: DataTag] List<InputHook>? OnInput = null
)
{
	[DataTag] public int Priority { get; set; }
}

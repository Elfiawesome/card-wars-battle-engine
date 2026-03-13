using CardWars.BattleEngine.Input;
using CardWars.BattleEngine.State;
using CardWars.Core.Data;

namespace CardWars.BattleEngine.Vanilla.Features;

[DataTagType()]
public record struct AttackRequestInput(
	[property: DataTag] List<EntityId> UnitIds,
	[property: DataTag] EntityId TargetId
) : IInput;
using CardWars.BattleEngine.Behaviour.DataDriven.Expression;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;

namespace CardWars.BattleEngine.Behaviour.DataDriven;

public record struct BehaviourInstruction(
	[property: DataTag] List<EventInstructions> OnEvent,
	[property: DataTag] List<InputInstructions> OnInput
);

public record struct EventInstructions(
	[property: DataTag] ResourceId EventType,
	[property: DataTag] List<IInstructionLine> Instructions
);

public record struct InputInstructions(
	[property: DataTag] ResourceId InputType,
	[property: DataTag] List<IInstructionLine> Instructions
);

public interface IInstructionLine;

[DataTagType()]
public record struct DeclarationLine(
	[property: DataTag] ResourceId Name,
	[property: DataTag] IExpressionSpec Value
) : IInstructionLine;

[DataTagType()]
public record struct RaiseEvent(
	[property: DataTag] IExpressionSpec Event
) : IInstructionLine;

[DataTagType()]
public record struct StageBlock(
	[property: DataTag] IExpressionSpec Block
) : IInstructionLine;

[DataTagType()]
public record struct CommitBlock() : IInstructionLine;
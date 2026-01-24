using CardWars.BattleEngine.Input;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Block;

namespace CardWars.BattleEngine.Vanilla.Input;

public record struct PlayerJoinedInput(
	EntityId Id
) : IInput;

public class PlayerJoinedInputHandler : IInputHandler<PlayerJoinedInput>
{
	public void Handle(Transaction context, PlayerJoinedInput request)
	{
		context.ApplyBlockBatch(new([new InstantiatePlayerBlock(request.Id)]));
	}
}
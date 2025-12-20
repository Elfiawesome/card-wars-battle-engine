using CardWars.BattleEngine.State.Entity;

namespace CardWars.BattleEngine.Block.Turn;

public record struct AddAllowedPlayerInputsBlock(
	PlayerId PlayerId,
	bool ClearPrev = true
) : IBlock;

public class AddAllowedPlayerInputsBlockHandler : IBlockHandler<AddAllowedPlayerInputsBlock>
{
	public bool Handle(IServiceContainer service, AddAllowedPlayerInputsBlock request)
	{
		if (request.ClearPrev) { service.State.AllowedPlayerInputs.Clear(); }
		service.State.AllowedPlayerInputs.Add(request.PlayerId);
		return true;
	}
}
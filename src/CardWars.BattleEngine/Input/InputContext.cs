using CardWars.BattleEngine.State;
using CardWars.Core.Logging;

namespace CardWars.BattleEngine.Input;

public record struct InputContext(
	Transaction Transaction,
	EntityId PlayerId
)
{
	public readonly Logger Logger => Transaction.Logger;
}

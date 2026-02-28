using CardWars.BattleEngine.State;

namespace CardWars.BattleEngine.Input;

public record struct InputContext(
	Transaction Transaction,
	EntityId PlayerId
);
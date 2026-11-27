using CardWars.Vanilla.Shared;
using CardWars.Core.Registry;

namespace CardWars.BattleEngine.Vanilla.Targeting;

public static class TargetBindings
{
	public static readonly ResourceId Candidate = CardWarsIds.Vanilla("candidate");
	public static readonly ResourceId Card = CardWarsIds.Vanilla("card");
	public static readonly ResourceId Player = CardWarsIds.Vanilla("player");
}

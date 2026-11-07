using CardWars.BattleEngine.Rules;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Registry;

namespace CardWars.BattleEngine.Vanilla.Rules;

public sealed class HeroHrtLoseCondition : IEntityCondition
{
	public static readonly ResourceId Id = ResourceId.Vanilla("hero_lose_conditions/hrt");

	public bool IsMet(GameState state, EntityId heroId)
		=> state.Get<GenericCard>(heroId) is { } hero && hero.Hrt <= 0;
}
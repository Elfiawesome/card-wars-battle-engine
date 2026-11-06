using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.State;
using CardWars.Core.Data.Attributes;

namespace CardWars.BattleEngine.Vanilla.Features;

[DataTagType()]
public class HeroDiedEvent : IEvent
{
	[DataTag] public EntityId HeroCardId { get; set; }
	[DataTag] public EntityId PlayerId { get; set; }
	[DataTag] public EntityId BattlefieldId { get; set; }
	[DataTag] public int Hrt { get; set; }
}

public class HeroDiedEventHandler : IEventHandler<HeroDiedEvent>
{
	public void Handle(Transaction context, HeroDiedEvent request)
	{
	}
}

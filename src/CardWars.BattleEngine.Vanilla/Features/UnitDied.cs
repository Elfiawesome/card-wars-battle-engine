using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.State;
using CardWars.Core.Data.Attributes;

namespace CardWars.BattleEngine.Vanilla.Features;

[DataTagType()]
public class UnitDiedEvent : IEvent
{
	[DataTag] public EntityId CardId { get; set; }
	[DataTag] public EntityId PlayerId { get; set; }
	[DataTag] public EntityId BattlefieldId { get; set; }
	[DataTag] public EntityId UnitSlotId { get; set; }
}

public class UnitDiedEventHandler : IEventHandler<UnitDiedEvent>
{
	public void Handle(Transaction context, UnitDiedEvent request)
	{
	}
}

namespace CardWars.BattleEngine.State.Entity;

public class Card<TCardId>(TCardId id) : EntityState<TCardId>(id)
	where TCardId : ICardId
{
	public virtual Dictionary<TargetPlay, List<IStateId>> PlayableOn { get; set; } = [];
}

public enum TargetPlay
{
	None,
	Player,
	Battlefield,
	UnitSlot,
	UnitCard,
	SpellCard,
}

public interface ICardId : IStateId;
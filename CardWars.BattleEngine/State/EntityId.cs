namespace CardWars.BattleEngine.State;

public readonly record struct EntityId(Guid Value)
{
	public static EntityId New() => new(Guid.NewGuid());
	public static readonly EntityId None = new(Guid.Empty);

	public bool IsNone => Value == Guid.Empty;

	public override string ToString() => Value.ToString()[..8];

	public static implicit operator EntityId(Guid guid) => new(guid);
}
using CardWars.Core.Data;

namespace CardWars.BattleEngine.State;

[DataTagConverter(typeof(EntityIdTagConverter))]
public readonly record struct EntityId(Guid Value)
{
	public static EntityId New() => new(Guid.NewGuid());
	public static readonly EntityId None = new(Guid.Empty);

	public bool IsNone => Value == Guid.Empty;

	public override string ToString() => Value.ToString();//[..8];

	public static implicit operator EntityId(Guid guid) => new(guid);
}

public class EntityIdTagConverter : DataTagConverter<EntityId>
{
	protected override DataTag? ConvertTo(EntityId value) => new GuidTag(value.Value);
	protected override EntityId ConvertFrom(DataTag tag) => tag switch
	{
		GuidTag g => new EntityId(g.Value),
		StringTag s when Guid.TryParse(s.Value, out var g) => new EntityId(g),
		_ => EntityId.None
	};
}
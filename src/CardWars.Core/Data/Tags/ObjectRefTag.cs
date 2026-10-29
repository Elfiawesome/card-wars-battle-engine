namespace CardWars.Core.Data.Tags;

public sealed class ObjectRefTag(object value) : DataTag
{
	public object Value { get; } = value;
	public override DataTagKind Type => DataTagKind.ObjectRef;
	public override DataTag Clone() => new ObjectRefTag(Value);

	public override bool IsTruthy => Value is not null;
}

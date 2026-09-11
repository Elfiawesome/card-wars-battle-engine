namespace CardWars.Core.Data.Tags;

public abstract class DataTag
{
	public abstract DataTagKind Type { get; }
	public abstract DataTag Clone();
}
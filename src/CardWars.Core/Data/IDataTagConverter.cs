namespace CardWars.Core.Data;

public interface IDataTagConverter
{
	DataTag? ToTag(object value);
	object? FromTag(DataTag tag);
}

public abstract class DataTagConverter<T> : IDataTagConverter
{
	protected abstract DataTag? ConvertTo(T value);
	protected abstract T ConvertFrom(DataTag tag);

	public DataTag? ToTag(object value) => value is T typed ? ConvertTo(typed) : null;
	public object? FromTag(DataTag tag) => ConvertFrom(tag);
}
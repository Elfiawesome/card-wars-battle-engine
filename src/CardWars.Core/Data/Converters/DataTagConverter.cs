using CardWars.Core.Data.Tags;

namespace CardWars.Core.Data.Converters;

public abstract class DataTagConverter<T> : IDataTagConverter
{
	protected abstract DataTag? ConvertTo(T value);
	protected abstract T ConvertFrom(DataTag tag);

	public DataTag? ToTag(object value) => value is T typed ? ConvertTo(typed) : null;
	public object? FromTag(DataTag tag) => ConvertFrom(tag);
}
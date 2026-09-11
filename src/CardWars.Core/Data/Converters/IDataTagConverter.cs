using CardWars.Core.Data.Tags;

namespace CardWars.Core.Data.Converters;

public interface IDataTagConverter
{
	DataTag? ToTag(object value);
	object? FromTag(DataTag tag);
}
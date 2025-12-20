namespace CardWars.Core.Common.Data;

public class DataCompound
{
	public DataCompound() { }

	private Dictionary<string, object> _data = [];

	public object this[string key] { set { _data[key] = value; } }

	public T? Get<T>(string key)
	{
		if (_data[key] is T v)
		{
			return v;
		}
		return default;
	}
}
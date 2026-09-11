namespace CardWars.Core.Data.Tags;

public sealed class ListTag : DataTag
{
	private readonly List<DataTag> _items = [];
	public override DataTagKind Type => DataTagKind.List;

	public IReadOnlyList<DataTag> Items => _items;
	public int Count => _items.Count;

	public ListTag Add(DataTag tag) { _items.Add(tag); return this; }
	public ListTag Add(int value) => Add(new IntTag(value));
	public ListTag Add(string value) => Add(new StringTag(value));
	public ListTag Add(Guid value) => Add(new GuidTag(value));
	// public ListTag Add<TEnum>(TEnum value) where TEnum : struct, Enum
	// 	=> Add(new StringTag(value.ToString()));

	public DataTag? Get(int index) => index >= 0 && index < _items.Count ? _items[index] : null;
	public T? Get<T>(int index) where T : DataTag => Get(index) as T;
	public T Get<T>(int index, T defaultValue) where T : DataTag => Get(index) as T ?? defaultValue;
	public void Set(int index, DataTag tag) { if (index >= 0 && index < _items.Count) _items[index] = tag; }
	public bool RemoveAt(int index)
	{
		if (index < 0 || index >= _items.Count) return false;
		_items.RemoveAt(index);
		return true;
	}

	public override DataTag Clone()
	{
		var clone = new ListTag();
		foreach (var item in _items) clone.Add(item.Clone());
		return clone;
	}
}
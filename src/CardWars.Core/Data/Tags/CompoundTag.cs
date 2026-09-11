namespace CardWars.Core.Data.Tags;

public sealed class CompoundTag : DataTag
{
	private readonly Dictionary<string, DataTag> _entries = [];
	public override DataTagKind Type => DataTagKind.Compound;

	public IReadOnlyDictionary<string, DataTag> Entries => _entries;
	public int Count => _entries.Count;

	// --- Core ---

	public DataTag? Get(string key) => _entries.GetValueOrDefault(key);
	public T? Get<T>(string key) where T : DataTag => Get(key) as T;
	public T Get<T>(string key, T defaultValue) where T : DataTag => Get(key) as T ?? defaultValue;

	public bool Has(string key) => _entries.ContainsKey(key);
	public bool Remove(string key) => _entries.Remove(key);

	// --- Fluent Setters ---

	public CompoundTag Set(string key, DataTag tag) { _entries[key] = tag; return this; }
	public CompoundTag Set(string key, int value) => Set(key, new IntTag(value));
	public CompoundTag Set(string key, float value) => Set(key, new FloatTag(value));
	public CompoundTag Set(string key, string value) => Set(key, new StringTag(value));
	public CompoundTag Set(string key, bool value) => Set(key, new BoolTag(value));
	public CompoundTag Set(string key, Guid value) => Set(key, new GuidTag(value));
	// public CompoundTag Set<TEnum>(string key, TEnum value) where TEnum : struct, Enum
	// 	=> Set(key, new StringTag(value.ToString()));

	// --- Typed Getters ---

	public int GetInt(string key, int fallback = 0) => Get<IntTag>(key)?.Value ?? fallback;
	public float GetFloat(string key, float fallback = 0f) => Get<FloatTag>(key)?.Value ?? fallback;
	public string GetString(string key, string fallback = "") => Get<StringTag>(key)?.Value ?? fallback;
	public bool GetBool(string key, bool fallback = false) => Get<BoolTag>(key)?.Value ?? fallback;
	public Guid GetGuid(string key) => Get<GuidTag>(key)?.Value ?? Guid.Empty;
	public CompoundTag? GetCompound(string key) => Get<CompoundTag>(key);
	public ListTag? GetList(string key) => Get<ListTag>(key);

	// --- Path Access ---

	public DataTag? GetByPath(string path)
	{
		DataTag? current = this;
		foreach (var segment in path.Split('.'))
		{
			current = current switch
			{
				CompoundTag c => c.Get(segment),
				ListTag l when int.TryParse(segment, out var i) => l.Get(i),
				_ => null
			};
			if (current == null) return null;
		}
		return current;
	}

	public int GetIntByPath(string path, int fallback = 0) => (GetByPath(path) as IntTag)?.Value ?? fallback;
	public string GetStringByPath(string path, string fallback = "") => (GetByPath(path) as StringTag)?.Value ?? fallback;

	public void SetByPath(string path, DataTag value)
	{
		var segments = path.Split('.');
		DataTag? current = this;

		for (int i = 0; i < segments.Length - 1; i++)
		{
			var next = current switch
			{
				CompoundTag c => c.Get(segments[i]),
				ListTag l when int.TryParse(segments[i], out var idx) => l.Get(idx),
				_ => null
			};

			if (next == null && current is CompoundTag compound)
			{
				next = new CompoundTag();
				compound.Set(segments[i], next);
			}

			current = next;
			if (current == null) return;
		}

		var last = segments[^1];
		if (current is CompoundTag lastCompound) lastCompound.Set(last, value);
		else if (current is ListTag lastList && int.TryParse(last, out var lastIdx)) lastList.Set(lastIdx, value);
	}

	// --- Merge & Clone ---

	public void Merge(CompoundTag other)
	{
		foreach (var (key, tag) in other._entries)
			_entries[key] = tag.Clone();
	}

	public override DataTag Clone()
	{
		var clone = new CompoundTag();
		foreach (var (key, tag) in _entries)
			clone.Set(key, tag.Clone());
		return clone;
	}
}
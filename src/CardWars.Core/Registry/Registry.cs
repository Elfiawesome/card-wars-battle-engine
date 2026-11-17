using System.Collections.Concurrent;

namespace CardWars.Core.Registry;

public class Registry<TId, T>
	where TId : notnull
	where T : notnull
{
	// Backed by a ConcurrentDictionary so the shared registry pattern is safe for
	// concurrent Register/Get. The data layer's id<->Type map (DataContext) is written
	// by assembly scanning and read during serialization on different threads (e.g. the
	// server tick thread plus setup), so the one registry pattern every subsystem shares
	// must tolerate that. Observable behavior is unchanged: Get returns default when the
	// id is absent, Register overwrites an existing entry.
	private readonly ConcurrentDictionary<TId, T> _registry = new();

	public void Register(TId id, T item) => _registry[id] = item;

	public T? Get(TId id) => _registry.TryGetValue(id, out var v) ? v : default;

	public void Clear() => _registry.Clear();
}

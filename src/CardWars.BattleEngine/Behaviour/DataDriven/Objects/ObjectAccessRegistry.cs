using CardWars.Core.Data.Tags;

namespace CardWars.BattleEngine.Behaviour.DataDriven.Objects;

public sealed class ObjectAccessRegistry
{
	private readonly List<(Type Type, IObjectAccessor Accessor)> _accessors = [];
	private readonly ReflectionObjectAccessor _fallback = new();

	public void Register<T>(IObjectAccessor accessor) => _accessors.Add((typeof(T), accessor));

	public bool TryGetMember(object target, string member, out DataTag? value)
	{
		foreach (var (type, accessor) in _accessors)
		{
			if (type.IsInstanceOfType(target) && accessor.TryGetMember(target, member, out value))
				return true;
		}

		return _fallback.TryGetMember(target, member, out value);
	}

	public bool TrySetMember(object target, string member, DataTag value)
	{
		foreach (var (type, accessor) in _accessors)
		{
			if (type.IsInstanceOfType(target) && accessor.TrySetMember(target, member, value))
				return true;
		}

		return _fallback.TrySetMember(target, member, value);
	}

	public bool TryGetIndex(object target, DataTag index, out DataTag? value)
	{
		foreach (var (type, accessor) in _accessors)
		{
			if (type.IsInstanceOfType(target) && accessor.TryGetIndex(target, index, out value))
				return true;
		}

		return _fallback.TryGetIndex(target, index, out value);
	}

	public bool TrySetIndex(object target, DataTag index, DataTag value)
	{
		foreach (var (type, accessor) in _accessors)
		{
			if (type.IsInstanceOfType(target) && accessor.TrySetIndex(target, index, value))
				return true;
		}

		return _fallback.TrySetIndex(target, index, value);
	}
}

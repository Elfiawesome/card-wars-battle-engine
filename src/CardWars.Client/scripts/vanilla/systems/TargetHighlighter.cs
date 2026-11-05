using System;
using System.Collections.Generic;
using System.Linq;
using CardWars.BattleEngine.State;
using CardWars.Client.scenes.vanilla.instance.battle;

namespace CardWars.Client.scripts.vanilla.systems;

public sealed class TargetHighlighter
{
	private readonly Dictionary<object, HashSet<EntityId>> _sources = [];
	private HashSet<EntityId> _applied = [];
	private bool _dirty = true;

	public void Set(object source, IEnumerable<EntityId> targets)
	{
		_sources[source] = [.. targets];
		_dirty = true;
	}

	public void Clear(object source)
	{
		if (_sources.Remove(source)) _dirty = true;
	}

	public void ClearAll()
	{
		_sources.Clear();
		_dirty = true;
	}

	public void Invalidate() => _dirty = true;

	public void Process(BattleInstance battle)
	{
		var next = _sources.Values.SelectMany(targets => targets).ToHashSet();
		if (!_dirty && next.SetEquals(_applied)) return;

		foreach (var entityId in _applied)
			battle.GetEntityNode<EntityNode3D>(entityId)?.SetValidTarget(false);

		foreach (var entityId in next)
			battle.GetEntityNode<EntityNode3D>(entityId)?.SetValidTarget(true);

		_applied = next;
		_dirty = false;
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using CardWars.Client.scenes.vanilla.instance.battle;
using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;

namespace CardWars.Client.scripts.vanilla.systems;

public class BattleAnimationHandler
{
	private class AnimationRack
	{
		public Queue<BattleAnimation> Queue { get; } = new();
		public BattleAnimation? CurrentAnimation { get; set; }
		public Action? CompletedCallback { get; set; }
		public bool IsCompleted { get; set; }
	}

	private readonly Dictionary<ResourceId, AnimationRack> _racks = [];

	public event Action? Completed;

	public void QueueAnimation(BattleAnimation animation, ResourceId? rackIdN = null)
	{
		var rackId = rackIdN ?? ResourceId.Empty;
		if (!_racks.TryGetValue(rackId, out var rack))
		{
			rack = new AnimationRack();
			_racks[rackId] = rack;
		}

		rack.Queue.Enqueue(animation);
	}

	public void Process(BattleInstance instance, double delta)
	{
		var wasBusy = IsBusy;
		var racks = _racks.Values.ToArray();

		foreach (var rack in racks)
		{
			ProcessRack(rack, instance, delta);
		}

		if (wasBusy && !IsBusy) Completed?.Invoke();
	}

	private void ProcessRack(AnimationRack rack, BattleInstance instance, double delta)
	{
		if (rack.CurrentAnimation is not null && rack.IsCompleted)
		{
			EndCurrentAnimation(rack, instance);
		}

		while (rack.CurrentAnimation is null && rack.Queue.Count > 0)
		{
			StartNextAnimation(rack, instance);
		}

		if (rack.CurrentAnimation is not null)
		{
			rack.CurrentAnimation.Process(instance, delta);

			if (rack.IsCompleted)
			{
				EndCurrentAnimation(rack, instance);
			}
		}
	}

	private void StartNextAnimation(AnimationRack rack, BattleInstance instance)
	{
		var anim = rack.Queue.Dequeue();
		rack.CurrentAnimation = anim;
		rack.IsCompleted = false;

		// Callback handler
		void onCompleted() => rack.IsCompleted = true;
		rack.CompletedCallback = onCompleted;
		anim.Completed += onCompleted;

		anim.Start(instance);

		if (rack.IsCompleted)
		{
			EndCurrentAnimation(rack, instance);
		}
	}

	private void EndCurrentAnimation(AnimationRack rack, BattleInstance instance)
	{
		if (rack.CurrentAnimation is null) return;

		var anim = rack.CurrentAnimation;

		if (rack.CompletedCallback is not null)
		{
			anim.Completed -= rack.CompletedCallback;
			rack.CompletedCallback = null;
		}

		anim.Destroy(instance);
		rack.CurrentAnimation = null;
		rack.IsCompleted = false;
	}

	public bool IsBusy => _racks.Values.Any(r => r.CurrentAnimation is not null || r.Queue.Count > 0);

	public bool IsRackBusy(ResourceId rackId)
	{
		return _racks.TryGetValue(rackId, out var rack) &&
			   (rack.CurrentAnimation is not null || rack.Queue.Count > 0);
	}

	public void ClearRack(ResourceId rackId, BattleInstance instance)
	{
		if (!_racks.TryGetValue(rackId, out var rack)) return;

		var wasBusy = IsBusy;
		if (rack.CurrentAnimation is not null)
		{
			EndCurrentAnimation(rack, instance);
		}

		while (rack.Queue.Count > 0)
		{
			var anim = rack.Queue.Dequeue();
			anim.Destroy(instance);
		}

		if (wasBusy && !IsBusy) Completed?.Invoke();
	}

	public void ClearAll(BattleInstance instance)
	{
		foreach (var rackId in _racks.Keys.ToArray())
		{
			ClearRack(rackId, instance);
		}
	}
}





public abstract class BattleAnimation
{
	public Action? Completed;
	public CompoundTag Data = new();

	public abstract void Start(BattleInstance instance);
	public virtual void Process(BattleInstance instance, double delta) { }
	public virtual void Destroy(BattleInstance instance) { }

	public void Complete()
	{
		Completed?.Invoke();
	}
}
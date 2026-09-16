using System.Collections.Generic;
using CardWars.Client.scenes.vanilla.instance.battle;
using CardWars.Core.Logging;
using Godot;

namespace CardWars.Client.scripts.vanilla.animation;

public class TestAnimation : BattleAnimation
{
	public override void Start(BattleInstance instance)
	{
		var tween = instance.CreateTween();
		tween.SetParallel(true).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);

		Log.Info("Running test animation");
		float delay = 0.0f;
		foreach (var e in instance.EntityNodes)
		{
			Log.Info(e);
			if (e.Value is UnitSlotNode unitSlotNode)
			{
				tween.TweenProperty(unitSlotNode, "position", unitSlotNode.Position, 1.0).SetDelay(delay);
				tween.TweenProperty(unitSlotNode, "scale", unitSlotNode.Scale, 1.0).SetDelay(delay);
				unitSlotNode.Position = new(0, 0, 0);
				unitSlotNode.Scale = new(0, 0, 0);
				delay += 0.1f;
			}
		}
		tween.Finished += Complete;
	}
}
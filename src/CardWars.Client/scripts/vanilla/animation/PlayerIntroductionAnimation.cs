using CardWars.BattleEngine.State;
using CardWars.Client.scenes.vanilla.instance.battle;
using CardWars.Core.Data.Tags;
using Godot;

namespace CardWars.Client.scripts.vanilla.animation;

public class PlayerIntroductionAnimation : BattleAnimation
{
	public override void Start(BattleInstance instance)
	{
		var tween = instance.CreateTween();
		tween.SetParallel(true).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);

		float delay = 0.0f;
		var unitSlots = Data.Get<ListTag>("unit_slots") ?? [];
		foreach (var unitSlotGuidTag in unitSlots.Enumerate<GuidTag>())
		{
			var unitSlotNode = instance.GetEntityNode<UnitSlotNode>(new EntityId(unitSlotGuidTag.Value));
			if (unitSlotNode is null) continue;
			tween.TweenProperty(unitSlotNode, "position", unitSlotNode.Position, 1.0).SetDelay(delay);
			tween.TweenProperty(unitSlotNode, "scale", unitSlotNode.Scale, 1.0).SetDelay(delay);
			unitSlotNode.Position = new(0, 0, 0);
			unitSlotNode.Scale = Vector3.One * 0.0001f;
			delay += 0.1f;
		}
		tween.Finished += Complete;
	}
}
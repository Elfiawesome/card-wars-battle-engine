using CardWars.BattleEngine.State;
using CardWars.Client.scenes.vanilla.instance.battle;
using CardWars.Client.scripts.vanilla.systems;
using CardWars.Core.Data.Tags;
using Godot;

namespace CardWars.Client.scripts.vanilla.animation;

public sealed class AttackAnimation : BattleAnimation
{
	public override void Start(BattleInstance instance)
	{
		var attackerId = AnimationHelpers.GetEntityId(Data, "attacker");
		var targetId = AnimationHelpers.GetEntityId(Data, "target");

		if (instance.GetEntityNode<Card3D>(attackerId) is not { } attacker
			|| instance.GetEntityNode<Card3D>(targetId) is not { } target)
		{
			Complete();
			return;
		}

		var start = attacker.Position;
		var parent = attacker.GetParent<Node3D>();
		var worldDelta = target.GlobalPosition - attacker.GlobalPosition;
		var localDelta = parent is null
			? worldDelta
			: parent.GlobalBasis.Inverse() * worldDelta;

		if (localDelta.LengthSquared() < 0.0001f)
		{
			Complete();
			return;
		}

		var lunge = start + localDelta.Normalized() * 0.25f;
		var tween = instance.CreateTween()
			.SetTrans(Tween.TransitionType.Cubic)
			.SetEase(Tween.EaseType.Out);

		tween.TweenProperty(attacker, "position", lunge, 0.1);
		tween.TweenProperty(attacker, "position", start, 0.16);
		tween.Finished += Complete;
	}
}

using CardWars.Client.scenes.vanilla.instance.battle;
using CardWars.Client.scripts.vanilla.systems;
using CardWars.Vanilla.Shared;
using Godot;

namespace CardWars.Client.scripts.vanilla.animation;

public sealed class TakeDamageAnimation : BattleAnimation
{
	public override void Start(BattleInstance instance)
	{
		var cardId = AnimationHelpers.GetEntityId(Data, SharedIds.Animations.Args.CardId);
		if (instance.GetEntityNode<Card3D>(cardId) is not { } card)
		{
			Complete();
			return;
		}

		var start = card.Position;
		var tween = instance.CreateTween()
			.SetTrans(Tween.TransitionType.Quad)
			.SetEase(Tween.EaseType.Out);

		tween.TweenProperty(card, "position", start + new Vector3(0.07f, 0f, 0f), 0.04);
		tween.TweenProperty(card, "position", start - new Vector3(0.07f, 0f, 0f), 0.06);
		tween.TweenProperty(card, "position", start + new Vector3(0.04f, 0f, 0f), 0.05);
		tween.TweenProperty(card, "position", start, 0.05);
		tween.Finished += Complete;
	}
}

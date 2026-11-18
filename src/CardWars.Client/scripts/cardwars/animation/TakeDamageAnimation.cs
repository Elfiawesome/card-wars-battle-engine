using System;
using CardWars.Client.scenes.cardwars.instance.battle;
using CardWars.Client.scenes.cardwars.instance.battle.entities;
using CardWars.Client.scripts.cardwars.systems;
using CardWars.Vanilla.Shared;
using Godot;

namespace CardWars.Client.scripts.cardwars.animation;

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


		float dur = 0.02f;
		int maxShakeTimes = 4;
		float maxShakeAmt = 0.06f;
		var rand = new Random();
		for (var i = 0; i < maxShakeTimes; i++)
		{
			tween.TweenProperty(card, "position", start + new Vector3(
				maxShakeAmt * (1.0f - i / (float)maxShakeTimes) * rand.Next(-1, 1),
				0f,
				maxShakeAmt * (1.0f - i / (float)maxShakeTimes) * rand.Next(-1, 1)), dur
			);
		}
		tween.TweenProperty(card, "position", start, dur);
		tween.Finished += Complete;
	}
}

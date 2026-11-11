using CardWars.Client.scenes.cardwars.instance.battle;
using CardWars.Client.scenes.cardwars.instance.battle.entities;
using CardWars.Client.scripts.cardwars.systems;
using CardWars.Vanilla.Shared;
using Godot;

namespace CardWars.Client.scripts.cardwars.animation;

public sealed class PlayCardAnimation : BattleAnimation
{
	public override void Start(BattleInstance instance)
	{
		var cardId = AnimationHelpers.GetEntityId(Data, SharedIds.Animations.Args.CardId);
		if (instance.GetEntityNode<Card3D>(cardId) is not { } card)
		{
			Complete();
			return;
		}

		var targetPosition = card.Position;
		card.Position = targetPosition + new Vector3(0f, 0f, +0.35f);
		card.Scale = Vector3.One * 0.15f;

		var tween = instance.CreateTween()
			.SetParallel(true)
			.SetTrans(Tween.TransitionType.Cubic)
			.SetEase(Tween.EaseType.Out);

		tween.TweenProperty(card, "position", targetPosition, 0.5);
		tween.TweenProperty(card, "scale", Vector3.One, 0.5);
		tween.Finished += Complete;
	}
}

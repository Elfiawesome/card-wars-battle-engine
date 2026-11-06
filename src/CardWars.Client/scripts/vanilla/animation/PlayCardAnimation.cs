using CardWars.Client.scenes.vanilla.instance.battle;
using CardWars.Client.scripts.vanilla.systems;
using Godot;

namespace CardWars.Client.scripts.vanilla.animation;

public sealed class PlayCardAnimation : BattleAnimation
{
	public override void Start(BattleInstance instance)
	{
		var cardId = AnimationHelpers.GetEntityId(Data, "card_id");
		if (instance.GetEntityNode<Card3D>(cardId) is not { } card)
		{
			Complete();
			return;
		}

		var targetPosition = card.Position;
		card.Position = targetPosition + new Vector3(0f, -0.35f, 0f);
		card.Scale = Vector3.One * 0.15f;

		var tween = instance.CreateTween()
			.SetParallel(true)
			.SetTrans(Tween.TransitionType.Cubic)
			.SetEase(Tween.EaseType.Out);

		tween.TweenProperty(card, "position", targetPosition, 0.25);
		tween.TweenProperty(card, "scale", Vector3.One, 0.25);
		tween.Finished += Complete;
	}
}

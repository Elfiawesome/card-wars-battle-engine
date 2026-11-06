using CardWars.BattleEngine.State;
using CardWars.Client.scenes.vanilla.instance.battle;
using CardWars.Client.scripts.vanilla.systems;
using CardWars.Core.Data.Tags;
using Godot;

namespace CardWars.Client.scripts.vanilla.animation;

public sealed class DeathAnimation : BattleAnimation
{
	public override void Start(BattleInstance instance)
	{
		var unitList = Data.GetList("units") ?? new();

		var tween = instance.CreateTween()
			.SetParallel(true)
			.SetTrans(Tween.TransitionType.Cubic)
			.SetEase(Tween.EaseType.Out);

		foreach (var item in unitList)
		{
			if (item is GuidTag unitId)
			{
				var node = instance.GetPendingEntityNode<EntityNode3D>(new EntityId(unitId.Value));
				if (node is not null)
				{
					tween.TweenProperty(node, "position", node.Position + new Vector3(0f, -0.15f, 0f), 0.5);
					tween.TweenProperty(node, "scale", new Vector3(0.001f, 0.001f, 0.001f), 0.5);
				}
			}
		}

		tween.Finished += Complete;
	}
}

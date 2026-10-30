using CardWars.Core.Data.Tags;
using Godot;

namespace CardWars.Client;

public partial class CardDisplay : Control
{
	public Tween? currentAnimationTween;

	public void SetData(CompoundTag data)
	{
		if (GetNodeOrNull<Label>("NameLabel") is { } nameLabel)
			nameLabel.Text = data.GetString("name");

		if (GetNodeOrNull<Label>("HpLabel") is { } hpLabel)
			hpLabel.Text = $"HP {data.GetInt("hp")}";

		if (GetNodeOrNull<Label>("AtkLabel") is { } atkLabel)
			atkLabel.Text = $"ATK {data.GetInt("atk")}";
	}
}

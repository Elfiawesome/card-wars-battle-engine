using CardWars.Core.Data.Tags;
using Godot;

namespace CardWars.Client;

public partial class CardDisplay : Control
{
	public Tween? currentAnimationTween;

	public void SetData(CompoundTag data)
	{
		var isHero = data.GetString("card_type") == "hero";

		if (GetNodeOrNull<Label>("NameLabel") is { } nameLabel)
			nameLabel.Text = data.GetString("name");

		if (GetNodeOrNull<Label>("HpLabel") is { } hpLabel)
			hpLabel.Text = $"{data.GetInt(isHero ? "hrt" : "hp")}";

		if (GetNodeOrNull<Label>("AtkLabel") is { } atkLabel)
		{
			atkLabel.Text = $"{data.GetInt("atk")}";
			atkLabel.Visible = !isHero;
		}

		if (GetNodeOrNull<Label>("ChargeLabel") is { } chargeLabel)
		{
			chargeLabel.Text = $"{data.GetInt("charge")}";
			chargeLabel.Visible = !isHero;
		}
	}
}

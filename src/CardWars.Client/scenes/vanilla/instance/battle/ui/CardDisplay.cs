using CardWars.Core.Data.Tags;
using Godot;

namespace CardWars.Client;

public partial class CardDisplay : Control
{
	public Tween? currentAnimationTween;

	[Export] public Label? NameLabelNode;
	[Export] public Label? WorldNameLabelNode;
	[Export] public Label? CardTypeLabelNode;
	[Export] public Label? PtLabelNode;
	[Export] public Label? HpLabelNode;
	[Export] public Label? AtkLabelNode;
	[Export] public Label? FlavourTextLabelNode;

	public void SetData(CompoundTag data)
	{
		if (NameLabelNode is { } nameLabelNode)
			nameLabelNode.Text = data.GetString("name");

		if (CardTypeLabelNode is { } cardTypeLabelNode)
			cardTypeLabelNode.Text = data.GetString("card_type").ToUpper();

		if (PtLabelNode is { } ptLabelNode)
			ptLabelNode.Text = $"{data.GetInt("pt")}";

		if (HpLabelNode is { } hpLabelNode)
			hpLabelNode.Text = $"{data.GetInt("hp")}";

		if (AtkLabelNode is { } atkLabelNode)
			atkLabelNode.Text = $"{data.GetInt("atk")}";
	}

	public void SetDisplay(DisplayType displayType)
	{
		var statContainer = GetNode<MarginContainer>("InfoContainer/BottomInfoContainer/MainInfoContainer/MainInfoContainer/StatContainer");

		switch (displayType)
		{
			case DisplayType.Hero: break;
			case DisplayType.Unit: break;
			case DisplayType.Spell:
				statContainer.Visible = false;
				break;
			case DisplayType.BattlefieldUnit: break;
		}
	}

	public enum DisplayType { Hero, Unit, Spell, BattlefieldUnit, }
}

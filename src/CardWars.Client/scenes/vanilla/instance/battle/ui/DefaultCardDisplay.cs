using CardWars.Core.Data.Tags;
using Godot;

namespace CardWars.Client;

public partial class DefaultCardDisplay : CardDisplay
{

	[Export] Control? PtContainer;
	[Export] Label? PtLabel;
	[Export] Control? HrtContainer;
	[Export] Label? HrtLabel;
	[Export] Label? NameLabel;
	[Export] Label? WorldLabel;
	[Export] Label? CardTypeLabel;
	[Export] TextureRect? BtmDivStarTexture;
	[Export] TextureRect? BtmDivGradTexture;
	[Export] MarginContainer? StatContainer;
	[Export] Label? HpLabel;
	[Export] Label? AtkLabel;
	[Export] Label? FlavourTextLabel;

	public Vector2 PortraitSize = new(1968.0f, 1968.0f);
	public Vector2 LandscapeSize = new(1968.0f, 1408.0f);

	public override void SetData(CompoundTag data)
	{
		if (
			PtContainer is { } ptContainer &&
			PtLabel is { } ptLabel &&
			HrtContainer is { } hrtContainer &&
			HrtLabel is { } hrtLabel &&
			NameLabel is { } nameLabel &&
			WorldLabel is { } worldLabel &&
			CardTypeLabel is { } cardTypeLabel &&
			BtmDivStarTexture is { } btmDivStarTexture &&
			BtmDivGradTexture is { } btmDivGradTexture &&
			StatContainer is { } statContainer &&
			HpLabel is { } hpLabel &&
			AtkLabel is { } atkLabel &&
			FlavourTextLabel is { } flavourTextLabel
		)
		{
			nameLabel.Text = data.GetString("name");
			worldLabel.Text = data.GetString("world");
			ptLabel.Text = $"{data.GetInt("pt")}";
			hpLabel.Text = $"{data.GetInt("hp")}";
			atkLabel.Text = $"{data.GetInt("atk")}";
			flavourTextLabel.Text = data.GetString("flavour_text");
			hrtLabel.Text = $"{data.GetInt("hrt")}";

			statContainer.Visible = false;
			ptContainer.Visible = false;
			hrtContainer.Visible = false;

			var cardType = data.GetString("card_type");
			cardTypeLabel.Text = cardType;
			switch (cardType)
			{
				case "unit":
					statContainer.Visible = true;
					ptContainer.Visible = true;
					Size = PortraitSize;
					break;
				case "spell":
					ptContainer.Visible = true;
					Size = PortraitSize;
					break;
				case "hero":
					hrtContainer.Visible = true;
					Size = LandscapeSize;
					break;
			}

		}
	}
}

public abstract partial class CardDisplay : Control
{
	public DisplayType Display { get; protected set; } = DisplayType.Full;
	public enum DisplayType { Full, Simplified }

	public Tween? currentAnimationTween;

	public virtual void SetData(CompoundTag data) { }
}
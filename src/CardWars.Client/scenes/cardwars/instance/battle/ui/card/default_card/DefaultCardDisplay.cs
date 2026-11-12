using System.Linq;
using CardWars.Client.scripts.core;
using CardWars.Core.Data.Tags;
using CardWars.Core.Logging;
using CardWars.Core.Registry;
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
	[Export] VBoxContainer? SideStatsContainer;
	[Export] TextureRect? BtmDivStarTexture;
	[Export] TextureRect? BtmDivGradTexture;
	[Export] MarginContainer? StatContainer;
	[Export] Label? HpLabel;
	[Export] Label? AtkLabel;
	[Export] Label? FlavourTextLabel;
	[Export] VBoxContainer? AbilitiesContainer;

	public Vector2 PortraitSize = new(1408.0f, 1968.0f);
	public Vector2 LandscapeSize = new(1968.0f, 1408.0f);

	public PackedScene AbilityTextScene = GD.Load<PackedScene>("res://scenes/cardwars/instance/battle/ui/card/default_card/ability_text.tscn");
	public PackedScene SpAtkBox = GD.Load<PackedScene>("res://scenes/cardwars/instance/battle/ui/card/default_card/sp_atk_box.tscn");

	public override void SetData(CompoundTag data, ClientRegistry registry)
	{
		if (
			PtContainer is { } ptContainer &&
			PtLabel is { } ptLabel &&
			HrtContainer is { } hrtContainer &&
			HrtLabel is { } hrtLabel &&
			NameLabel is { } nameLabel &&
			WorldLabel is { } worldLabel &&
			CardTypeLabel is { } cardTypeLabel &&
			SideStatsContainer is { } sideStatsContainer &&
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

					if (data.GetList("abilities") is { } abilities && AbilitiesContainer is { } abilitiesContainer)
					{
						abilitiesContainer.Visible = true;

						var abilityTags = abilities.Items.OfType<CompoundTag>().ToList();

						// Grow / shrink the container to match the data
						var children = abilitiesContainer.GetChildren();
						int diff = abilityTags.Count - children.Count;

						if (diff > 0)
						{
							for (int i = 0; i < diff; i++)
							{
								var entry = AbilityTextScene.Instantiate<HBoxContainer>();
								abilitiesContainer.AddChild(entry);
							}
						}
						else if (diff < 0)
						{
							for (int i = 0; i < -diff; i++)
							{
								var last = abilitiesContainer.GetChild(abilitiesContainer.GetChildCount() - 1);
								last.QueueFree();
							}
						}

						var entries = abilitiesContainer.GetChildren();
						for (int i = 0; i < abilityTags.Count; i++)
						{
							var tag = abilityTags[i];
							var entry = entries[i];

							// Populate.
							if (entry is HBoxContainer)
							{
								var aDesc = tag.GetString("description");
								var aTrigger = tag.GetString("ability_type");
								var acolor = new Color(tag.GetString("color"));
								var aTexture = registry.Resources.Instantiate<Texture>(ResourceId.Parse(tag.GetString("texture")))
									?? new ImageTexture();
								entry.Call("set_data", [aDesc, aTrigger, acolor, aTexture]);
							}
						}
					}

					if (data.GetList("sp_atk") is { } spAtk)
					{
						Log.Info("Sp Atk");
						Log.Info(spAtk);
						// sideStatsContainer
					}

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

	public override void SetDisplay(DisplayType displayType)
	{
		if (displayType == DisplayType.Full)
		{

		}
		else if (displayType == DisplayType.Simplified)
		{

		}
	}
}

public abstract partial class CardDisplay : Control
{
	public DisplayType Display { get; protected set; } = DisplayType.Full;
	public enum DisplayType { Full, Simplified }

	public Tween? currentAnimationTween;

	public virtual void SetData(CompoundTag data, ClientRegistry registry) { }
	public virtual void SetDisplay(DisplayType displayType) { }
}

using System.Linq;
using CardWars.Client.scenes.core.game_session;
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

	public override void SetData(CompoundTag data, ClientInstanceRegistry registry)
	{
		if (PtContainer is not { } ptContainer) return;
		if (PtLabel is not { } ptLabel) return;
		if (HrtContainer is not { } hrtContainer) return;
		if (HrtLabel is not { } hrtLabel) return;
		if (NameLabel is not { } nameLabel) return;
		if (WorldLabel is not { } worldLabel) return;
		if (CardTypeLabel is not { } cardTypeLabel) return;
		if (SideStatsContainer is not { } sideStatsContainer) return;
		if (BtmDivStarTexture is not { } btmDivStarTexture) return;
		if (BtmDivGradTexture is not { } btmDivGradTexture) return;
		if (StatContainer is not { } statContainer) return;
		if (HpLabel is not { } hpLabel) return;
		if (AtkLabel is not { } atkLabel) return;
		if (FlavourTextLabel is not { } flavourTextLabel) return;
		if (AbilitiesContainer is not { } abilitiesContainer) return;

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
		sideStatsContainer.Visible = false;

		var cardType = data.GetString("card_type");
		cardTypeLabel.Text = cardType;
		switch (cardType)
		{
			case "unit":
				statContainer.Visible = true;
				ptContainer.Visible = true;
				abilitiesContainer.Visible = true;
				Size = PortraitSize;

				// Set Abilities
				if (data.GetList("abilities") is { } abilities)
				{
					var abilityItems = new Godot.Collections.Array();
					foreach (var tag in abilities.Items.OfType<CompoundTag>())
					{
						var aTypePointer = tag.GetString("ability_type");
						CompoundTag? aType = registry.Battle?.AbilityTypeDefinitions.Get(
							ResourceId.Parse(aTypePointer)
						);
						if (aType is null) continue;

						var aTypeTex = registry.Client?.Resources.Instantiate<Texture>(ResourceId.Parse(aType.GetString("texture")))
							?? new ImageTexture();

						abilityItems.Add(new Godot.Collections.Dictionary
						{
							["description"] = tag.GetString("description"),
							["ability_type"] = aType.GetString("name"),
							["color"] = new Color(aType.GetString("color")),
							["texture"] = aTypeTex,
						});
					}
					abilitiesContainer.Call("set_items", abilityItems);
				}

				// Set SP Atk
				if (data.GetList("sp_atks") is { } spAtks)
				{
					var spAtkItems = new Godot.Collections.Array<Godot.Collections.Dictionary>();
					sideStatsContainer.Visible = true;
					foreach (var spAtk in spAtks.OfType<CompoundTag>())
					{
						CompoundTag? attackDef;
						if (spAtk.Has("definition"))
						{
							attackDef = spAtk.GetCompound("definition");
						}
						else
						{
							var resourceId = ResourceId.Parse(spAtk.GetString("resource"));
							attackDef = registry.Battle?.AttackDefinitions.Get(resourceId);
						}

						if (attackDef is null) continue;

						var aText = attackDef.GetString("name") ?? "";
						var aTexture = registry.Client?.Resources.Instantiate<Texture>(ResourceId.Parse(attackDef.GetString("texture")))
							?? new ImageTexture();

						spAtkItems.Add(new Godot.Collections.Dictionary()
						{
							["text"] = aText,
							["texture"] = aTexture
						});
					}
					SideStatsContainer.Call("set_items", spAtkItems);
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

	public virtual void SetData(CompoundTag data, ClientInstanceRegistry registry) { }
	public virtual void SetDisplay(DisplayType displayType) { }
}

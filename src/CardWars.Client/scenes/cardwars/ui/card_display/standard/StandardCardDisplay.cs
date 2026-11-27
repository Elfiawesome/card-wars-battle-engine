using System.Linq;
using CardWars.Client.scenes.core.game_session;
using CardWars.Client.scripts.cardwars.registry;
using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;
using Godot;

namespace CardWars.Client.scenes.cardwars.ui.card_display.standard;

public partial class StandardCardDisplay : CardDisplay
{
	[Export] TextureRect? BackgroundArt;
	[Export] TextureRect? MainArt;
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

	private float _idleTime;
	private Vector2 _mouseOffset;

	public override void SetData(CompoundTag data, ClientInstanceRegistry registry)
	{
		if (BackgroundArt is not { } backgroundArt) return;
		if (MainArt is not { } mainArt) return;
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

		var definitions = registry.Client?.GetExtension<BattleClientRuntime>()?.Definitions;

		nameLabel.Text = data.GetString("name");
		worldLabel.Text = data.GetString("world");
		ptLabel.Text = $"{data.GetInt("pt")}";
		hpLabel.Text = $"{data.GetInt("hp")}";
		atkLabel.Text = $"{data.GetInt("atk")}";
		flavourTextLabel.Text = data.GetString("flavour_text");
		hrtLabel.Text = $"{data.GetInt("hrt")}";

		if (data.GetCompound("textures") is CompoundTag textures)
		{
			ResourceId mainResourceId = ResourceId.Parse(textures.GetString("main"));
			if (registry.Client?.Resources.Instantiate<Texture2D>(mainResourceId) is { } mainResource)
				mainArt.Texture = mainResource;

			ResourceId backgroundResourceId = ResourceId.Parse(textures.GetString("background"));
			if (registry.Client?.Resources.Instantiate<Texture2D>(backgroundResourceId) is { } backgroundResource)
				backgroundArt.Texture = backgroundResource;
		}

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
						CompoundTag? aType = definitions?.AbilityTypeDefinitions.Get(
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
							attackDef = definitions?.AttackDefinitions.Get(resourceId);
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
		Display = displayType;
		if (FlavourTextLabel is not { } flavourTextLabel) return;
		if (AbilitiesContainer is not { } abilitiesContainer) return;
		if (Display == DisplayType.Full)
		{
			flavourTextLabel.Visible = true;
			abilitiesContainer.Visible = true;
		}
		else if (Display == DisplayType.Simplified)
		{
			flavourTextLabel.Visible = false;
			abilitiesContainer.Visible = false;
		}
	}

	public override void _Process(double delta)
	{
		if (MainArt is not { } mainArt) return;
		if (Display != DisplayType.Full) return;

		_idleTime += (float)delta;

		var idle = new Vector2(
			Mathf.Sin(_idleTime * 0.6f),
			Mathf.Sin(_idleTime * 0.43f + 1.3f));

		var mouse = Vector2.Zero;
		if (Size.X > 0.0f && Size.Y > 0.0f)
			mouse = (GetLocalMousePosition() / Size).Clamp(Vector2.Zero, Vector2.One) * 2.0f - Vector2.One;

		var weight = 1.0f - Mathf.Exp(-8.0f * (float)delta);
		_mouseOffset = _mouseOffset.Lerp(mouse, weight);

		var shift = idle * mainArt.Size * 0.012f + _mouseOffset * mainArt.Size * 0.02f;

		mainArt.OffsetLeft = shift.X;
		mainArt.OffsetTop = shift.Y;
		mainArt.OffsetRight = shift.X;
		mainArt.OffsetBottom = shift.Y;
	}
}

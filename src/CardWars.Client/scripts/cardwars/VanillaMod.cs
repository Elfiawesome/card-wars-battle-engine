using System.Collections.Generic;
using CardWars.Client.scripts.cardwars.animation;
using CardWars.Client.scripts.cardwars.entity_view;
using CardWars.Client.scripts.cardwars.layout;
using CardWars.Client.scripts.cardwars.packet;
using CardWars.Client.scripts.cardwars.registry;
using CardWars.Client.scripts.core;
using CardWars.Core.Registry;
using CardWars.ModLoader;
using CardWars.Vanilla.Shared;

namespace CardWars.Client.scripts.cardwars;

public class VanillaMod : IClientMod
{
	public string ModName => "Card Wars Vanilla Client";
	public string Version => "0.0.0";

	public BattleViewRegistry BattleViewRegistry { get; } = new();

	public void OnLoad(ClientRegistry registry, IReadOnlyList<ModContentResult> modContents)
	{
		registry.PacketHandlers.Register(new S2C_PlayerJoinedRequestPacketHandler());
		registry.PacketHandlers.Register(new S2C_ConnectionConfirmedPacketHandler());
		registry.PacketHandlers.Register(new S2C_EnterInstancePacketHandler());
		registry.PacketHandlers.Register(new S2C_EnterInstanceRejectedPacketHandler());
		registry.PacketHandlers.Register(new S2C_LeaveInstancePacketHandler());

		// Instances
		registry.Instances.Register(SharedIds.Instances.World, "res://scenes/cardwars/instance/world/world_instance.tscn");
		registry.Instances.Register(SharedIds.Instances.Battle, "res://scenes/cardwars/instance/battle/battle_instance.tscn");

		// UI
		// TODO: By right this shouldve been auto registered, not hard coded 
		registry.UserInterface.Register(SharedIds.UI.StandardCardDisplay, "res://scenes/cardwars/ui/card_display/standard/standard_card_display.tscn");
		registry.UserInterface.Register(SharedIds.UI.StandardCardInspectionPanel, "res://scenes/cardwars/ui/card_inspection_panel/standard/standard_card_inspection_panel.tscn");
		
		registry.UserInterface.Register(SharedIds.UI.LineArrow, "res://scenes/cardwars/ui/battle/line_arrow.tscn");
		registry.UserInterface.Register(SharedIds.UI.TargetHighlight, "res://scenes/cardwars/ui/battle/target_highlight.tscn");

		// Set display
		BattleViewRegistry.DefaultCardDisplay = SharedIds.UI.StandardCardDisplay;
		BattleViewRegistry.DefaultCardInspectionPanel = SharedIds.UI.StandardCardInspectionPanel;

		// --- Battle ---
		// Entities
		BattleViewRegistry.EntityScene.Register(SharedIds.Entities.Battlefield, "res://scenes/cardwars/instance/battle/entities/battlefield_node.tscn");
		BattleViewRegistry.EntityScene.Register(SharedIds.Entities.UnitSlot, "res://scenes/cardwars/instance/battle/entities/unit_slot_node.tscn");
		BattleViewRegistry.EntityScene.Register(SharedIds.Entities.Card, "res://scenes/cardwars/instance/battle/entities/card_3d.tscn");

		// Entity View Handlers: How the node structure/heritance is like
		BattleViewRegistry.EntityViewHandlers.Register(new BattlefieldViewHandler());
		BattleViewRegistry.EntityViewHandlers.Register(new UnitSlotViewHandler());
		BattleViewRegistry.EntityViewHandlers.Register(new CardViewHandler());

		// Layout Handler: Sets default positions, no animation
		BattleViewRegistry.DefaultLayoutId = ResourceId.Vanilla("default");
		BattleViewRegistry.LayoutHandler.Register(ResourceId.Vanilla("default"), new DefaultLayout());
		BattleViewRegistry.LayoutHandler.Register(ResourceId.Vanilla("line"), new LineLayout());

		// Battle Animation
		BattleViewRegistry.Animations.Register<PlayerIntroductionAnimation>(SharedIds.Animations.PlayerIntroduction);
		BattleViewRegistry.Animations.Register<PlayCardAnimation>(SharedIds.Animations.PlayCard);
		BattleViewRegistry.Animations.Register<AttackAnimation>(SharedIds.Animations.Attack);
		BattleViewRegistry.Animations.Register<TakeDamageAnimation>(SharedIds.Animations.TakeDamage);
		BattleViewRegistry.Animations.Register<DeathAnimation>(SharedIds.Animations.Death);

		// See later if i want to use it
		registry.RegisterExtension(BattleViewRegistry);
	}
}
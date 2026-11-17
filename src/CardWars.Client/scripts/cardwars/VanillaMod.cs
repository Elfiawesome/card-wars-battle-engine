using System.Collections.Generic;
using CardWars.Client.scripts.cardwars.animation;
using CardWars.Client.scripts.cardwars.entity_view;
using CardWars.Client.scripts.cardwars.layout;
using CardWars.Client.scripts.cardwars.packet;
using CardWars.Client.scripts.cardwars.registry;
using CardWars.Client.scripts.cardwars.ui.card_inspection_panel;
using CardWars.Client.scripts.core;
using CardWars.Client.scripts.core.storage;
using CardWars.Core.Registry;
using CardWars.ModLoader;
using CardWars.Vanilla.Shared;

namespace CardWars.Client.scripts.cardwars;

// PROVISIONAL / UNCOMPILED (Phase 4): CardWars.Client needs Godot 4.7, absent in the engine
// sandbox, so this file is not compiled/verified here; validate on a Godot-equipped machine in
// Phase 6. Vanilla is now discovered and loaded through the same IModEntry/LoadModEntry<IClientMod>
// mechanism as any third-party mod (no privileged `new VanillaMod()` path in GameSession, R5).
public class VanillaMod : IClientMod
{
	// Unified IModEntry identity (R5): real values, never throw (was NotImplementedException).
	public string Name => "Vanilla Mod";
	public string Version => "0.0.0";

	public BattleRegistry BattleRegistry = new();

	public void OnLoad(ClientRegistry registry, List<ModContentResult> modContents)
	{
		registry.PacketHandlers.Register(new S2C_CustomModPacketHandler());
		registry.PacketHandlers.Register(new S2C_PlayerJoinedRequestPacketHandler());
		registry.PacketHandlers.Register(new S2C_ConnectionConfirmedPacketHandler());
		registry.PacketHandlers.Register(new S2C_EnterInstancePacketHandler());
		registry.PacketHandlers.Register(new S2C_LeaveInstancePacketHandler());
		registry.PacketHandlers.Register(new S2C_BattleBlockBatchHandler());

		// Instances
		registry.Instances.Register(SharedIds.Instances.World, "res://scenes/cardwars/instance/world/world_instance.tscn");
		registry.Instances.Register(SharedIds.Instances.Battle, "res://scenes/cardwars/instance/battle/battle_instance.tscn");

		// UI
		registry.UserInterface.Register(SharedIds.UI.StandardCardDisplay, "res://scenes/cardwars/ui/card_display/standard/standard_card_display.tscn");
		registry.UserInterface.Register(SharedIds.UI.StandardCardInspectionPanel, "res://scenes/cardwars/ui/card_inspection_panel/standard/standard_card_inspection_panel.tscn");
		registry.UserInterface.Register(SharedIds.UI.LineArrow, "res://scenes/cardwars/ui/battle/line_arrow.tscn");
		registry.UserInterface.Register(SharedIds.UI.TargetHighlight, "res://scenes/cardwars/ui/battle/target_highlight.tscn");

		// Card Inspection Panel Handlers (for keywords etc.)
		BattleRegistry.CardInspectionHandlers.Register(ResourceId.Vanilla("attack"), new CardInspectionAttackHandler());
		BattleRegistry.CardInspectionHandlers.Register(ResourceId.Vanilla("abilities"), new CardInspectionAbilitiesHandler());
		BattleRegistry.CardInspectionHandlers.Register(ResourceId.Vanilla("keywords"), new CardInspectionKeywordHandler());
		BattleRegistry.CardInspectionHandlerOrder.Add(ResourceId.Vanilla("attack"));
		BattleRegistry.CardInspectionHandlerOrder.Add(ResourceId.Vanilla("abilities"));
		BattleRegistry.CardInspectionHandlerOrder.Add(ResourceId.Vanilla("keywords"));

		// Register all content in res://
		var GDContentLoader = new GDContentLoader("cardwars");
		foreach (var (resourceId, resourcePath) in GDContentLoader.LoadAssets())
		{
			// Route through the owning session's logger (exposed on ClientRegistry, Phase 3/R9);
			// it is the immutable no-op NullLogger.Instance when no session logger was supplied
			// (R1). PROVISIONAL / UNCOMPILED: CardWars.Client needs Godot 4.7, absent here;
			// validate in Phase 6.
			registry.Logger.Info("Registered Resource: " + resourceId);
			registry.Resources.Register(resourceId, resourcePath);
		}

		// --- Battle ---
		// Entities
		BattleRegistry.EntityScene.Register(SharedIds.Entities.Battlefield, "res://scenes/cardwars/instance/battle/entities/battlefield_node.tscn");
		BattleRegistry.EntityScene.Register(SharedIds.Entities.UnitSlot, "res://scenes/cardwars/instance/battle/entities/unit_slot_node.tscn");
		BattleRegistry.EntityScene.Register(SharedIds.Entities.Card, "res://scenes/cardwars/instance/battle/entities/card_3d.tscn");

		// Entity View Handlers: How the node structure/heritance is like
		BattleRegistry.EntityViewHandlers.Register(new BattlefieldViewHandler());
		BattleRegistry.EntityViewHandlers.Register(new UnitSlotViewHandler());
		BattleRegistry.EntityViewHandlers.Register(new CardViewHandler());

		// Layout Handler: Sets default positions, no animation
		BattleRegistry.DefaultLayoutId = ResourceId.Vanilla("default");
		BattleRegistry.LayoutHandler.Register(ResourceId.Vanilla("default"), new DefaultLayout());
		BattleRegistry.LayoutHandler.Register(ResourceId.Vanilla("line"), new LineLayout());

		// Battle Animation
		BattleRegistry.Animations.Register<PlayerIntroductionAnimation>(SharedIds.Animations.PlayerIntroduction);
		BattleRegistry.Animations.Register<PlayCardAnimation>(SharedIds.Animations.PlayCard);
		BattleRegistry.Animations.Register<AttackAnimation>(SharedIds.Animations.Attack);
		BattleRegistry.Animations.Register<TakeDamageAnimation>(SharedIds.Animations.TakeDamage);
		BattleRegistry.Animations.Register<DeathAnimation>(SharedIds.Animations.Death);

		// See later if i want to use it
		registry.RegisterExtension(BattleRegistry);
	}
}
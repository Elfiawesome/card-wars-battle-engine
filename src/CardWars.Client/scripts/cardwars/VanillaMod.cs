using System.Collections.Generic;
using CardWars.Client.scripts.cardwars.animation;
using CardWars.Client.scripts.cardwars.entity_view;
using CardWars.Client.scripts.cardwars.layout;
using CardWars.Client.scripts.cardwars.packet;
using CardWars.Client.scripts.cardwars.registry;
using CardWars.Client.scripts.core;
using CardWars.Client.scripts.core.storage;
using CardWars.Core.Logging;
using CardWars.Core.Registry;
using CardWars.ModLoader;
using CardWars.Vanilla.Shared;

namespace CardWars.Client.scripts.cardwars;

public class VanillaMod : IClientMod
{
	public string ModName => throw new System.NotImplementedException();
	public string Version => throw new System.NotImplementedException();

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
		registry.UserInterface.Register(SharedIds.UI.DefaultCardDisplay, "res://scenes/cardwars/instance/battle/ui/card/default_card/default_card_display.tscn");
		registry.UserInterface.Register(SharedIds.UI.LineArrow, "res://scenes/cardwars/instance/battle/ui/line_arrow.tscn");
		registry.UserInterface.Register(SharedIds.UI.TargetHighlight, "res://scenes/cardwars/instance/battle/ui/target_highlight.tscn");

		// Register all content in res://
		var GDContentLoader = new GDContentLoader("cardwars");
		foreach (var (resourceId, resourcePath) in GDContentLoader.LoadAssets())
		{
			Log.Info("Registered Resource: " + resourceId);
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
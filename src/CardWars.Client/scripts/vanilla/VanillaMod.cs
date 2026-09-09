using System.Collections.Generic;
using CardWars.Client.scripts.core;
using CardWars.Client.scripts.vanilla.entity_view;
using CardWars.Client.scripts.vanilla.packet;
using CardWars.Client.scripts.vanilla.registry;
using CardWars.ModLoader;
using CardWars.Vanilla.Shared;

namespace CardWars.Client.scripts.vanilla;

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
		registry.Instances.Register(SharedIds.WorldInstanceId, "res://scenes/vanilla/instance/world/world_instance.tscn");
		registry.Instances.Register(SharedIds.BattleInstanceId, "res://scenes/vanilla/instance/battle/battle_instance.tscn");

		// UI
		registry.UserInterface.Register(SharedIds.CardDisplay, "res://scenes/vanilla/instance/battle/card_display.tscn");

		// --- Battle ---
		// Entities
		BattleRegistry.EntityScene.Register(SharedIds.Battlefield, "res://scenes/vanilla/instance/battle/battlefield.tscn");
		BattleRegistry.EntityScene.Register(SharedIds.UnitSlot, "res://scenes/vanilla/instance/battle/unit_slot.tscn");

		// Entity View Handlers
		BattleRegistry.EntityViewHandlers.Register(new BattlefieldViewHandler());
		BattleRegistry.EntityViewHandlers.Register(new UnitSlotViewHandler());

		// See later if i want to use it
		registry.RegisterExtension(BattleRegistry);
	}
}
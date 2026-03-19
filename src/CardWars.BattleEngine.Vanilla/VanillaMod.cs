using CardWars.BattleEngine.Vanilla.Behaviour;
using CardWars.BattleEngine.Vanilla.Block;
using CardWars.BattleEngine.Vanilla.Features;
using CardWars.Core.Data;
using CardWars.Core.Logging;
using CardWars.Core.Registry;
using CardWars.ModLoader;

namespace CardWars.BattleEngine.Vanilla;

public class VanillaMod : IBattleEngineMod
{
	public string ModName => "Vanilla Mod";
	public string Version => "0.0.0";

	public void OnLoad(BattleEngineRegistry registry, List<ModContentResult> modContents)
	{
		// --- Input Handlers ---
		registry.InputHandlers.Register(new PlayerJoinedRequestInputHandler());
		registry.InputHandlers.Register(new EndTurnRequestInputHandler());
		registry.InputHandlers.Register(new DrawCardRequestInputHandler());
		registry.InputHandlers.Register(new UseCardRequestInputHandler());
		registry.InputHandlers.Register(new AttackRequestInputHandler());

		// --- Event Handlers ---
		registry.EventHandlers.Register(new PlayerJoinedEventHandler());
		registry.EventHandlers.Register(new EndTurnRequestEventHandler());
		registry.EventHandlers.Register(new EndPhaseEventHandler());
		registry.EventHandlers.Register(new EndTurnEventHandler());
		registry.EventHandlers.Register(new DrawCardRequestEventHandler());
		registry.EventHandlers.Register(new DrawCardEventHandler());
		registry.EventHandlers.Register(new UseCardRequestEventHandler());
		registry.EventHandlers.Register(new AttackRequestEventHandler());
		registry.EventHandlers.Register(new TargetUnitEventHandler());
		registry.EventHandlers.Register(new UnitAttackEventHandler());
		registry.EventHandlers.Register(new UnitTakeDamageEventHandler());


		// --- Block Handlers ---
		registry.BlockHandlers.Register(new AttachBattlefieldToPlayerBlockHandler());
		registry.BlockHandlers.Register(new AttachCardToDeckBlockHandler());
		registry.BlockHandlers.Register(new AttachCardToPlayerBlockHandler());
		registry.BlockHandlers.Register(new AttachCardToUnitSlotBlockHandler());
		registry.BlockHandlers.Register(new AttachDeckToPlayerBlockHandler());
		registry.BlockHandlers.Register(new AttachUnitSlotToBattlefieldBlockHandler());
		registry.BlockHandlers.Register(new DetachBattlefieldFromPlayerHandler());
		registry.BlockHandlers.Register(new DetachCardFromDeckBlockHandler());
		registry.BlockHandlers.Register(new DetachCardFromPlayerBlockHandler());
		registry.BlockHandlers.Register(new DetachCardFromUnitSlotBlockHandler());
		registry.BlockHandlers.Register(new DetachDeckFromPlayerBlockHandler());
		registry.BlockHandlers.Register(new DetachUnitSlotFromBattlefieldBlockHandler());
		registry.BlockHandlers.Register(new InstantiateBattlefieldBlockHandler());
		registry.BlockHandlers.Register(new InstantiateCardBlockHandler());
		registry.BlockHandlers.Register(new InstantiateDeckBlockHandler());
		registry.BlockHandlers.Register(new InstantiatePlayerBlockHandler());
		registry.BlockHandlers.Register(new InstantiateUnitSlotBlockHandler());
		registry.BlockHandlers.Register(new ModifyUnitSlotPositionBlockHandler());
		registry.BlockHandlers.Register(new SetCardDataBlockHandler());
		registry.BlockHandlers.Register(new UpdateTurnStateBlockHandler());

		// --- Behaviours ---
		registry.Behaviours.Register<SummonUnitCardToUnitSlotBehaviour>(ResourceId.Vanilla("summon_unit_card_to_unit_slot_behaviour"));

		// --- Card Definitions ---
		RegisterCardDefinitions(registry, modContents);
	}


	private void RegisterCardDefinitions(BattleEngineRegistry registry, List<ModContentResult> modContents)
	{
		foreach (var content in modContents)
		{
			using StreamReader sr = new(content.Stream);

			switch (content.Category)
			{
				case ["cards", "units"]:
					var unitData = sr.ReadToEnd();
					var unitDataTag = DataTagSerializer.Deserialize<CompoundTag>(unitData);
					if (unitDataTag == null) continue;
					Logger.Info("Registered Unit: " + content.Id.ToString());

					// Set basic unit data structure
					unitDataTag.Set("card_type", "unit");
					unitDataTag.Set("intrinsic_behaviours", new ListTag().Add(new CompoundTag().Set("resource", "cardwars:summon_unit_card_to_unit_slot_behaviour")));

					registry.CardDefinitions.Register(content.Id, unitDataTag);

					break;
				case ["cards", "spells"]:
					break;
				case ["cards", "heroes"]:
					var heroData = sr.ReadToEnd();
					var heroDataTag = DataTagSerializer.Deserialize<CompoundTag>(heroData);
					if (heroDataTag == null) continue;
					Logger.Info("Registered Hero: " + content.Id.ToString());

					// Set basic hero data structure
					heroDataTag.Set("card_type", "hero");

					registry.CardDefinitions.Register(content.Id, heroDataTag);
					break;
			}

		}
	}
}

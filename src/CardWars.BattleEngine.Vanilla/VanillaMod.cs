using CardWars.BattleEngine.Vanilla.Behaviour;
using CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Instructions;
using CardWars.BattleEngine.Vanilla.Block;
using CardWars.BattleEngine.Vanilla.Features;
using CardWars.BattleEngine.Vanilla.Targeting;
using CardWars.Core.Data.Mapping;
using CardWars.Core.Data.Tags;
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
		registry.InputHandlers.Register(new AttackHeroRequestInputHandler());

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
		registry.EventHandlers.Register(new AttackHeroRequestEventHandler());
		registry.EventHandlers.Register(new HeroAttackEventHandler());
		registry.EventHandlers.Register(new HeroTakeDamageEventHandler());


		// --- Block Handlers ---
		registry.BlockHandlers.Register(new AttachBattlefieldToPlayerBlockHandler());
		registry.BlockHandlers.Register(new AttachCardToDeckBlockHandler());
		registry.BlockHandlers.Register(new AttachCardToUnitSlotBlockHandler());
		registry.BlockHandlers.Register(new AttachHeroCardToBattlefieldBlockHandler());
		registry.BlockHandlers.Register(new AttachCardToPlayerBlockHandler());
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
		registry.BlockHandlers.Register(new ModifyLayoutBlockHandler());
		registry.BlockHandlers.Register(new ModifyPlayerTeamBlockHandler());
		registry.BlockHandlers.Register(new SetCardDataBlockHandler());
		registry.BlockHandlers.Register(new UpdateTurnStateBlockHandler());

		// --- Behaviours ---
		registry.Behaviours.Register<SummonUnitCardToUnitSlotBehaviour>(ResourceId.Vanilla("summon_unit_card_to_unit_slot_behaviour"));

		// --- Data-Driven Behaviours ---
		RegisterDataDriven(registry);

		// --- Attack Definitions ---
		RegisterAttackDefinitions(registry, modContents);

		// --- Card Definitions ---
		RegisterCardDefinitions(registry, modContents);
	}

	private static void RegisterDataDriven(BattleEngineRegistry registry)
	{
		var dataDriven = registry.DataDriven;

		dataDriven.ExpressionHandlers.Register(new LiteralExpressionHandler());
		dataDriven.ExpressionHandlers.Register(new VariableExpressionHandler());
		dataDriven.ExpressionHandlers.Register(new MemberExpressionHandler());
		dataDriven.ExpressionHandlers.Register(new IndexExpressionHandler());
		dataDriven.ExpressionHandlers.Register(new AddExpressionHandler());
		dataDriven.ExpressionHandlers.Register(new SubtractExpressionHandler());
		dataDriven.ExpressionHandlers.Register(new MultiplyExpressionHandler());
		dataDriven.ExpressionHandlers.Register(new DivideExpressionHandler());
		dataDriven.ExpressionHandlers.Register(new ModuloExpressionHandler());
		dataDriven.ExpressionHandlers.Register(new EqualsExpressionHandler());
		dataDriven.ExpressionHandlers.Register(new NotEqualsExpressionHandler());
		dataDriven.ExpressionHandlers.Register(new LessThanExpressionHandler());
		dataDriven.ExpressionHandlers.Register(new LessThanOrEqualExpressionHandler());
		dataDriven.ExpressionHandlers.Register(new GreaterThanExpressionHandler());
		dataDriven.ExpressionHandlers.Register(new GreaterThanOrEqualExpressionHandler());
		dataDriven.ExpressionHandlers.Register(new AndExpressionHandler());
		dataDriven.ExpressionHandlers.Register(new OrExpressionHandler());
		dataDriven.ExpressionHandlers.Register(new NotExpressionHandler());
		dataDriven.ExpressionHandlers.Register(new NegateExpressionHandler());
		dataDriven.ExpressionHandlers.Register(new CoalesceExpressionHandler());
		dataDriven.ExpressionHandlers.Register(new ConditionalExpressionHandler());
		dataDriven.ExpressionHandlers.Register(new ListExpressionHandler());
		dataDriven.ExpressionHandlers.Register(new ObjectExpressionHandler());
		dataDriven.ExpressionHandlers.Register(new ConstructExpressionHandler());
		dataDriven.ExpressionHandlers.Register(new CallExpressionHandler());
		dataDriven.ExpressionHandlers.Register(new IsEntityTypeExpressionHandler());

		dataDriven.InstructionHandlers.Register(new DeclarationInstructionHandler());
		dataDriven.InstructionHandlers.Register(new SetInstructionHandler());
		dataDriven.InstructionHandlers.Register(new IfInstructionHandler());
		dataDriven.InstructionHandlers.Register(new WhileInstructionHandler());
		dataDriven.InstructionHandlers.Register(new ForInstructionHandler());
		dataDriven.InstructionHandlers.Register(new ForEachInstructionHandler());
		dataDriven.InstructionHandlers.Register(new StageBlockInstructionHandler());
		dataDriven.InstructionHandlers.Register(new StageBlocksInstructionHandler());
		dataDriven.InstructionHandlers.Register(new CommitInstructionHandler());
		dataDriven.InstructionHandlers.Register(new RaiseInstructionHandler());
		dataDriven.InstructionHandlers.Register(new ReturnInstructionHandler());
		dataDriven.InstructionHandlers.Register(new BreakInstructionHandler());
		dataDriven.InstructionHandlers.Register(new ContinueInstructionHandler());
		dataDriven.InstructionHandlers.Register(new WaitInstructionHandler());
		dataDriven.InstructionHandlers.Register(new LogInstructionHandler());
	}


	private static void RegisterAttackDefinitions(BattleEngineRegistry registry, List<ModContentResult> modContents)
	{
		foreach (var content in modContents)
		{
			if (content.Category.Length == 0 || content.Category[0] is not ("attacks" or "sp_atks")) continue;

			var attack = content.ReadAs<CompoundTag>();
			if (attack is null) continue;

			registry.AttackDefinitions.Register(content.Id, attack);
			Log.Info("Registered Attack: " + content.Id.ToString());
		}
	}

	private static void ResolveAttackReferences(BattleEngineRegistry registry, CompoundTag card)
	{
		if (card.Get("ba_atk") is StringTag basicReference)
		{
			if (registry.AttackDefinitions.Get(ResourceId.Parse(basicReference.Value)) is { } basic)
			{
				card.Set("ba_atk", basic.Clone());
			}
			else
			{
				Log.Warn($"Attack definition [{basicReference.Value}] was not found for ba_atk");
			}
		}

		if (card.GetList("sp_atk") is not { } specialAttacks) return;

		var resolved = new ListTag();
		foreach (var item in specialAttacks.Items)
		{
			if (item is StringTag reference)
			{
				if (registry.AttackDefinitions.Get(ResourceId.Parse(reference.Value)) is { } attack)
				{
					resolved.Add(attack.Clone());
				}
				else
				{
					Log.Warn($"Attack definition [{reference.Value}] was not found for sp_atk");
					resolved.Add(item);
				}
			}
			else
			{
				resolved.Add(item);
			}
		}

		card.Set("sp_atk", resolved);
	}

	private void RegisterCardDefinitions(BattleEngineRegistry registry, List<ModContentResult> modContents)
	{
		foreach (var content in modContents)
		{
			switch (content.Category)
			{
				case ["cards", "units"]:
					var unitDataTag = content.ReadAs<CompoundTag>();
					if (unitDataTag is null) continue;
					Log.Info("Registered Unit: " + content.Id.ToString());

					unitDataTag.Set("card_type", "unit");
					unitDataTag.Set("intrinsic_behaviours", new ListTag().Add(new CompoundTag().Set("resource", "cardwars:summon_unit_card_to_unit_slot_behaviour")));
					if (!unitDataTag.Has("target_filter"))
						unitDataTag.Set("target_filter", DataTagMapper.ToTag(UnitTargetFilter.Create()));
					ResolveAttackReferences(registry, unitDataTag);

					registry.CardDefinitions.Register(content.Id, unitDataTag);

					break;
				case ["cards", "spells"]:
					break;
				case ["cards", "heroes"]:
					var heroDataTag = content.ReadAs<CompoundTag>();
					if (heroDataTag is null) continue;
					Log.Info("Registered Hero: " + content.Id.ToString());

					heroDataTag.Set("card_type", "hero");

					registry.CardDefinitions.Register(content.Id, heroDataTag);
					break;
			}
		}
	}
}

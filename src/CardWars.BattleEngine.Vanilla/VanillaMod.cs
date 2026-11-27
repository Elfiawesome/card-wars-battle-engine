using CardWars.Vanilla.Shared;
using CardWars.BattleEngine.Vanilla.Behaviour;
using CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Instructions;
using CardWars.BattleEngine.Vanilla.Block;
using CardWars.BattleEngine.Vanilla.Features;
using CardWars.BattleEngine.Vanilla.Rules;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.BattleEngine.Vanilla.State;
using CardWars.Core.Data;
using CardWars.Core.Data.Tags;
using CardWars.Core.Logging;
using CardWars.Core.Registry;
using CardWars.ModLoader;
using CardWars.BattleEngine.registry;

namespace CardWars.BattleEngine.Vanilla;

public class VanillaMod : IBattleEngineMod
{
	public string ModName => "Vanilla Mod";
	public string Version => "0.0.0";

	public void OnLoad(BattleEngineRegistry registry, IReadOnlyList<ModContentResult> modContents)
	{
		var definitions = registry.GetOrCreateExtension<CardWarsDefinitions>();
		registry.StateInitializer = new CardWarsStateInitializer();
		registry.GameOverPolicy = new CardWarsGameOverPolicy();
		registry.InputPolicy = new CardWarsInputPolicy();
		registry.EntityBehaviourProviders.Register(typeof(GenericCard), new CardWarsBehaviourProvider(registry.DataTags));

		// --- Input Handlers ---
		registry.InputHandlers.Register(new PlayerJoinedRequestInputHandler());
		registry.InputHandlers.Register(new PlayerLeftRequestInputHandler());
		registry.InputHandlers.Register(new EndTurnRequestInputHandler());
		registry.InputHandlers.Register(new DrawCardRequestInputHandler());
		registry.InputHandlers.Register(new UseCardRequestInputHandler());
		registry.InputHandlers.Register(new AttackRequestInputHandler());
		registry.InputHandlers.Register(new AttackHeroRequestInputHandler());

		// --- Event Handlers ---
		registry.EventHandlers.Register(new PlayerJoinedEventHandler());
		registry.EventHandlers.Register(new TurnEndedEventHandler());
		registry.EventHandlers.Register(new PhaseEndedEventHandler());
		registry.EventHandlers.Register(new RoundEndedEventHandler());
		registry.EventHandlers.Register(new UnitDiedEventHandler());
		registry.EventHandlers.Register(new HeroDiedEventHandler());
		registry.EventHandlers.Register(new PlayerDefeatedEventHandler());
		registry.EventHandlers.Register(new WinConditionCheckEventHandler());
		registry.EventHandlers.Register(new GameEndedEventHandler());
		registry.EventHandlers.Register(new ChargeRefillEventHandler());
		registry.EventHandlers.Register(new PtReplenishEventHandler());
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
		registry.BlockHandlers.Register(new AttachCardToGraveyardBlockHandler());
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
		registry.BlockHandlers.Register(new DetachHeroCardFromBattlefieldBlockHandler());
		registry.BlockHandlers.Register(new DetachUnitSlotFromBattlefieldBlockHandler());
		registry.BlockHandlers.Register(new InstantiateBattlefieldBlockHandler());
		registry.BlockHandlers.Register(new InstantiateCardBlockHandler());
		registry.BlockHandlers.Register(new InstantiateDeckBlockHandler());
		registry.BlockHandlers.Register(new InstantiatePlayerBlockHandler());
		registry.BlockHandlers.Register(new InstantiateUnitSlotBlockHandler());
		registry.BlockHandlers.Register(new ModifyUnitSlotPositionBlockHandler());
		registry.BlockHandlers.Register(new MarkHeroDefeatedBlockHandler());
		registry.BlockHandlers.Register(new MarkPlayerDefeatedBlockHandler());
		registry.BlockHandlers.Register(new ModifyLayoutBlockHandler());
		registry.BlockHandlers.Register(new ModifyPlayerPtBlockHandler());
		registry.BlockHandlers.Register(new ModifyPlayerTeamBlockHandler());
		registry.BlockHandlers.Register(new RemoveEntityBlockHandler());
		registry.BlockHandlers.Register(new SetCardDataBlockHandler());
		registry.BlockHandlers.Register(new SetGameOutcomeBlockHandler());
		registry.BlockHandlers.Register(new SetGameRuleBlockHandler());
		registry.BlockHandlers.Register(new UpdateTurnStateBlockHandler());

		// --- Behaviours ---
		registry.Behaviours.Register<SummonUnitCardToUnitSlotBehaviour>(CardWarsIds.Vanilla("summon_unit_card_to_unit_slot_behaviour"));
		registry.Behaviours.Register<TestAbility>(CardWarsIds.Vanilla("test_ability"));

		// --- Hero Lose Conditions ---
		registry.EntityConditions.Register<HeroHrtLoseCondition>(HeroHrtLoseCondition.Id);

		// --- Data-Driven Behaviours ---
		RegisterDataDriven(registry);

		// --- Game Rules ---
		RegisterGameRules(definitions, modContents, registry.DataTags, registry.Logger);

		// --- Attack Definitions ---
		RegisterAttackDefinitions(definitions, modContents, registry.DataTags, registry.Logger);

		// --- Register Ability Types ---
		RegisterAbilityTypes(definitions, modContents, registry.DataTags, registry.Logger);

		// --- Card Definitions ---
		RegisterCardDefinitions(definitions, modContents, registry.DataTags, registry.Logger);
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


	private static void RegisterGameRules(CardWarsDefinitions definitions, IReadOnlyList<ModContentResult> modContents, DataTagContext dataTags, Logger logger)
	{
		foreach (var content in modContents)
		{
			if (content.Category.Length == 0 || content.Category[0] != "rules") continue;

			var rules = content.ReadAs<CompoundTag>(dataTags);
			if (rules is null) continue;

			definitions.RuleDefinitions.Register(content.Id, rules);
			if (content.Id == GameRulesDefinition.DefaultId)
			{
				try
				{
					definitions.DefaultUnitTargetFilter = dataTags.FromTag<GameRulesDefinition>(rules).UnitTargetFilter;
				}
				catch (Exception ex)
				{
					logger.Warn($"Could not parse default unit target filter from [{content.Id}]: {ex.Message}");
				}
			}

			logger.Info($"Registered Game Rules: {content.Id}");
		}
	}

	private static void RegisterAttackDefinitions(CardWarsDefinitions definitions, IReadOnlyList<ModContentResult> modContents, DataTagContext dataTags, Logger logger)
	{
		foreach (var content in modContents)
		{
			if (content.Category.Length == 0 || content.Category[0] is not ("attacks" or "sp_atks")) continue;

			var attack = content.ReadAs<CompoundTag>(dataTags);
			if (attack is null) continue;

			definitions.AttackDefinitions.Register(content.Id, attack);
			logger.Info($"Registered Attack: {content.Id}");
		}
	}

	public static void RegisterAbilityTypes(CardWarsDefinitions definitions, IReadOnlyList<ModContentResult> modContents, DataTagContext dataTags, Logger logger)
	{
		foreach (var content in modContents)
		{
			if (content.Category.Length == 1 && content.Category[0] == "ability_types")
			{
				var abilityType = content.ReadAs<CompoundTag>(dataTags);
				if (abilityType is null) continue;
				definitions.AbilityTypeDefinitions.Register(content.Id, abilityType);
				logger.Info($"Registered Ability Type: {content.Id}");
			}
		}
	}

	private static void RegisterCardDefinitions(CardWarsDefinitions definitions, IReadOnlyList<ModContentResult> modContents, DataTagContext dataTags, Logger logger)
	{
		foreach (var content in modContents)
		{
			switch (content.Category)
			{
				case ["cards", "units", ..]:
					var unitDataTag = content.ReadAs<CompoundTag>(dataTags);
					if (unitDataTag is null) continue;

					unitDataTag.Set("card_type", "unit");
					// Set unit card's intrinsic behaviour
					unitDataTag.Set("intrinsic_behaviours", new ListTag().Add(new CompoundTag().Set("resource", "cardwars:summon_unit_card_to_unit_slot_behaviour")));
					// Set unit card's default unit targeting
					if (!unitDataTag.Has("target_filter") && definitions.DefaultUnitTargetFilter is { } targetFilter)
						unitDataTag.Set("target_filter", dataTags.ToTag(targetFilter));

					// Set unit card's abilities
					// var abilities = unitDataTag.GetList("abilities");
					// if (abilities is ListTag { })
					// {
					// 	foreach (var a in abilities)
					// 	{
					// 		if (a is CompoundTag ability)
					// 		{
					// 			var abilityType = ability.Get<StringTag>("ability_type")?.Value ?? "";
					// 			var abilityTypeId = registry.AbilityTypeDefinitions.Get(ResourceId.Parse(abilityType));
					// 			if (abilityTypeId is { })
					// 			{
					// 				ability.Remove("ability_type");
					// 				ability.Set("ability_type", abilityTypeId.GetString("name", ""));
					// 				ability.Set("color", abilityTypeId.GetString("color", ""));
					// 				ability.Set("texture", abilityTypeId.GetString("texture", ""));
					// 			}
					// 		}
					// 	}
					// }

					definitions.CardDefinitions.Register(content.Id, unitDataTag);
					logger.Info($"Registered Unit Card: {content.Id}");
					break;
				case ["cards", "spells", ..]:
					var spellDataTag = content.ReadAs<CompoundTag>(dataTags);
					if (spellDataTag is null) continue;

					spellDataTag.Set("card_type", "spell");
					definitions.CardDefinitions.Register(content.Id, spellDataTag);
					logger.Info($"Registered Spell Card: {content.Id}");
					break;
				case ["cards", "heroes", ..]:
					var heroDataTag = content.ReadAs<CompoundTag>(dataTags);
					if (heroDataTag is null) continue;

					heroDataTag.Set("card_type", "hero");
					if (!heroDataTag.Has("lose_condition"))
						heroDataTag.Set("lose_condition", dataTags.ToTag(new ContentPointer(Resource: HeroHrtLoseCondition.Id)));

					definitions.CardDefinitions.Register(content.Id, heroDataTag);
					logger.Info($"Registered Hero Card: {content.Id}");
					break;
			}
		}
	}
}

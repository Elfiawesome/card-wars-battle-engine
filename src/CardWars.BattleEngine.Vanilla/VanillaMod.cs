using CardWars.BattleEngine.Rules;
using CardWars.BattleEngine.Vanilla.Behaviour;
using CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Instructions;
using CardWars.BattleEngine.Vanilla.Block;
using CardWars.BattleEngine.Vanilla.Features;
using CardWars.BattleEngine.Vanilla.Rules;
using CardWars.BattleEngine.Vanilla.Targeting;
using CardWars.Core.Data;
using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;
using CardWars.ModLoader;

namespace CardWars.BattleEngine.Vanilla;

public class VanillaMod : IBattleEngineMod
{
	public string Name => "Vanilla Mod";
	public string Version => "0.0.0";

	public void OnLoad(BattleEngineRegistry registry, DataContext data, List<ModContentResult> modContents)
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
		registry.Behaviours.Register<SummonUnitCardToUnitSlotBehaviour>(ResourceId.Vanilla("summon_unit_card_to_unit_slot_behaviour"));
		registry.Behaviours.Register<TestAbility>(ResourceId.Vanilla("test_ability"));

		// --- Hero Lose Conditions ---
		registry.EntityConditions.Register<HeroHrtLoseCondition>(HeroHrtLoseCondition.Id);

		// --- Data-Driven Behaviours ---
		RegisterDataDriven(registry);

		// --- Game Rules ---
		RegisterGameRules(registry, modContents);

		// --- Attack Definitions ---
		RegisterAttackDefinitions(registry, modContents);

		// --- Register Ability Types ---
		RegisterAbilityTypes(registry, modContents);

		// --- Card Definitions ---
		var cardDefaults = ReadCardDefaults(modContents);
		RegisterCardDefinitions(registry, cardDefaults, modContents);
	}

	// Reads the data-driven card-type default templates (content/server/card_defaults/*.json,
	// Category ['card_defaults']) into a local map keyed by the template's category name
	// (unit/hero/spell). The templates carry the defaults that used to be baked into C# switch
	// arms (Finding 7): card_type, intrinsic_behaviours, target_filter, lose_condition. Loaded
	// through the SAME ModContentResult.ReadAs<CompoundTag>() path as rules/attacks/ability_types.
	// No static/global state (R1): the map is returned and passed as a local into
	// RegisterCardDefinitions. These templates are NOT registered as card definitions.
	private static Dictionary<string, CompoundTag> ReadCardDefaults(List<ModContentResult> modContents)
	{
		var defaults = new Dictionary<string, CompoundTag>();
		foreach (var content in modContents)
		{
			if (content.Category is not ["card_defaults"]) continue;

			var template = content.ReadAs<CompoundTag>();
			if (template is null) continue;

			// Key by the file stem (unit/hero/spell) — the last id segment.
			var idText = content.Id.ToString();
			var key = idText[(idText.LastIndexOf('/') + 1)..];
			defaults[key] = template;
		}
		return defaults;
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


	private static void RegisterGameRules(BattleEngineRegistry registry, List<ModContentResult> modContents)
	{
		foreach (var content in modContents)
		{
			if (content.Category.Length == 0 || content.Category[0] != "rules") continue;

			var rules = content.ReadAs<CompoundTag>();
			if (rules is null) continue;

			registry.RuleDefinitions.Register(content.Id, rules);
			registry.Logger.Info($"Registered Game Rules: {content.Id}");
		}
	}

	private static void RegisterAttackDefinitions(BattleEngineRegistry registry, List<ModContentResult> modContents)
	{
		foreach (var content in modContents)
		{
			if (content.Category.Length == 0 || content.Category[0] is not ("attacks" or "sp_atks")) continue;

			var attack = content.ReadAs<CompoundTag>();
			if (attack is null) continue;

			registry.AttackDefinitions.Register(content.Id, attack);
			registry.Logger.Info($"Registered Attack: {content.Id}");
		}
	}

	public static void RegisterAbilityTypes(BattleEngineRegistry registry, List<ModContentResult> modContents)
	{
		foreach (var content in modContents)
		{
			if (content.Category.Length == 1 && content.Category[0] == "ability_types")
			{
				var abilityType = content.ReadAs<CompoundTag>();
				if (abilityType is null) continue;
				registry.AbilityTypeDefinitions.Register(content.Id, abilityType);
				registry.Logger.Info($"Registered Ability Type: {content.Id}");
			}
		}
	}

	// Data-driven card-type defaults (Finding 7): the per-category defaults (card_type,
	// intrinsic_behaviours, target_filter, lose_condition) now live in JSON templates under
	// content/server/card_defaults/*.json and are passed in via `cardDefaults` keyed by category
	// name (unit/spell/hero). Each template field is merged into the card tag fill-if-absent.
	// This is behavior-identical to the former hardcoded C# switch arms: no vanilla card JSON
	// defines card_type or intrinsic_behaviours today, so filling them only when absent produces
	// the same tags the unconditional Set calls did; target_filter and lose_condition were already
	// fill-if-absent. No DataContext is needed any more here because no tag is baked in C#.
	private static void RegisterCardDefinitions(BattleEngineRegistry registry, Dictionary<string, CompoundTag> cardDefaults, List<ModContentResult> modContents)
	{
		foreach (var content in modContents)
		{
			switch (content.Category)
			{
				case ["cards", "units"]:
					var unitDataTag = content.ReadAs<CompoundTag>();
					if (unitDataTag is null) continue;

					ApplyCardDefaults(unitDataTag, cardDefaults, "unit");

					registry.CardDefinitions.Register(content.Id, unitDataTag);
					registry.Logger.Info($"Registered Unit Card: {content.Id}");
					break;
				case ["cards", "spells"]:
					var spellDataTag = content.ReadAs<CompoundTag>();
					if (spellDataTag is null) continue;

					ApplyCardDefaults(spellDataTag, cardDefaults, "spell");

					registry.CardDefinitions.Register(content.Id, spellDataTag);
					registry.Logger.Info($"Registered Spell Card: {content.Id}");
					break;
				case ["cards", "heroes"]:
					var heroDataTag = content.ReadAs<CompoundTag>();
					if (heroDataTag is null) continue;

					ApplyCardDefaults(heroDataTag, cardDefaults, "hero");

					registry.CardDefinitions.Register(content.Id, heroDataTag);
					registry.Logger.Info($"Registered Hero Card: {content.Id}");
					break;
			}
		}
	}

	// Fill-if-absent merge of a card-type default template into a loaded card tag. Each top-level
	// field from the template is copied (cloned) onto the card only when the card does not already
	// define that field, so card JSON always wins over the default template.
	private static void ApplyCardDefaults(CompoundTag card, Dictionary<string, CompoundTag> cardDefaults, string category)
	{
		if (!cardDefaults.TryGetValue(category, out var template)) return;

		foreach (var (key, value) in template.Entries)
		{
			if (!card.Has(key))
				card.Set(key, value.Clone());
		}
	}
}

# 06. Vanilla Battle

How the vanilla card game is built on the engine. Rules here are the legacy rules as implemented in `src/CardWars.BattleEngine.Vanilla` and pinned by the legacy tests, unless marked **changed** or listed under open questions.

Where code lives:

- `CardWars.Vanilla.Common`: content schemas, component types, entity types, cue types, legal action kinds, player action types and their codecs. Everything the client needs to decode what it receives.
- `CardWars.Vanilla.Server`: setup handler, action handlers, event types and apply handlers, behaviour providers, effects, script functions, visibility rule, win and lose evaluation.

## 1. Content types

All common and synced unless noted. Paths are under `vanilla/common/content/vanilla/`.

### 1.1 Cards (`cards/`)

```json
{
	"kind": "unit",
	"name": "Apocalypse Tank",
	"world": "Red Alert 3",
	"flavour_text": "",
	"cost": 1,
	"stats": { "hp": 2, "atk": 3, "charge": 1 },
	"attacks": { "special": [ "cross" ] },
	"abilities": [
		{ "description": "Deal 50% more damage on front row units", "ability_type": "attack", "behaviour": { "type": "script", "on": "attack_declared", "do": [] } }
	],
	"textures": { "main": "texture/cards/units/redalert3/apocalypse_tank", "background": "texture/cards/worlds/redalert3" }
}
```

```csharp
public sealed record CardDefinition(
	CardKind Kind,                          // unit, spell, hero (explicit; legacy inferred it from the folder)
	string Name,
	string World,
	string FlavourText,
	int Cost,                               // legacy "pt"
	UnitStats? Stats,                       // required for units
	int? HeroHealth,                        // required for heroes; legacy "hrt"
	CardAttacks Attacks,                    // basic attack holder and special attacks
	ImmutableArray<AbilityDefinition> Abilities,
	TargetFilter? PlayTarget,               // defaults by kind (section 6.1)
	LoseCondition? LoseCondition,           // heroes; defaults to hero health <= 0
	CardTextures Textures);                 // asset ids, resolved on the client
```

- Max values (`hp_max`, `atk_max`, `charge_max`) are not written into content. They are the definition's stats; the runtime stats component holds current values. **Changed:** legacy wrote defaults back into the shared definition at join time.
- The legacy apocalypse tank's `test_ability` (sets HP 1000 and ATK 2000 every turn end) is debug behaviour and is not carried over.

### 1.2 Other content

| Folder | Type | Contents |
|---|---|---|
| `attacks/` | `AttackDefinition` | `name`, `icon`, `charge_cost`, `bypass`, `patterns` (each `shape` of `cell`/`row`/`column`/`all`, `offset`, `damage { percent, amount }`) |
| `ability_types/` | `AbilityTypeDefinition` | `name`, `color`, `icon`, `text`. Display data for the client; it has no rules effect |
| `decks/` | `DeckDefinition` | `heroes: [card ids]`, `cards: [card ids]`. Replaces the hard-coded starter deck |
| `battlefield_layouts/` | `BattlefieldLayoutDefinition` | Slot positions, for example three front slots at `y = 0`, `x = -1..1` and one back slot at `(0, -1)`. Replaces the hard-coded layout |
| `battle_rules/` | `BattleRulesDefinition` | `pt_schedule`, `pt_cap`, `win_condition` script, default battlefield layout, turn structure options |
| `tags/card/` | content tags | groups of cards (02-ids-registries.md, section 6) |

Client-only content (`vanilla/client/content/vanilla/`): `battle_layouts/` (how battlefields are arranged on screen: gap, curvature, facing), `cues/` (animation timing per cue), `card_skins/`.

## 2. Entity types

| Type | Purpose |
|---|---|
| `vanilla:battle` | The root entity. Holds turn state and the rules in force |
| `vanilla:seat` | One participant. Team, points, defeated flag |
| `vanilla:battlefield` | One battlefield owned by a seat |
| `vanilla:slot` | A unit slot on a battlefield, with a grid position |
| `vanilla:card` | Any card: in deck, hand, slot, hero row, or graveyard |

The legacy `Deck` entity is replaced by the deck zone (section 3); a seat's deck is the ordered set of its cards in that zone.

## 3. Components

| Component | Type | On | Visibility |
|---|---|---|---|
| `battle:seat` | `SeatInfo(ParticipantId)` | seat | public |
| `vanilla:team` | `int` | seat | public |
| `vanilla:points` | `int` (legacy "pt") | seat | public |
| `vanilla:defeated` | marker | seat, card (heroes) | public |
| `vanilla:turn` | `TurnState(order, index, turn number, round number, phase)` | root | public |
| `vanilla:rules` | `Key<BattleRulesDefinition>` | root | public |
| `vanilla:owner` | `EntityId` of the seat | battlefield, card | public |
| `vanilla:slot_position` | `(x, y)` | slot | public |
| `vanilla:location` | `Location(container, zone, order)` | card | public, except `order` in hidden zones |
| `vanilla:card` | `Key<CardDefinition>` | card | see 3.1 |
| `vanilla:unit_stats` | `UnitStats(hp, atk, charge)` current values | card | see 3.1 |
| `vanilla:hero_health` | `int` current | card | see 3.1 |

### 3.1 Zones and visibility

`Location.zone` is one of `deck`, `hand`, `slot`, `hero`, `graveyard`. `container` is the seat (for deck, hand, graveyard), the slot (for slot), or the battlefield (for hero).

This single component replaces the legacy pairs (`Player.HandCardIds` plus `GenericCard.OwnerPlayerId`, `UnitSlot.HoldingCardId` plus `GenericCard.OwnerUnitSlotId`, and so on). "The card in this slot" is a query over the location index.

Visibility rule (vanilla's `IVisibilityRule`):

| Zone | Owner sees | Others see |
|---|---|---|
| `deck` | entity, owner, location without order | entity, owner, location without order |
| `hand` | everything | entity, owner, location |
| `slot`, `hero`, `graveyard` | everything | everything |

So nobody, including the owner, sees deck contents or order, and opponents see how many cards are in a hand but not which.

When cards are shuffled into a deck, they are destroyed and recreated with new ids, so a card cannot be tracked through the deck (05-battle.md, section 10.4).

## 4. Setup

The vanilla setup handler receives:

```csharp
public sealed record VanillaBattleSetup(
	Key<BattleRulesDefinition> Rules,
	ImmutableArray<VanillaParticipant> Participants);

public sealed record VanillaParticipant(ParticipantId Participant, string Name, int Team, Key<DeckDefinition> Deck);
```

It creates the root entity, one seat per participant (in setup order, which is the turn order), one battlefield per seat with slots from the rules' battlefield layout, hero cards in the `hero` zone, the deck cards in the `deck` zone in shuffled order, and gives each seat the points for round 1.

The server builds this from the encounter and each player's profile. Until profiles have decks, the server uses `vanilla:starter` from content.

## 5. Turn structure

Same as legacy:

- `TurnState` has a turn order (seats), the index of the acting seat, turn number, round number, and phase (`setup` or `attacking`).
- Only the acting seat may act, except for answering prompts.
- Ending a turn advances the index. When it wraps, the phase flips. When it flips from `attacking` to `setup`, the round ends (section 7) and the round number increases.

## 6. Player actions

| Action | When | Checks | Legal option shape |
|---|---|---|---|
| `vanilla:end_turn` | acting seat | none | `{}` |
| `vanilla:draw_card` | acting seat | deck not empty | `{}` |
| `vanilla:play_card` | acting seat, setup phase | card in own hand; cost <= seat points; target passes the card's play target filter | `{ card, targets: [...] }` |
| `vanilla:attack` | acting seat, attacking phase | attacker is own unit in a slot; charge >= attack cost; target slot is an enemy unit; target not protected unless the attack bypasses | `{ attacker, targets: [...] }` |
| `vanilla:attack_hero` | acting seat, attacking phase | as `attack`, and the target hero's battlefield has no living units | `{ attacker, targets: [...] }` |

**Changed:** legacy `draw_card` named a deck and a receiving player, which let the caller choose any deck. The rewrite draws from the acting seat's own deck. A mod that wants "draw from another deck" adds an effect for it.

**Changed:** legacy attacks took a list of attacker slots in one input. The rewrite takes one attacker per action. The client can still send several in a row, and the server validates each against the then-current state.

### 6.1 Default play targets

- Unit: an empty slot on one of the seat's own battlefields (legacy used only the first battlefield).
- Spell, hero: none until spells and hero plays are designed.

## 7. Events, attacks, round end

### 7.1 Attacks

1. `vanilla:attack` raises `AttackDeclared(attacker, target slot)`. Before listeners may cancel.
2. Apply: spend charge, then for each slot on the target's battlefield that holds a unit and matches the attack's pattern relative to the target slot, raise `UnitDamaged(target, source, amount)`. Across the patterns that match a slot, take the highest `percent` and the highest `amount` separately; the damage is `max(round(atk * percent), amount)`.
3. `UnitDamaged` apply: subtract from current HP. Units are not removed yet.

Attack resolution (`AttackProfile` in legacy):

- With special attacks: patterns are concatenated, charge costs summed, bypass if any bypasses, name joined with `+`.
- Otherwise the card's basic attack, defaulting to a single-cell 100% attack.

Protection: a target slot is protected if another slot on the same battlefield with a greater `y` holds a unit with HP above 0. Bypass ignores protection.

Hero attacks: allowed only when every slot on the hero's battlefield is empty or holds a unit with HP 0 or less. Hero damage is `ceil(damage / 10)` hero health.

### 7.2 Round end

In this order, as legacy:

1. Units with HP 0 or less move to their owner's graveyard; `UnitDied` per unit (with a `death` cue for the batch).
2. For each undefeated seat, each hero whose lose condition holds is marked defeated; `HeroDefeated`.
3. A seat whose heroes are all defeated is marked defeated; `SeatDefeated`. Its battlefields, slots, and heroes are removed.
4. Win check (section 8).
5. Units on battlefields refill charge to their definition's charge.
6. Defeated seats leave the turn order; the first remaining seat acts.
7. Each undefeated seat is topped up to the points for the new round: `points = max(points, min(pt_schedule[round - 1], pt_cap))`, with the last schedule entry reused for later rounds.

## 8. Win and lose conditions

- Each team is evaluated with the rules' `win_condition` script (signature `vanilla:win_condition`). Default: the team is alive and exactly one team is alive.
- Exactly one winning team: `Ended(Win(team))`. More than one, or no team alive: `Ended(Draw)`.
- Hero lose condition: the card's `lose_condition` script (signature `vanilla:hero_lose_condition`) or the default `hero_health <= 0`. The legacy C# `HeroHrtLoseCondition` becomes the default script, so there is one mechanism.

## 9. Effects and functions for scripts

Initial set, extended as abilities are written:

| Effect | Raises |
|---|---|
| `vanilla:deal_damage(target, amount)` | `UnitDamaged` or `HeroDamaged` |
| `vanilla:heal(target, amount)` | `UnitHealed` |
| `vanilla:modify_stats(target, hp, atk, charge)` | `StatsModified` |
| `vanilla:draw_cards(seat, count)` | `CardDrawn` per card |
| `vanilla:give_points(seat, amount)` | `PointsChanged` |

Functions: `units_on(battlefield)`, `slot_at(battlefield, x, y)`, `row_of(slot)`, `enemies_of(seat)`, `card_in(slot)`, `random_int(min, max)`, `in_tag(card, tag)`.

## 10. Cues

`vanilla:player_intro`, `vanilla:play_card`, `vanilla:attack`, `vanilla:take_damage`, `vanilla:death`, `vanilla:draw`. Each has a small record with a codec (entity ids only). Client animations map from these.

## 11. Parity tests

The legacy simulation tests (`src/CardWars.Tests/Simulation`, `Vanilla`) are rewritten against the new API with the same scenarios: setup, points, rounds, attacks, special attacks, hero attacks, win conditions, target filters, attack definitions, hero lose conditions. Rules marked **changed** above get new tests instead.

## Open questions

1. **Draw rule.** Legacy lets the acting seat draw any number of cards at any time in its turn (the client binds it to a key). Is drawing free and unlimited, once per turn, or automatic at turn start?
2. **Multi-attacker attacks.** Is there a rule that several units attacking one target together matters (combined damage, one animation)? If so, `attack` should keep taking several attackers.
3. **Deck shuffle.** Legacy does not shuffle; the deck order is the starter list order. Shuffle at setup (proposed)?
4. **Spells and hero plays.** Not implemented in legacy beyond the card kind. Out of scope for parity; confirm.

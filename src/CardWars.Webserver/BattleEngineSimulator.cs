using System.Runtime.CompilerServices;
using CardWars.BattleEngine;
using CardWars.BattleEngine.Input;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.BattleEngine.Vanilla.Features;
using CardWars.Core.Logging;
using VanillaMod = CardWars.BattleEngine.Vanilla.VanillaMod;

namespace CardWars.Webserver;

public class Simulator
{
	public BattleEngine.BattleEngine Engine { get; }
	public GameState State => Engine.State;

	private readonly Dictionary<string, EntityId> _aliases = new();
	private int _stepCount;

	public Simulator()
	{
		Engine = new();
		Engine.LoadMod(new VanillaMod());
	}

	// ═══════════════════════════════════════
	//  Aliases – refer to entities by name
	// ═══════════════════════════════════════

	public EntityId Id(string alias)
	{
		if (_aliases.TryGetValue(alias, out var id)) return id;
		throw new InvalidOperationException(
			$"Unknown alias '{alias}'. Register it via AddPlayer() or Alias().");
	}

	public void Alias(string name, EntityId id) => _aliases[name] = id;

	// ═══════════════════════════════════════
	//  Steps – named, logged actions
	// ═══════════════════════════════════════

	public void Step(string description, Action action)
	{
		_stepCount++;
		Logger.Info($"┌─ Step {_stepCount}: {description}");
		try
		{
			action();
			Logger.Info("└─ ✓ OK");
		}
		catch (Exception ex)
		{
			Logger.Error($"└─ ✗ FAILED: {ex.Message}");
			throw;
		}
	}

	public T Step<T>(string description, Func<T> action)
	{
		_stepCount++;
		Logger.Info($"┌─ Step {_stepCount}: {description}");
		try
		{
			var result = action();
			Logger.Info($"└─ ✓ → {result}");
			return result;
		}
		catch (Exception ex)
		{
			Logger.Error($"└─ ✗ FAILED: {ex.Message}");
			throw;
		}
	}

	// ═══════════════════════════════════════
	//  Generic Input (escape hatch)
	// ═══════════════════════════════════════

	/// <summary>Send any IInput as a system-level action (no player check).</summary>
	public void SendInput(IInput input)
		=> Engine.HandleInput(EntityId.None, input);

	/// <summary>Send any IInput on behalf of a specific player.</summary>
	public void SendInput(EntityId playerId, IInput input)
		=> Engine.HandleInput(playerId, input);

	/// <summary>Send any IInput on behalf of a named player.</summary>
	public void SendInput(string playerAlias, IInput input)
		=> SendInput(Id(playerAlias), input);

	// ═══════════════════════════════════════
	//  Convenience Actions
	// ═══════════════════════════════════════

	public EntityId AddPlayer(string alias)
	{
		var playerId = EntityId.New();
		Alias(alias, playerId);
		SendInput(new PlayerJoinedRequestInput(playerId));
		return playerId;
	}

	public void DrawCard(string playerAlias, int deckIndex = 0)
	{
		var playerId = Id(playerAlias);
		var deck = Decks(playerAlias).ElementAtOrDefault(deckIndex)
			?? throw new InvalidOperationException(
				$"'{playerAlias}' has no deck at index {deckIndex}");
		SendInput(playerId, new DrawCardRequestInput
		{
			DeckId = deck.Id,
			ReceivedPlayerId = playerId
		});
	}

	public void PlayCardToSlot(string playerAlias, int handIndex,
		UnitSlotPos position, int battlefieldIndex = 0)
	{
		var playerId = Id(playerAlias);
		var card = Hand(playerAlias).ElementAtOrDefault(handIndex)
			?? throw new InvalidOperationException(
				$"'{playerAlias}' has no card at hand index {handIndex}");
		var slot = FindEmptySlot(playerAlias, position, battlefieldIndex)
			?? throw new InvalidOperationException(
				$"No empty slot at ({position.X}, {position.Y}) for '{playerAlias}'");
		SendInput(playerId, new UseCardRequestInput
		{
			CardId = card.Id,
			TargetEntityId = slot.Id
		});
	}

	public void Attack(string playerAlias, UnitSlotPos from, UnitSlotPos to)
	{
		var playerId = Id(playerAlias);
		var attackerSlot = FindOccupiedSlot(playerAlias, from)
			?? throw new InvalidOperationException(
				$"No unit at ({from.X}, {from.Y}) for '{playerAlias}'");
		var targetSlot = FindSlotOnOtherPlayers(playerId, to)
			?? throw new InvalidOperationException(
				$"No target slot found at ({to.X}, {to.Y})");
		SendInput(playerId, new AttackRequestInput
		{
			AttackerSlotIds = [attackerSlot.Id],
			TargetSlotId = targetSlot.Id
		});
	}

	public void EndTurn(string playerAlias)
		=> SendInput(Id(playerAlias), new EndTurnRequestInput());

	// ═══════════════════════════════════════
	//  Queries
	// ═══════════════════════════════════════

	public Player GetPlayer(string alias)
		=> State.Require<Player>(Id(alias))
		   ?? throw new InvalidOperationException($"Player '{alias}' not in state");

	public List<GenericCard> Hand(string playerAlias)
		=> GetPlayer(playerAlias).HandCardIds
			.Select(id => State.Get<GenericCard>(id))
			.OfType<GenericCard>().ToList();

	public List<Deck> Decks(string playerAlias)
		=> GetPlayer(playerAlias).DeckIds
			.Select(id => State.Get<Deck>(id))
			.OfType<Deck>().ToList();

	public List<Battlefield> Battlefields(string playerAlias)
		=> GetPlayer(playerAlias).BattlefieldIds
			.Select(id => State.Get<Battlefield>(id))
			.OfType<Battlefield>().ToList();

	public List<UnitSlot> Slots(EntityId battlefieldId)
		=> (State.Require<Battlefield>(battlefieldId)?.UnitSlotIds ?? [])
			.Select(id => State.Get<UnitSlot>(id))
			.OfType<UnitSlot>().ToList();

	public List<UnitSlot> Slots(string playerAlias, int battlefieldIndex = 0)
	{
		var bf = Battlefields(playerAlias).ElementAtOrDefault(battlefieldIndex);
		return bf != null ? Slots(bf.Id) : [];
	}

	public UnitSlot? FindEmptySlot(string playerAlias, UnitSlotPos pos, int bfIndex = 0)
		=> Slots(playerAlias, bfIndex)
			.FirstOrDefault(s => s.Position == pos && s.HoldingCardId == null);

	public UnitSlot? FindOccupiedSlot(string playerAlias, UnitSlotPos pos, int bfIndex = 0)
		=> Slots(playerAlias, bfIndex)
			.FirstOrDefault(s => s.Position == pos && s.HoldingCardId != null);

	private UnitSlot? FindSlotOnOtherPlayers(EntityId excludePlayerId, UnitSlotPos pos)
	{
		foreach (var player in State.OfType<Player>())
		{
			if (player.Id == excludePlayerId) continue;
			foreach (var bfId in player.BattlefieldIds)
			{
				var slot = Slots(bfId).FirstOrDefault(s => s.Position == pos);
				if (slot != null) return slot;
			}
		}
		return null;
	}

	// ═══════════════════════════════════════
	//  Output
	// ═══════════════════════════════════════

	public void DumpState()
	{
		Logger.Info("═══ State Dump ═══");
		Console.WriteLine(Helper.GameStateDump(State));
	}

	public void PrintHand(string playerAlias)
	{
		var cards = Hand(playerAlias);
		Logger.Info($"── {playerAlias}'s Hand ({cards.Count} cards) ──");
		for (var i = 0; i < cards.Count; i++)
			Logger.Info($"  [{i}] {cards[i].Name}  ATK:{cards[i].Atk}  HP:{cards[i].Hp}  PT:{cards[i].Pt}");
	}

	public void PrintBoard(string playerAlias)
	{
		Logger.Info($"── {playerAlias}'s Board ──");
		foreach (var slot in Slots(playerAlias)
					 .OrderBy(s => s.Position.Y).ThenBy(s => s.Position.X))
		{
			var card = slot.HoldingCardId != null
				? State.Get<GenericCard>((EntityId)slot.HoldingCardId) : null;
			var label = card != null
				? $"{card.Name}  ATK:{card.Atk}  HP:{card.Hp}"
				: "[empty]";
			Logger.Info($"  ({slot.Position.X},{slot.Position.Y}) {label}");
		}
	}
}
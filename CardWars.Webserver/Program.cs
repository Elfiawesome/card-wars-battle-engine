using CardWars.BattleEngine;
using CardWars.BattleEngine.Input.Player;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.State.Entity;
using CardWars.Core.Common.Mapping;

var be = new BattleEngine();
var p1 = be.AddPlayer();
var p2 = be.AddPlayer();
var p3 = be.AddPlayer();
var p4 = be.AddPlayer();

var deckId = be.State.Players[p1].ControllingDecks[DeckType.Unit].First();
be.HandleInput(p1, new DrawCardInput(deckId));

Console.WriteLine(StateSerializer.ToJson(be.State));

var battlefield = be.State.Battlefields.First().Value;

// Console.WriteLine(
// 	be.Mapping.GetValue<string>(battlefield, "owner_player_id")
// );

be.Mapping.Print();
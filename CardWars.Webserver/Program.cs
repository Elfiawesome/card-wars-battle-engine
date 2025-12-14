using CardWars.BattleEngine;
using CardWars.BattleEngine.State;

var be = new BattleEngine();
var p1 = be.AddPlayer();
var p2 = be.AddPlayer();
var p3 = be.AddPlayer();
var p4 = be.AddPlayer();


be.State.PrintSnapshot();

Console.WriteLine(StateSerializer.ToJson(be.State));
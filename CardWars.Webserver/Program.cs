using CardWars.BattleEngine;
using CardWars.BattleEngine.Input;

Console.WriteLine("Hello, World!");

var e = new BattleEngine();
var p1 = e.AddPlayer();
var p2 = e.AddPlayer();
var p3 = e.AddPlayer();
e.HandleInput(p1, new EndTurnInput());
e.HandleInput(p2, new EndTurnInput());
e.HandleInput(p3, new EndTurnInput());
using CardWars.BattleEngine;
using CardWars.BattleEngine.Core.Resolvers;

var Engine = new BattleEngine();

Engine._InitializeGame();
Engine.QueueResolver(new SummonUnitResolver(new(1)));
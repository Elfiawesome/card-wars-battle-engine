using CardWars.BattleEngine;
using CardWars.BattleEngine.Core.Resolvers;

var Engine = new BattleEngine();

Engine.QueueResolver(new SummonUnitResolver());
using CardWars.BattleEngine;
using CardWars.BattleEngineVanilla;
using CardWars.BattleEngineVanilla.Entity;
using CardWars.BattleEngineVanilla.Input;

var be = new BattleEngine();
var mod = new VanillaMod();
be.LoadMod(mod);


be.State.Add(new TestEntity(Guid.NewGuid()));
be.HandleInput(new TestInput());
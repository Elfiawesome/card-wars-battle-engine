using CardWars.BattleEngine;
using CardWars.BattleEngine.Vanilla;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.BattleEngine.Vanilla.Input;

var be = new BattleEngine();
var mod = new VanillaMod();
be.LoadMod(mod);


be.State.Add(new TestEntity(Guid.NewGuid()));
be.HandleInput(new TestInput());
be.HandleInput(new TestAdditionalInput());
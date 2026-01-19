using CardWars.BattleEngine;
using CardWars.BattleEngineVanilla;
using CardWars.BattleEngineVanilla.Input;

var be = new BattleEngine();
var mod = new VanillaMod();
be.LoadMod(mod);

be.HandleInput(new TestInput());
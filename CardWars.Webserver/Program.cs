using CardWars.BattleEngine;
using CardWars.BattleEngineVanilla;
using CardWars.BattleEngineVanilla.Behaviour;
using CardWars.BattleEngineVanilla.Input;

var be = new BattleEngine();
var mod = new VanillaMod();
be.LoadMod(mod);

be.HandleInput(new TestInput(), [
	new TestBehehaviour()
]);
using CardWars.BattleEngine;
using CardWars.Webserver;

var s = new BattleEngineSimulator();

var p1 = s.AddPlayer();
var p2 = s.AddPlayer();

s.DrawCard(p1);
s.PlayCard(p1,
	s.ListHandCards(p1).First().Id,
	s.ListUnitSlots(s.ListBattlefields(p1).First().Id).First((us) => us.HoldingCardId == null).Id
);



// --- Debug Dump State ---
Console.WriteLine(Helper.GameStateDump(s.State));
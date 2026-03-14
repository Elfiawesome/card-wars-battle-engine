using CardWars.BattleEngine;
using CardWars.Core.Logging;
using CardWars.Webserver;

var s = new BattleEngineSimulator();

var p1 = s.AddPlayer();
var p2 = s.AddPlayer();

s.DrawCard(p1, s.ListDecks(p1).First().Id);

s.DrawCard(p2, s.ListDecks(p2).First().Id);


// s.PlayCard(p1,
// 	s.ListHandCards(p1).First().Id,
// 	s.ListUnitSlots(s.ListBattlefields(p1).First().Id).First((us) => us.HoldingCardId == null).Id
// );

// s.DrawCard(p2);
// s.PlayCard(p2,
// 	s.ListHandCards(p2).First().Id,
// 	s.ListUnitSlots(s.ListBattlefields(p2).First().Id).First((us) => us.HoldingCardId == null).Id
// );

// --- Debug Dump State ---
Logger.Info(Helper.GameStateDump(s.State));
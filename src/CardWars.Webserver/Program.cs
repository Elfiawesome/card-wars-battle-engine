using CardWars.BattleEngine;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Data;
using CardWars.Webserver;

var s = new BattleEngineSimulator();

var p1 = s.AddPlayer();
var p2 = s.AddPlayer();

s.DrawCard(p1, s.ListDecks(p1).First().Id);
s.PlayCard(p1,
	s.ListHandCards(p1).First().Id,
	new UnitSlotPos(0, -1)
);
s.EndTurn(p1);

s.DrawCard(p2, s.ListDecks(p1).First().Id);
s.PlayCard(p2,
	s.ListHandCards(p2).First().Id,
	new UnitSlotPos(0, 0)
);
s.EndTurn(p2);

s.UnitAttack(p1,
	s.ListUnitSlots(s.ListBattlefields(p1).First().Id).First((us) => us.HoldingCardId != null).Id,
	new UnitSlotPos(0, 0)
);

// --- Debug Dump State ---
Console.WriteLine(Helper.GameStateDump(s.State));
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Webserver;

var sim = new Simulator();

sim.Step("Add Player 1", () => sim.AddPlayer("p1"));
sim.Step("Add Player 2", () => sim.AddPlayer("p2"));

sim.Step("P1 draws a card", () => sim.DrawCard("p1"));
sim.Step("P1 plays card to (0,-1)", () =>
	sim.PlayCardToSlot("p1", handIndex: 0, new UnitSlotPos(0, -1)));
sim.Step("P1 ends turn", () => sim.EndTurn("p1"));

sim.Step("P2 draws a card", () => sim.DrawCard("p2"));
sim.Step("P2 plays card to (0,0)", () =>
	sim.PlayCardToSlot("p2", handIndex: 0, new UnitSlotPos(0, -1)));
sim.Step("P2 ends turn", () => sim.EndTurn("p2"));

sim.Step("P1 attacks (0,-1) → (0,0)", () =>
	sim.Attack("p1", from: new UnitSlotPos(0, -1), to: new UnitSlotPos(0, -1)));


// sim.PrintBoard("p1");
// sim.PrintBoard("p2");
sim.DumpState();
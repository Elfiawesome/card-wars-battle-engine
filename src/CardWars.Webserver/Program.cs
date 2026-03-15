using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Webserver;

var sim = new Simulator();

// --- Startup ---
sim.Step("Add Player 1", () => sim.AddPlayer("p1"));
sim.Step("Add Player 2", () => sim.AddPlayer("p2"));

// --- Setup Phase ---
// Player 1
sim.Step("P1 draws 1st card", () => sim.DrawCard("p1"));
sim.Step("P1 draws 2nd card", () => sim.DrawCard("p1"));
sim.Step("P1 draws 3rd card", () => sim.DrawCard("p1"));
sim.Step("P1 plays card to (0,-1)", () =>
	sim.PlayCardToSlot("p1", handIndex: 0, new UnitSlotPos(0, -1)));
sim.Step("P1 plays card to (-1, 0)", () =>
	sim.PlayCardToSlot("p1", handIndex: 0, new UnitSlotPos(-1, 0)));
sim.Step("P1 plays card to (1, 0)", () =>
	sim.PlayCardToSlot("p1", handIndex: 0, new UnitSlotPos(1, 0)));
sim.Step("P1 ends turn", () => sim.EndTurn("p1"));

// Player 2
sim.Step("P2 draws a card", () => sim.DrawCard("p2"));
sim.Step("P2 plays card to (0,0)", () =>
	sim.PlayCardToSlot("p2", handIndex: 0, new UnitSlotPos(0, 0)));
sim.Step("P2 ends turn", () => sim.EndTurn("p2"));


// --- Attacking Phase ---
// Player 1
sim.Step("P1 ends turn", () => sim.EndTurn("p1"));

// Player 2
sim.Step("P2 attacks (0, 0) → (0, -1)", () =>
	sim.Attack("p2", from: new UnitSlotPos(0, 0), to: new UnitSlotPos(0, -1)));
sim.Step("P2 attacks (0, 0) → (-1, 0)", () =>
	sim.Attack("p2", from: new UnitSlotPos(0, 0), to: new UnitSlotPos(-1, 0)));


// sim.PrintBoard("p1");
// sim.PrintBoard("p2");
sim.DumpState();
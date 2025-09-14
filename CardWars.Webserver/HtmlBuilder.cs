using CardWars.BattleEngine.Core.Inputs;
using CardWars.BattleEngine.Core.States;

namespace CardWars.Webserver;

public class HTMLBuilder
{
	public GameState? state { get; set; }
	public List<string> options = [];
	private string _styleString = """
body {
	background-color: #121212;
	color: #e0e0e0;
	font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Helvetica, Arial, sans-serif;
	margin: 0;
	padding: 2rem;
}

h1,
h2,
h3,
h4 {
	border-bottom: 1px solid #333;
	padding-bottom: 10px;
}

h3,
h4 {
	margin: 0px;
}

h4 {
	padding-bottom: 0px;
}

/* Main container */
.control-panel {
	max-width: 1200px;
	margin: auto;
}

/* Section styling */
.section {
	margin-bottom: 2rem;
	background-color: #1e1e1e;
	padding: 1.5rem;
	border-radius: 8px;
}

/* Turn Order Display */
.player-turn-list {
	display: flex;
	justify-content: flex-start;
	padding: 0;
	list-style: none;
}

.player-turn {
	padding: 10px 15px;
	margin-right: 10px;
	background-color: #282828;
	border-radius: 4px;
	font-weight: bold;
}

.player-turn.active-turn {
	background-color: #007bff;
	/* A bright color to indicate the active player */
	color: #ffffff;
}

/* Players List */
.player-list ul {
	list-style: none;
	padding: 0;
}

.player-list li {
	background-color: #282828;
	padding: 10px;
	margin-bottom: 8px;
	border-radius: 4px;
}

/* Battlefield Grid */
.battlefield-grid {
	display: grid;
	grid-template-columns: auto auto auto auto;
	/* grid-template-columns: repeat(auto-fill, minmax(100px, 1fr)); */
	gap: 10px;
}

/* Unit Slots */
.unit-slot {
	background-color: #2a2a2a;
	padding: 0.5em;
	border: 1px dashed #444;
	border-radius: 4px;
	min-height: 100px;
	display: flex;
	align-items: center;
	flex-direction: column;
}


/* Unit inside a slot */
.unit {
	text-align: left;
	background-color: #00000038;
	padding: 0.5em;
	border: 1px dashed #444;
	border-radius: 4px;
}

.unit-name {
	font-weight: bold;
}

.unit-stat {}

/* Input Form Styling */
#input-section form {
	display: flex;
	flex-direction: column;
	gap: 1rem;
}

.form-group {
	display: flex;
	flex-direction: column;
}

.form-group label {
	margin-bottom: 0.5rem;
	font-weight: bold;
}

.form-group select,
.form-group button {
	padding: 0.8rem;
	border-radius: 4px;
	background-color: #2a2a2a;
	color: #e0e0e0;
	border: 1px solid #333;
}

.form-group button {
	background-color: #007bff;
	color: white;
	border: none;
	cursor: pointer;
	transition: background-color 0.3s;
}

.form-group button:hover {
	background-color: #0056b3;
}

/* Input Boxes */
.section select {
	width: 40%;
	background-color: #2a2a2a;
	border: none;
	padding: 0.5rem;
	border-radius: 8px;
}
""";


	public HTMLBuilder()
	{

	}

	private string BuildPlayers()
	{
		if (state == null) { return ""; }
		string d = "";
		foreach (var player in state.Players)
		{
			string controllingBattlefields = "";
			foreach (var battlefieldId in player.Value.ControllingBattlefields)
			{
				controllingBattlefields += $"[{battlefieldId.Value}]";
			}
			d += $"<li>[{player.Key.Value}]: {player.Value.Name} -> {controllingBattlefields}</li>";
		}
		return d;
	}

	private string BuildBattlefields()
	{
		if (state == null) { return ""; }
		string d = "";
		foreach (var battlefield in state.Battlefields)
		{

			d += $"""<div class="battlefield"><h3>[{battlefield.Key.Value}]</h3><div class="battlefield-grid">{BuildUnitSlots(battlefield.Value.UnitSlots)}</div></div>""";
		}
		return d;
	}

	private string BuildUnitSlots(List<UnitSlotId> unitSlotIds)
	{
		if (state == null) { return ""; }
		string d = "";
		foreach (var unitSlotId in unitSlotIds)
		{
			if (state.UnitSlots.TryGetValue(unitSlotId, out var unitSlot))
			{
				if (unitSlot.HoldingUnit != null)
				{
					d += $"""<div class="unit-slot"><h3>[{unitSlotId.Value}]</h3>{BuildUnit((UnitId)unitSlot.HoldingUnit)}</div>""";
				}
				else
				{
					d += $"""<div class="unit-slot"><h3>[{unitSlotId.Value}]</h3></div>""";
				}
			}
		}
		return d;
	}

	private string BuildUnit(UnitId unitId)
	{
		if (state == null) { return ""; }
		string d = "";
		if (state.Units.TryGetValue(unitId, out var unit))
		{
			d += $"""
<div class="unit">
	<div class="unit-name">[{unitId.Value}]: {unit.Name}</div>
	<div class="unit-stat">{unit.Hp} HP</div>
	<div class="unit-stat">{unit.Atk} ATK</div>
</div>
""";
		}
		return d;
	}

	private string BuildTurnOrder()
	{
		string d = "";
		if (state == null) { return ""; }
		foreach (var playerId in state.TurnOrder)
		{
			if (playerId == state.CurrentPlayer)
			{
				d += $"""<div class="player-turn active-turn">{playerId.Value}</div>""";
			}
			else
			{
				d += $"""<div class="player-turn">{playerId.Value}</div>""";
			}
		}
		return d;
	}

	public string Build()
	{
		if (state == null) { return ""; }
		return $"""
<!DOCTYPE html>
<html lang="en">

<head>
	<meta charset="UTF-8">
	<meta name="viewport" content="width=device-width, initial-scale=1.0">
	<title>Card Wars Control Panel</title>
	<style>{_styleString}</style>
</head>

<body>

	<div class="control-panel">
		<h1>Card Wars Game Control Panel</h1>
		<!-- Turn Order Section -->
		<div id="turn-order" class="section">
			<h2>Turn Order</h2>
			<div class="player-turn-list">
				{BuildTurnOrder()}
			</div>
		</div>

		<!-- Players Section -->
		<div id="players" class="section">
			<h2>Players</h2>
			<div class="player-list">
				<ul>
					{BuildPlayers()}
				</ul>
			</div>
		</div>

		<!-- Battlefields Section -->
		<div id="battlefields" class="section">
			<h2>Battlefields</h2>
			{BuildBattlefields()}
		</div>
	</div>

	<div class="control-panel">
		<h1>Input</h1>
		<div id="input-section" class="section">
			<form>
				<div class="form-group">
					<label for="player-select">Player:</label>
					<select id="player-select" name="player">
						{BuildPlayerSelect()}
					</select>
				</div>
				<div class="form-group">
					<label for="action-select">Action:</label>
					<select id="action-select" name="action">
						{BuildActionSelect()}
					</select>
				</div>
				<div class="form-group">
					<button type="submit">Submit Action</button>
				</div>
			</form>
		</div>
	</div>

</body>

</html>
""";

	}


	private string BuildPlayerSelect()
	{
		if (state == null) { return ""; }
		string d = "";
		foreach (var player in state.Players)
		{
			d += $"""<option value="{player.Key.Value}">{player.Key.Value}: {player.Value.Name}</option>""";
		}
		return d;
	}

	private string BuildActionSelect()
	{
		if (state == null) { return ""; }
		string d = "";
		foreach (var option in options)
		{
			d += $"""<option value="{option}">{option}</option>""";
		}
		return d;
	}
}
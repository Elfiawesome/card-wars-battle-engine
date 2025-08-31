
using System.Net;
using System.Text;
using CardWars.BattleEngine;


// Engine Singleton
var engine = new BattleEngine();

// Setup webapp server
var listener = new HttpListener();
string url = "http://localhost:8080/";
listener.Prefixes.Add(url);
listener.Start();
Console.WriteLine($"Server is listening on {url}");
Console.WriteLine("Open your browser and navigate to the URL to interact with the game.");


while (true)
{
	HttpListenerContext context = await listener.GetContextAsync();
	HttpListenerRequest request = context.Request;
	HttpListenerResponse response = context.Response;

	Console.WriteLine($"Received request: {request.HttpMethod} {request?.Url?.AbsolutePath}");

	string responseString = "No Response... :(";

	if (request?.HttpMethod == "GET")
	{
		responseString = RenderGameStateAsHtml(engine);
		response.ContentType = "text/html";
		response.StatusCode = (int)HttpStatusCode.OK;
	}
	else if (request?.HttpMethod == "POST")
	{

	}


	if (!string.IsNullOrEmpty(responseString))
	{
		byte[] buffer = Encoding.UTF8.GetBytes(responseString);
		response.ContentLength64 = buffer.Length;
		response.OutputStream.Write(buffer, 0, buffer.Length);
	}
	response.OutputStream.Close();
}


// TODO better renderer
static string RenderGameStateAsHtml(BattleEngine engine)
{
	var gameState = engine.GameState;
	var sb = new StringBuilder();

	// --- Start of HTML Document ---
	sb.Append("<!DOCTYPE html><html><head><title>Card Wars - Full State</title>");
	sb.Append("<style>body { font-family: sans-serif; } table { border-collapse: collapse; margin-bottom: 20px; } th, td { border: 1px solid #ccc; padding: 8px; text-align: left; } th { background-color: #f2f2f2; } h2 { border-bottom: 2px solid #000; padding-bottom: 5px; } </style>");
	sb.Append("</head><body>");
	sb.Append("<h1>Card Wars Full Game State</h1>");

	// --- Battlefield View Section ---
	sb.Append("<h2>Battlefield View</h2>");

	// Loop over every player and render their battlefields
	foreach (var player in gameState.Players.Values)
	{
		sb.Append($"<h3>Player {player.Id.Value}'s Battlefields</h3>");
		if (!player.ControllingBattlefields.Any())
		{
			sb.Append("<p>This player controls no battlefields.</p>");
			continue;
		}

		// A player might control multiple battlefields, so we loop through them
		foreach (var battlefieldId in player.ControllingBattlefields)
		{
			if (gameState.Battlefields.TryGetValue(battlefieldId, out var battlefield))
			{
				sb.Append($"<h4>Battlefield ID: {battlefieldId.Value}</h4>");
				sb.Append("<table style='width: 600px; text-align: center;'><tr>");
				for (int i = 0; i < battlefield.UnitSlots.Count; i++) sb.Append($"<th>Slot {i}</th>");
				sb.Append("</tr><tr>");
				foreach (var slotId in battlefield.UnitSlots)
				{
					var slot = gameState.UnitSlots[slotId];
					string unitInfo = "<em>-empty-</em>";
					if (slot.HoldingUnit.Value != 0 && gameState.Units.TryGetValue(slot.HoldingUnit, out var unit))
					{
						unitInfo = $"<b>{unit.Name}</b><br/><small>Unit ID: {unit.Id.Value}</small>";
					}
					sb.Append($"<td style='height: 50px;'>{unitInfo}</td>");
				}
				sb.Append("</tr></table>");
			}
		}
	}

	// --- Interaction Form (Now includes Battlefield ID) ---
	sb.Append("<h2>Summon a Unit</h2>");
	sb.Append("<form method='post' action='/summon'>");
	sb.Append("<label for='battlefieldId'>Battlefield ID:</label> <input type='number' id='battlefieldId' name='battlefieldId' required /> ");
	sb.Append("<label for='slotIndex'>Slot Index:</label> <input type='number' id='slotIndex' name='slotIndex' min='0' required /> ");
	sb.Append("<label for='unitName'>Unit Name:</label> <input type='text' id='unitName' name='unitName' required /> ");
	sb.Append("<input type='submit' value='Summon' />");
	sb.Append("</form><hr>");

	// --- Raw Game State Dump (Unchanged) ---
	sb.Append("<h2>Raw Game State Objects</h2>");

	// Players Table
	sb.Append("<h3>Players</h3><table><tr><th>PlayerId</th><th>ControllingBattlefields</th></tr>");
	foreach (var p in gameState.Players) sb.Append($"<tr><td>{p.Key.Value}</td><td>[{string.Join(", ", p.Value.ControllingBattlefields.Select(id => id.Value))}]</td></tr>");
	sb.Append("</table>");

	// Battlefields Table
	sb.Append("<h3>Battlefields</h3><table><tr><th>BattlefieldId</th><th>UnitSlots</th></tr>");
	foreach (var b in gameState.Battlefields) sb.Append($"<tr><td>{b.Key.Value}</td><td>[{string.Join(", ", b.Value.UnitSlots.Select(id => id.Value))}]</td></tr>");
	sb.Append("</table>");

	// Unit Slots Table
	sb.Append("<h3>Unit Slots</h3><table><tr><th>UnitSlotId</th><th>HoldingUnit</th></tr>");
	foreach (var us in gameState.UnitSlots) sb.Append($"<tr><td>{us.Key.Value}</td><td>{(us.Value.HoldingUnit.Value == 0 ? "null" : us.Value.HoldingUnit.Value)}</td></tr>");
	sb.Append("</table>");

	// Units Table
	sb.Append("<h3>Units</h3><table><tr><th>UnitId</th><th>Name</th><th>Abilities</th></tr>");
	foreach (var u in gameState.Units) sb.Append($"<tr><td>{u.Key.Value}</td><td>{u.Value.Name}</td><td>[{string.Join(", ", u.Value.Abilities.Select(id => id.Value))}]</td></tr>");
	sb.Append("</table>");

	// Abilities Table
	sb.Append("<h3>Abilities</h3><table><tr><th>AbilityId</th><th>Type</th></tr>");
	foreach (var a in gameState.Abilities) sb.Append($"<tr><td>{a.Key.Value}</td><td>{a.Value.GetType().Name}</td></tr>");
	sb.Append("</table>");

	// --- End of HTML Document ---
	sb.Append("</body></html>");
	return sb.ToString();
}
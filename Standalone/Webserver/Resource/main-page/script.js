

document.addEventListener("DOMContentLoaded", () => {
	const connectionStatus = document.getElementById('connection-status');
	const playersSection = document.getElementById('players-section');
	const battlefieldSection = document.getElementById('battlefield-section');
	const commandInput = document.getElementById('command-input');
	const sendCommandBtn = document.getElementById('send-command-btn');
	const commandLog = document.getElementById('command-log');

	fetch("api/gamestate").then(response => {
		response.json().then(data => {
			for (var playerId in data["Players"]) {
				playerData = data["Players"][playerId]

				controllingBattlefields = data["Players"]["ControllingBattlefields"]
				battlefieldHtml = ``
				blist = ""
				for (var battlefieldId in controllingBattlefields) {
					blist += `<li>${battlefieldId}</li>`
				}
				if (blist == "")
				{
					battlefieldHtml = `<h4>Battlefields:</h4><ul>${blist}</ul>`
				}

				playersSection.innerHTML += `
					<div class="player-card">
						<h4>Player ${playerData["Name"]} | ${playerId}</h4>
						<h4>Data:</h4>
						<ul>
							<li>Points: 10/10</li>
							<li>Deck: 10/10</li>
						</ul>
						${battlefieldHtml}
					</div>
				`
			}
		})
	})

	function updateControlPage() {

	}

	updateControlPage();
});
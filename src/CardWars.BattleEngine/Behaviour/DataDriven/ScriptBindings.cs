using CardWars.Core.Registry;

namespace CardWars.BattleEngine.Behaviour.DataDriven;

public static class ScriptBindings
{
	public static readonly ResourceId Event = new ResourceId("cardwars", "event");
	public static readonly ResourceId EventArgs = new ResourceId("cardwars", "event_args");
	public static readonly ResourceId Input = new ResourceId("cardwars", "input");
	public static readonly ResourceId InputArgs = new ResourceId("cardwars", "input_args");
	public static readonly ResourceId InputPlayer = new ResourceId("cardwars", "input_player");
	public static readonly ResourceId Owner = new ResourceId("cardwars", "owner");
	public static readonly ResourceId Self = new ResourceId("cardwars", "self");
	public static readonly ResourceId State = new ResourceId("cardwars", "state");
	public static readonly ResourceId Vars = new ResourceId("cardwars", "vars");
}

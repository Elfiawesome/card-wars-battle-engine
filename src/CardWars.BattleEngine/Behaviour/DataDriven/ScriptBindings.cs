using CardWars.Core.Registry;

namespace CardWars.BattleEngine.Behaviour.DataDriven;

public static class ScriptBindings
{
	public static readonly ResourceId Event = ResourceId.Vanilla("event");
	public static readonly ResourceId EventArgs = ResourceId.Vanilla("event_args");
	public static readonly ResourceId Input = ResourceId.Vanilla("input");
	public static readonly ResourceId InputArgs = ResourceId.Vanilla("input_args");
	public static readonly ResourceId InputPlayer = ResourceId.Vanilla("input_player");
	public static readonly ResourceId Owner = ResourceId.Vanilla("owner");
	public static readonly ResourceId Self = ResourceId.Vanilla("self");
	public static readonly ResourceId State = ResourceId.Vanilla("state");
	public static readonly ResourceId Vars = ResourceId.Vanilla("vars");
}

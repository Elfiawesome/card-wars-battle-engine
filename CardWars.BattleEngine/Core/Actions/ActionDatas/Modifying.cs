using CardWars.BattleEngine.Core.States;

namespace CardWars.BattleEngine.Core.Actions.ActionDatas;

public class UpdateUnitActionData() : ActionData
{
	public UnitId UnitId { get; set; }
	public string? Name { get; set; } = null;
	public int? Hp { get; set; } = null;
	public int? Atk { get; set; } = null;
	public int? Pt { get; set; } = null;
}
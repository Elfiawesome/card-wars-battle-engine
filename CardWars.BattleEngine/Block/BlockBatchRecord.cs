namespace CardWars.BattleEngine.Block;

public record struct BlockBatchRecord()
{
	public string AnimationId { get; set; } = "";
	public List<IBlock> Blocks { get; set; } = [];

}
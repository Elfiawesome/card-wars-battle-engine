using System.Reflection;
using CardWars.BattleEngine.State.Entity;
using CardWars.Core.Common.Dispatching;

namespace CardWars.BattleEngine.Block;

public class BlockDispatcher : RequestDispatcher<IServiceContainer, IBlock, bool>
{
	public Action<PlayerId, BlockBatchRecord>? BlockBatchProcessedAction = null;

	public override void Register()
	{

		RequestRegister.RegisterAssembly<
			BlockHandlerRegistry,
			RequestDispatcher<IServiceContainer, IBlock, bool>>
				(Assembly.GetExecutingAssembly(), this);
	}

	public void Handle(IServiceContainer serviceContainer, BlockBatch blockBatch)
	{
		Dictionary<PlayerId, BlockBatchRecord> blockBatchRecords = [];

		foreach (var block in blockBatch.Blocks)
		{
			var success = Handle(serviceContainer, block.Block);
			if (success)
			{
				foreach (var playerId in serviceContainer.State.Players.Keys)
				{
					if (!blockBatchRecords.TryGetValue(playerId, out var blockBatchRecord))
					{
						blockBatchRecord = new BlockBatchRecord();
						blockBatchRecords.Add(playerId, blockBatchRecord);
					}

					if (block.TargetedPlayerIds == null)
					{
						blockBatchRecord.Blocks.Add(block.Block);
						continue;
					}
					if (block.TargetedPlayerIds.Count == 0)
					{
						blockBatchRecord.Blocks.Add(block.Block);
						continue;
					}
					if (block.TargetedPlayerIds.Contains(playerId))
					{
						blockBatchRecord.Blocks.Add(block.Block);
						continue;
					}
				}
			}
		}

		foreach (var item in blockBatchRecords)
		{
			BlockBatchProcessedAction?.Invoke(item.Key, item.Value);
		}
	}
}
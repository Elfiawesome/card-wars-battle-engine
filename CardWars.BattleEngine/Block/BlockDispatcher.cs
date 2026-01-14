using System.Reflection;
using CardWars.BattleEngine.State.Entity;
using CardWars.Core.Common.Dispatching;

namespace CardWars.BattleEngine.Block;

public class BlockDispatcher : RequestDispatcher<IServiceContainer, IBlock, bool>
{
	public Action<PlayerId, BlockBatchRecord>? BlockBatchProcessedAction = null;

	public override void Register()
	{
		RegisterAssembly(Assembly.GetExecutingAssembly());
	}

	// Might change this for a .py script to create the auto registration
	public void RegisterAssembly(Assembly assembly)
	{
		var handlerTypes = assembly.GetTypes()
			.Where(t => t.GetCustomAttribute<BlockHandlerRegistry>() != null && t.IsClass && !t.IsAbstract);
		
		var registerMethodDefinition = typeof(RequestDispatcher<IServiceContainer, IBlock, bool>)
			.GetMethod(nameof(RegisterHandler));
		
		if (registerMethodDefinition == null) return;
		
		foreach (var handlerType in handlerTypes)
		{
			// Find the IBlockHandler<T> interface implementation to determine T
			var interfaceType = handlerType.GetInterfaces()
				.FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IBlockHandler<>));

			if (interfaceType == null) continue;

			// Extract the Block type (e.g., AttachBattlefieldToPlayerBlock)
			var blockType = interfaceType.GetGenericArguments()[0];

			// Create instance of the handler
			var handlerInstance = Activator.CreateInstance(handlerType);

			// Call RegisterHandler<T>(handlerInstance) dynamically
			var genericRegisterMethod = registerMethodDefinition.MakeGenericMethod(blockType);
			
			try 
			{
				genericRegisterMethod.Invoke(this, [handlerInstance]);
			}
			catch (TargetInvocationException ex)
			{
				throw ex.InnerException ?? ex;
			}
		}
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
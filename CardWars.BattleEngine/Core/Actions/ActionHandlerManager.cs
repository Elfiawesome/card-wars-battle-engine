using CardWars.BattleEngine.Core.Actions.ActionHandlers;
using CardWars.BattleEngine.Core.States;

namespace CardWars.BattleEngine.Core.Actions;

public class ActionHandlerManager
{
	private readonly Dictionary<Type, Action<GameState, IActionData>> _handlers = [];

	public ActionHandlerManager()
	{
		// Initialize all the handlers here
		RegisterHandler<AttachAbilityToUnitHandler, AttachAbilityToUnitData>();
		RegisterHandler<AttachBattlefieldToPlayerHandler, AttachBattlefieldToPlayerData>();
		RegisterHandler<AttachUnitSlotToBattlefieldHandler, AttachUnitSlotToBattlefieldData>();
		RegisterHandler<AttachUnitToUnitSlotHandler, AttachUnitToUnitSlotData>();

		RegisterHandler<InstantiateAbilityHandler, InstantiateAbilityData>();
		RegisterHandler<InstantiateBattlefieldHandler, InstantiateBattlefieldData>();
		RegisterHandler<InstantiateUnitHandler, InstantiateUnitData>();
		RegisterHandler<InstantiatePlayerHandler, InstantiatePlayerData>();
		RegisterHandler<InstantiateUnitSlotHandler, InstantiateUnitSlotData>();

		RegisterHandler<AddPlayerToTurnOrderHandler, AddPlayerToTurnOrderData>();
		RegisterHandler<AdvanceTurnOrderHandler, AdvanceTurnOrderData>();
	}

	public void RegisterHandler<TActionHandler, TActionData>()
		where TActionHandler : ActionHandler<TActionData>, new()
		where TActionData : IActionData
	{
		var handler = new TActionHandler();
		if (_handlers.ContainsKey(typeof(TActionData)))
		{
			return;
		}
		_handlers[typeof(TActionData)] = (gameState, actionData) =>
		{
			if (actionData is TActionData typedActionData)
			{
				var success = handler.Handle(gameState, typedActionData);
				if (!success)
				{
					Console.WriteLine($"Action Handler [{handler.GetType().Name}] was not successful");
				}
			}
		};
	}

	public void HandleActionData<TActionData>(GameState gameState, TActionData actionData)
		where TActionData : IActionData
	{
		// TODO: point action data to its handler
		var actionDataType = actionData.GetType();
		if (_handlers.TryGetValue(actionDataType, out var handler))
		{
			handler.Invoke(gameState, actionData);
		}
	}
}

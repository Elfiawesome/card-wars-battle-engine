using CardWars.BattleEngine.Core.States;

namespace CardWars.BattleEngine.Core.Actions;

public class ActionHandlerManager
{
	private readonly Dictionary<Type, Action<GameState, ActionData>> _handlers = [];

	public ActionHandlerManager()
	{
		// Initialize all the handlers here
	}

	public void RegisterHandler<TActionHandler, TActionData>(TActionHandler handler)
		where TActionHandler : ActionHandler<TActionData>
		where TActionData : ActionData
	{
		if (_handlers.ContainsKey(typeof(TActionData)))
		{
			return;
		}
		_handlers[typeof(TActionData)] = (gameState, actionData) =>
		{
			if (actionData is TActionData typedActionData)
			{
				handler.Handle(gameState, typedActionData);
			}
		};
	}

	public void HandleActionData<TActionData>(GameState gameState, TActionData actionData)
		where TActionData : ActionData
	{
		// TODO: point action data to its handler
		var actionDataType = actionData.GetType();
		if (_handlers.TryGetValue(actionDataType, out var handler))
		{
			handler.Invoke(gameState, actionData);
		}
	}
}

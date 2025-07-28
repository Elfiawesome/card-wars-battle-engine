using CardWars.BattleEngine.Core.Actions.ActionDatas.Attachments;
using CardWars.BattleEngine.Core.Actions.ActionDatas.Creations;
using CardWars.BattleEngine.Core.Actions.ActionDatas.Modifications;
using CardWars.BattleEngine.Core.Actions.ActionHandlers.Creations;
using CardWars.BattleEngine.Core.Actions.ActionHandlers.Modifications;
using CardWars.BattleEngine.Core.Events;
using CardWars.BattleEngine.Core.States;

namespace CardWars.BattleEngine.Core.Actions;

public class ActionHandlerManager
{
	private readonly Dictionary<Type, object> _handlers = [];
	
	public ActionHandlerManager()
	{
		// Initialize all the handlers here

		// Attachments
		RegisterHandler<AttachUnitToUnitSlotActionHandler, AttachUnitToUnitSlotActionData>(new());

		// Creations
		RegisterHandler<CreateUnitActionHandler, CreateUnitActionData>(new());
		RegisterHandler<CreateUnitSlotActionHandler, CreateUnitSlotActionData>(new());

		// Modifications
		RegisterHandler<UpdateUnitActionHandler, UpdateUnitActionData>(new());
	}

	public void RegisterHandler<TActionHandler, TActionData>(TActionHandler handler)
		where TActionHandler : ActionHandler<TActionData>
		where TActionData : ActionData
	{
		if (_handlers.ContainsKey(typeof(TActionData)))
		{
			return;
		}
		_handlers.Add(typeof(TActionData), handler);
	}

	public void HandleActionData<TActionData>(GameState gameState, EventManager eventManager, TActionData actionData)
		where TActionData : ActionData
	{
		// TODO: point action data to its handler
	}
}

using CardWars.BattleEngine.registry;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Data;
using CardWars.Core.Registry;

namespace CardWars.BattleEngine.Vanilla.State;

public sealed class CardWarsStateInitializer : IStateInitializer
{
	public void Initialize(GameState state)
		=> state.Ext.GetOrCreate<CardWarsBattleState>();
}

public sealed class CardWarsGameOverPolicy : IGameOverPolicy
{
	public bool IsGameOver(GameState state)
		=> state.Ext.GetOrCreate<CardWarsBattleState>().Outcome.IsGameOver;
}

public sealed class CardWarsInputPolicy : IInputPolicy
{
	public bool CanProcessInput(GameState state, EntityId playerId)
		=> state.Ext.GetOrCreate<CardWarsBattleState>().Turn.AllowedPlayerInputs.Contains(playerId);
}

public sealed class CardWarsBehaviourProvider(DataTagContext dataTags) : IEntityBehaviourProvider
{
	public IReadOnlyList<ContentPointer> GetBehaviours(IEntity entity)
		=> entity is GenericCard card ? card.GetBehaviourPointers(dataTags) : [];
}

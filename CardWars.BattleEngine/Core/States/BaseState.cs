namespace CardWars.BattleEngine.Core.States;

public abstract class BaseState<TId>
	where TId : struct
{
	public readonly TId Id;
	public GameState GameState;

	public BaseState(GameState state, TId id)
	{
		Id = id;
		GameState = state;
		Ready(state, id);
	}
	public virtual void Ready(GameState state, TId id) { }
}
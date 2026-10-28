using CardWars.BattleEngine.Behaviour;
using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Behaviour.DataDriven.Instructions;
using CardWars.Core.Data.Mapping;
using CardWars.Core.Data.Tags;
using CardWars.Core.Logging;
using CardWars.Core.Registry;

namespace CardWars.BattleEngine.Behaviour.DataDriven.Runtime;

public sealed class ScriptRuntimeContext
{
	private readonly List<ScriptScope> _scopes = [new()];

	public BehaviourContext Behaviour { get; }
	public DataDrivenRegistry DataDriven { get; }
	public ScriptExecutor Executor { get; }
	public ScriptScope CurrentScope => _scopes[^1];
	public IReadOnlyDictionary<ResourceId, DataTag> Variables => CurrentScope.Variables;
	public int MaxLoopIterations { get; set; } = 10_000;

	public ScriptRuntimeContext(BehaviourContext behaviour, DataDrivenRegistry dataDriven)
	{
		Behaviour = behaviour;
		DataDriven = dataDriven;
		Executor = new ScriptExecutor(this, dataDriven.InstructionHandlers);
	}

	public DataTag Evaluate(IExpression expression)
	{
		if (DataDriven.ExpressionHandlers.TryExecute(this, expression, out var result) && result != null) return result;
		return new NullTag();
	}

	public void PushScope() => _scopes.Add(new ScriptScope());

	public void PopScope()
	{
		if (_scopes.Count > 1) _scopes.RemoveAt(_scopes.Count - 1);
	}

	public void DeclareVariable(ResourceId name, DataTag value, bool isConstant = false)
	{
		CurrentScope.Variables[name] = value;
		if (isConstant) CurrentScope.Constants.Add(name);
	}

	public bool TryGetVariable(ResourceId name, out DataTag? value)
	{
		for (var i = _scopes.Count - 1; i >= 0; i--)
		{
			if (_scopes[i].Variables.TryGetValue(name, out value)) return true;
		}

		value = null;
		return false;
	}

	public bool TrySetVariable(ResourceId name, DataTag value)
	{
		for (var i = _scopes.Count - 1; i >= 0; i--)
		{
			var scope = _scopes[i];
			if (!scope.Variables.ContainsKey(name)) continue;
			if (scope.Constants.Contains(name))
			{
				Log.Warn($"Cannot assign to constant variable [{name}]");
				return false;
			}

			scope.Variables[name] = value;
			return true;
		}

		Log.Warn($"Unknown variable [{name}]");
		return false;
	}

	public T? ResolveObject<T>(DataTag? tag) where T : class
	{
		switch (tag)
		{
			case ObjectRefTag objectRef when objectRef.Value is T typed:
				return typed;
			case NullTag or null:
				return null;
			case CompoundTag compound:
				try
				{
					return DataTagMapper.FromTag<T>(compound);
				}
				catch (Exception ex)
				{
					Log.Error($"Could not resolve [{typeof(T).Name}] from data tag: {ex.Message}");
					return null;
				}
			default:
				return null;
		}
	}

	public InstructionOutcome ExecuteBlock(IReadOnlyList<IInstruction>? instructions)
	{
		PushScope();
		try
		{
			return Executor.Execute(instructions);
		}
		finally
		{
			PopScope();
		}
	}
}

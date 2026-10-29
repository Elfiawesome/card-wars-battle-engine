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
	public ScriptScope GlobalScope { get; }
	public ScriptScope CurrentScope => _scopes[^1];
	public IReadOnlyDictionary<ResourceId, DataTag> Variables => CurrentScope.Variables;
	public IReadOnlyDictionary<ResourceId, DataTag> GlobalVariables => GlobalScope.Variables;
	public int MaxLoopIterations { get; set; } = 10_000;

	public ScriptRuntimeContext(BehaviourContext behaviour, DataDrivenRegistry dataDriven)
	{
		Behaviour = behaviour;
		DataDriven = dataDriven;
		GlobalScope = behaviour.State.GlobalScope;
		Executor = new ScriptExecutor(this, dataDriven.InstructionHandlers);
	}

	public DataTag Evaluate(IExpression expression)
	{
		if (DataDriven.ExpressionHandlers.TryExecute(this, expression, out var result) && result != null) return result;
		return new NullTag();
	}

	public ScriptScope PushScope()
	{
		var scope = new ScriptScope();
		_scopes.Add(scope);
		return scope;
	}

	public void PopScope()
	{
		if (_scopes.Count > 1) _scopes.RemoveAt(_scopes.Count - 1);
	}

	public void DeclareVariable(ResourceId name, DataTag value, bool isConstant = false, bool isGlobal = false)
		=> DeclareVariable(isGlobal ? GlobalScope : CurrentScope, name, value, isConstant);

	public void DeclareVariable(ScriptScope scope, ResourceId name, DataTag value, bool isConstant = false)
	{
		if (scope.Constants.Contains(name))
		{
			Log.Warn($"Cannot redeclare constant variable [{name}]");
			return;
		}

		scope.Variables[name] = value;
		if (isConstant) scope.Constants.Add(name);
	}

	public bool TryGetVariable(ResourceId name, out DataTag? value)
	{
		for (var i = _scopes.Count - 1; i >= 0; i--)
		{
			if (_scopes[i].Variables.TryGetValue(name, out value)) return true;
		}

		if (GlobalScope.Variables.TryGetValue(name, out value)) return true;

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

		if (GlobalScope.Variables.ContainsKey(name))
		{
			if (GlobalScope.Constants.Contains(name))
			{
				Log.Warn($"Cannot assign to constant variable [{name}]");
				return false;
			}

			GlobalScope.Variables[name] = value;
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

	public IEnumerable<InstructionOutcome> ExecuteBlock(IReadOnlyList<IInstruction>? instructions)
	{
		if (instructions == null) yield break;

		PushScope();
		try
		{
			foreach (var outcome in Executor.Execute(instructions))
				yield return outcome;
		}
		finally
		{
			PopScope();
		}
	}
}

using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;

namespace CardWars.BattleEngine.Behaviour.DataDriven.Objects;

public sealed class RuntimeVariablesObjectAccessor : IObjectAccessor
{
	public bool TryGetMember(object target, string member, out DataTag? value)
	{
		if (target is not ScriptContext context)
		{
			value = null;
			return false;
		}

		if (context.TryGetVariable(ResourceId.Parse(member), out var variable) && variable != null)
		{
			value = variable;
			return true;
		}

		value = null;
		return false;
	}

	public bool TrySetMember(object target, string member, DataTag value)
		=> target is ScriptContext context && context.TrySetVariable(ResourceId.Parse(member), value);

	public bool TryGetIndex(object target, DataTag index, out DataTag? value)
	{
		if (target is not ScriptContext context || index is not StringTag stringTag)
		{
			value = null;
			return false;
		}

		return TryGetMember(context, stringTag.Value, out value);
	}

	public bool TrySetIndex(object target, DataTag index, DataTag value)
		=> target is ScriptContext context
			&& index is StringTag stringTag
			&& context.TrySetVariable(ResourceId.Parse(stringTag.Value), value);
}

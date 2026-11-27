using CardWars.Core.Data.Access;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;

namespace CardWars.BattleEngine.Vanilla.State;

[DataTagType("battle/cardwars_state")]
public sealed class CardWarsBattleState : IDataTagAccessible
{
	[DataTag] public TurnState Turn { get; set; } = new();
	[DataTag] public LayoutState Layout { get; set; } = new()
	{
		Layout = ResourceId.Empty,
		LayoutConfig = new CompoundTag()
	};
	[DataTag] public ResourceId RuleId { get; set; } = ResourceId.Empty;
	[DataTag] public GameOutcome Outcome { get; set; } = GameOutcome.Continue;

	public bool TryGetMember(string member, out DataTag? value)
	{
		switch (Normalize(member))
		{
			case "turn":
				value = new ObjectRefTag(Turn);
				return true;
			case "layout":
				value = new ObjectRefTag(Layout);
				return true;
			case "ruleid":
				value = new StringTag(RuleId.ToString());
				return true;
			case "outcome":
				value = new ObjectRefTag(Outcome);
				return true;
		}

		value = null;
		return false;
	}

	public bool TrySetMember(string member, DataTag value) => false;

	public bool TryGetIndex(DataTag index, out DataTag? value)
	{
		value = null;
		return false;
	}

	public bool TrySetIndex(DataTag index, DataTag value) => false;

	private static string Normalize(string value) => value.Replace("_", "").ToLowerInvariant();
}

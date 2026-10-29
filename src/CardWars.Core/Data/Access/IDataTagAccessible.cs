using CardWars.Core.Data.Tags;

namespace CardWars.Core.Data.Access;

public interface IDataTagAccessible
{
	bool TryGetMember(string member, out DataTag? value);
	bool TrySetMember(string member, DataTag value);
	bool TryGetIndex(DataTag index, out DataTag? value);
	bool TrySetIndex(DataTag index, DataTag value);
}

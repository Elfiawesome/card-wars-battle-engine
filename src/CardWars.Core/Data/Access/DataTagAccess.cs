using CardWars.Core.Data.Tags;

namespace CardWars.Core.Data.Access;

/// <summary>
/// TEMPORARY Phase 1 compatibility shim. Delegates every call to
/// <see cref="DataContext.Default"/> so existing callers keep working unchanged while the
/// data layer moves to per-instance <see cref="DataContext"/> ownership (doc.md §5, AGENT.md R1).
/// This static facade is slated for removal in a later phase once callers thread an owned
/// context through directly; the real logic lives in <see cref="DataTagAccessCore"/>.
/// </summary>
public static class DataTagAccess
{
	public static bool TryGetMember(object target, string member, out DataTag? value)
		=> DataContext.Default.TryGetMember(target, member, out value);

	public static bool TrySetMember(object target, string member, DataTag value)
		=> DataContext.Default.TrySetMember(target, member, value);

	public static bool TryGetIndex(object target, DataTag index, out DataTag? value)
		=> DataContext.Default.TryGetIndex(target, index, out value);

	public static bool TrySetIndex(object target, DataTag index, DataTag value)
		=> DataContext.Default.TrySetIndex(target, index, value);
}

using CardWars.Core.Data.Converters;
using CardWars.Core.Data.Tags;

namespace CardWars.Core.Data.Mapping;

/// <summary>
/// TEMPORARY Phase 1 compatibility shim. Delegates every call to
/// <see cref="DataContext.Default"/>, which now owns the converter map and the
/// polymorphic discriminator key as instance state (doc.md §5, AGENT.md R1). Kept so
/// existing callers keep compiling unchanged while the data layer migrates to per-instance
/// <see cref="DataContext"/> ownership; slated for removal in a later phase.
/// </summary>
public static class DataTagMapper
{
	/// <summary>
	/// The discriminator key used for polymorphic types. Defaulted to "_type".
	/// Proxies <see cref="DataContext.Default"/>'s key.
	/// </summary>
	public static string TypeDiscriminatorKey
	{
		get => DataContext.Default.TypeDiscriminatorKey;
		set => DataContext.Default.TypeDiscriminatorKey = value;
	}

	// ======================== PUBLIC SERIALIZATION ========================

	public static CompoundTag ToTag(object obj, bool includeType = true)
		=> DataContext.Default.ToTag(obj, includeType);

	public static CompoundTag ToTagAs<T>(T obj, bool includeType = true)
		=> DataContext.Default.ToTagAs(obj, includeType);

	public static CompoundTag ToTagAs(object obj, Type declaredType, bool includeType = true)
		=> DataContext.Default.ToTagAs(obj, declaredType, includeType);

	// ======================== PUBLIC DESERIALIZATION ========================

	public static T FromTag<T>(CompoundTag tag) => DataContext.Default.FromTag<T>(tag);

	public static object? FromTag(CompoundTag tag, Type targetType)
		=> DataContext.Default.FromTag(tag, targetType);

	// ======================== SINGLE VALUES ========================

	public static DataTag? ToValueTag(object? value, Type? declaredType = null)
		=> DataContext.Default.ToValueTag(value, declaredType);

	public static object? FromValueTag(DataTag? tag, Type targetType)
		=> DataContext.Default.FromValueTag(tag, targetType);

	// ======================== CONVERTERS ========================

	public static void RegisterConverter<T>(DataTagConverter<T> converter)
		=> DataContext.Default.RegisterConverter(converter);

	public static void RegisterConverter(Type type, IDataTagConverter converter)
		=> DataContext.Default.RegisterConverter(type, converter);

	public static bool HasConverter(Type type) => DataContext.Default.HasConverter(type);

	internal static bool TryGetConverter(Type type, out IDataTagConverter converter)
		=> DataContext.Default.TryGetConverter(type, out converter);
}

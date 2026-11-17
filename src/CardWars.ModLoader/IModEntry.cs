namespace CardWars.ModLoader;

/// <summary>
/// The single coherent base contract every mod entry point implements, whatever side it targets
/// (server, battle engine, client). It carries the mod's identity so the loader and host can
/// describe a loaded mod uniformly. The vanilla game is discovered and loaded through this same
/// mechanism as any third-party mod, with no privileged path (AGENT.md R5).
///
/// Implementations MUST return real values for <see cref="Name"/> and <see cref="Version"/> and
/// MUST NOT throw: these are plain identity accessors read during/after mod load, not optional
/// members. <see cref="Name"/> is the canonical identity member (matching
/// <see cref="IModManifest.Name"/>); the older <c>ModName</c> spelling is removed.
/// </summary>
public interface IModEntry
{
	string Name { get; }
	string Version { get; }
}

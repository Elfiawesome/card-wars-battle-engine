using CardWars.ModLoader;
using System.Collections.Generic;

namespace CardWars.Client.scripts.core;

// Phase 4 (R5): Name/Version are inherited from the unified IModEntry (the old client-specific
// ModName/Version declarations are removed so there is one coherent identity contract across
// server/battle-engine/client mods). Implementations must return real values and never throw.
// Phase 6: compiled/validated against Godot 4.7. Stays in the game assembly (not
// CardWars.Client.Core) because it references ClientRegistry/PacketContextClient, which are bound
// to the scene Node GameSession.
public interface IClientMod : IModEntry
{
	public void OnLoad(ClientRegistry registry, List<ModContentResult> modContents);
};

using CardWars.ModLoader;
using System.Collections.Generic;

namespace CardWars.Client.scripts.core;

// Phase 4 (R5): Name/Version are inherited from the unified IModEntry (the old client-specific
// ModName/Version declarations are removed so there is one coherent identity contract across
// server/battle-engine/client mods). Implementations must return real values and never throw.
// PROVISIONAL / UNCOMPILED: CardWars.Client targets Godot 4.7 which is absent in the engine
// sandbox, so this file is not compiled or verified here; validate on a Godot-equipped machine in
// Phase 6.
public interface IClientMod : IModEntry
{
	public void OnLoad(ClientRegistry registry, List<ModContentResult> modContents);
};

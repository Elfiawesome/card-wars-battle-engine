using CardWars.BattleEngine;
using CardWars.ModLoader;
using CardWars.Server;

Server server = new();

ModLoader ml = new(@"C:\Users\elfia\OneDrive\Desktop\CardWars\mods\");
ml.DiscoverMods();
ml.ResolveDependencies();
ml.LoadAssemblies();

ml.LoadModEntry<IBattleEngineMod>().ForEach((m) => server.BattleEngine.LoadMod(m));
ml.LoadModEntry<IServerMod>().ForEach((m) => server.LoadMod(m));
using CardWars.ModLoader;
using Godot;

namespace CardWars.Client.scripts.core.content;

public interface IContentAssetHandler
{
	Resource? Load(ModContentResult content);
}

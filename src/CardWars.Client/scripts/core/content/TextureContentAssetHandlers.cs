using System;
using CardWars.ModLoader;
using Godot;

namespace CardWars.Client.scripts.core.content;

public sealed class RasterTextureContentAssetHandler(Func<Image, byte[], Error> load) : IContentAssetHandler
{
	public Resource? Load(ModContentResult content)
	{
		var image = new Image();
		var error = load(image, content.ReadAllBytes());
		return error == Error.Ok ? ImageTexture.CreateFromImage(image) : null;
	}
}

public sealed class SvgTextureContentAssetHandler : IContentAssetHandler
{
	public Resource? Load(ModContentResult content)
	{
		var image = new Image();
		var error = image.LoadSvgFromString(content.ReadAllText());
		return error == Error.Ok ? ImageTexture.CreateFromImage(image) : null;
	}
}

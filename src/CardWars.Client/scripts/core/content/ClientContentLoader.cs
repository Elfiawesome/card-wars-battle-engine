using System.Collections.Generic;
using CardWars.Client.scripts.core.registry;
using CardWars.Client.scripts.core.storage;
using CardWars.Core.Logging;
using CardWars.Core.Registry;
using CardWars.ModLoader;
using Godot;

namespace CardWars.Client.scripts.core.content;

public sealed class ClientContentLoader(ResourceRegistry<ResourceId> resources)
{
	private const string AssetsRoot = "res://assets";

	private static readonly HashSet<string> SourceAssetExtensions = [".kra"];

	private readonly Registry<string, IContentAssetHandler> _assetHandlers = CreateAssetHandlers();

	private static Registry<string, IContentAssetHandler> CreateAssetHandlers()
	{
		var handlers = new Registry<string, IContentAssetHandler>();
		handlers.Register(".png", new RasterTextureContentAssetHandler((image, bytes) => image.LoadPngFromBuffer(bytes)));
		handlers.Register(".jpg", new RasterTextureContentAssetHandler((image, bytes) => image.LoadJpgFromBuffer(bytes)));
		handlers.Register(".jpeg", new RasterTextureContentAssetHandler((image, bytes) => image.LoadJpgFromBuffer(bytes)));
		handlers.Register(".svg", new SvgTextureContentAssetHandler());
		return handlers;
	}

	public void MountPacks(IEnumerable<ModContentResult> packs)
	{
		foreach (var pack in packs)
		{
			if (!ProjectSettings.LoadResourcePack(pack.FilePath.FullPath, true))
			{
				Log.Error($"Failed to mount client pack [{pack.Id}]");
				continue;
			}

			Log.Info($"Mounted client pack [{pack.Id}]");
		}
	}

	public void RegisterMountedAssets()
	{
		foreach (var item in ResourceLoader.ListDirectory(AssetsRoot))
		{
			if (!item.EndsWith('/')) continue;

			var modName = item.TrimEnd('/');
			foreach (var (resourceId, resourcePath) in new GDContentLoader(modName).LoadAssets())
			{
				resources.Register(resourceId, resourcePath);
				Log.Debug($"Registered mounted asset [{resourceId}]");
			}
		}
	}

	public void LoadRawAssets(IEnumerable<ModContentResult> assets)
	{
		foreach (var content in assets)
		{
			var extension = content.FilePath.Extension.ToLowerInvariant();
			var handler = _assetHandlers.Get(extension);
			if (handler is null)
			{
				if (SourceAssetExtensions.Contains(extension)) continue;
				Log.Warn($"No asset handler for [{content.Id}] ({extension})");
				continue;
			}

			var resource = handler.Load(content);
			if (resource is null)
			{
				Log.Warn($"Failed to load asset [{content.Id}]");
				continue;
			}

			resources.Register(content.Id, resource);
			Log.Debug($"Registered asset [{content.Id}]");
		}
	}
}

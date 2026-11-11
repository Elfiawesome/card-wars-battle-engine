using System.Collections.Generic;
using System.IO;
using System.Linq;
using CardWars.Core.Registry;
using Godot;

namespace CardWars.Client.scripts.core.storage;

public class GDContentLoader
{
	public GDContentLoader(string modName)
	{
		ModName = modName;
	}

	public string AssetsFolder => $"res://assets/{ModName}";
	public string ScenesFolder => $"res://scenes/{ModName}";

	public string ModName { get; private set; }
	public IEnumerable<(ResourceId, string)> LoadAssets()
		=> TraverseDir(AssetsFolder).Select(
			path => (
				new ResourceId(
					ModName,
					path.ParentDir + Path.GetFileNameWithoutExtension(path.FileName.GetFile())
				),
				path.RootPath + path.ParentDir + path.FileName
			)
		);

	private IEnumerable<TraversedPath> TraverseDir(string rootPath, string parentDir = "", string filePath = "")
	{
		if (!rootPath.EndsWith('/')) rootPath += '/';
		string[] items = ResourceLoader.ListDirectory(rootPath + parentDir);

		foreach (var item in items)
		{
			var fullPath = rootPath + parentDir + item;
			if (item.StartsWith('.')) continue;
			if (item.EndsWith('/'))
			{
				foreach (var i in TraverseDir(rootPath, parentDir + item, filePath))
				{
					yield return i;
				}

			}
			else
			{
				yield return new TraversedPath(rootPath, parentDir, item);
			}
		}
	}

	private record struct TraversedPath(string RootPath, string ParentDir, string FileName);
}
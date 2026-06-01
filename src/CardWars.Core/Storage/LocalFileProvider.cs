using System.IO;

namespace CardWars.Core.Storage;

public class LocalFileProvider : IFileProvider
{
	public bool Exists(string path) => File.Exists(path) || Directory.Exists(path);
	public bool IsFile(string path) => File.Exists(path);
	public bool IsDirectory(string path) => Directory.Exists(path);

	public Stream OpenRead(string path) => new FileStream(path, FileMode.Open, FileAccess.Read);
	public Stream OpenWrite(string path) => new FileStream(path, FileMode.Create, FileAccess.Write);

	public string ReadAllText(string path) => File.ReadAllText(path);
	public void WriteAllText(string path, string content) => File.WriteAllText(path, content);

	public void CreateDirectory(string path) => Directory.CreateDirectory(path);

	public void Delete(string path)
	{
		if (File.Exists(path))
			File.Delete(path);
		else if (Directory.Exists(path))
			Directory.Delete(path, true);
	}

	public string[] GetFiles(string directoryPath, string pattern = "*")
		=> Directory.Exists(directoryPath) ? Directory.GetFiles(directoryPath, pattern) : [];

	public string[] GetDirectories(string directoryPath)
		=> Directory.Exists(directoryPath) ? Directory.GetDirectories(directoryPath) : [];

	public IEnumerable<(string filePath, string relativePath)> Walk(string directoryPath)
		=> WalkRecursive(directoryPath, directoryPath);

	private IEnumerable<(string filePath, string relativePath)> WalkRecursive(string rootPath, string currentPath)
	{
		foreach (var file in GetFiles(currentPath))
			yield return (file, Path.GetRelativePath(rootPath, file));

		foreach (var dir in GetDirectories(currentPath))
		{
			foreach (var item in WalkRecursive(rootPath, dir))
				yield return item;
		}
	}

	public string Combine(string path1, string path2) => Path.Combine(path1, path2);
}

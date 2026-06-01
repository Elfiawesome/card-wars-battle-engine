namespace CardWars.Core.Storage;

public class StoragePath
{
	public string FullPath { get; }
	public string Name => System.IO.Path.GetFileName(FullPath);
	public string Extension => System.IO.Path.GetExtension(FullPath);
	public string[] Parts => FullPath.Split(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);

	private readonly IFileProvider _provider;

	public StoragePath(string fullPath, IFileProvider provider)
	{
		FullPath = System.IO.Path.GetFullPath(fullPath);
		_provider = provider;
	}

	public bool Exists => _provider.Exists(FullPath);
	public bool IsFile => _provider.IsFile(FullPath);
	public bool IsDirectory => _provider.IsDirectory(FullPath);

	public StoragePath Combine(string part)
		=> new(_provider.Combine(FullPath, part), _provider);

	public Stream OpenRead() => _provider.OpenRead(FullPath);
	public Stream OpenWrite() => _provider.OpenWrite(FullPath);
	public string ReadAllText() => _provider.ReadAllText(FullPath);
	public void WriteAllText(string content) => _provider.WriteAllText(FullPath, content);

	public void CreateDirectory() => _provider.CreateDirectory(FullPath);
	public void Delete() => _provider.Delete(FullPath);

	public StoragePath[] GetFiles(string pattern = "*")
		=> _provider.GetFiles(FullPath, pattern)
			.Select(f => new StoragePath(f, _provider))
			.ToArray();

	public StoragePath[] GetDirectories()
		=> _provider.GetDirectories(FullPath)
			.Select(d => new StoragePath(d, _provider))
			.ToArray();

	public IEnumerable<(StoragePath file, string relativePath)> Walk()
	{
		foreach (var (filePath, relPath) in _provider.Walk(FullPath))
			yield return (new StoragePath(filePath, _provider), relPath);
	}

	public override string ToString() => FullPath;
}

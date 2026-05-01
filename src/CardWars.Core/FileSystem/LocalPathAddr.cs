namespace CardWars.Core.FileSystem;

public class LocalPathAddr(string path) : IPathAddr
{
	public string Path { get; } = System.IO.Path.GetFullPath(path);
	public string Name => System.IO.Path.GetFileName(Path);
	public string Extension => System.IO.Path.GetExtension(Path);

	public IPathAddr Combine(params string[] parts)
		=> new LocalPathAddr(System.IO.Path.Combine([Path, .. parts]));

	public bool Exists => IsFile || IsDirectory;
	public bool IsDirectory => Directory.Exists(Path);
	public bool IsFile => File.Exists(Path);

	public IEnumerable<IPathAddr> GetFiles(string searchPattern = "*")
		=> IsDirectory ? Directory.GetFiles(Path, searchPattern).Select(f => new LocalPathAddr(f)) : [];

	public IEnumerable<IPathAddr> GetDirectories()
		=> IsDirectory ? Directory.GetDirectories(Path).Select(d => new LocalPathAddr(d)) : [];

	public void Delete()
	{
		if (IsFile) File.Delete(Path);
		else if (IsDirectory) Directory.Delete(Path, true);
	}

	public void CreateDirectory()
		=> Directory.CreateDirectory(Path);

	public Stream OpenRead()
		=> new FileStream(Path, FileMode.Open, FileAccess.Read);

	public Stream OpenWrite()
		=> new FileStream(Path, FileMode.Create, FileAccess.Write);

	public string ReadAllText() => File.ReadAllText(Path);
	public void WriteAllText(string content) => File.WriteAllText(Path, content);
}
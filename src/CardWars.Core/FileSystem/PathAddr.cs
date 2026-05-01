namespace CardWars.Core.FileSystem;

public class PathAddr(string root) : IPathAddr
{
	private readonly string _root = root;
	public string BasePath { get => _root; }


	public IPathAddr Combine(params string[] parts)
		=> new PathAddr(Path.Combine([BasePath, .. parts]));

	public bool FileExists()
		=> File.Exists(BasePath);

	public bool IsDirectory()
		=> Directory.Exists(BasePath);

	public bool IsFile()
		=> File.Exists(BasePath);

	public IEnumerable<IPathAddr> GetFiles()
		=> Directory.GetFiles(BasePath).Select(f => new PathAddr(f));

	public IEnumerable<IPathAddr> GetDirectories()
		=> Directory.GetDirectories(BasePath).Select(d => new PathAddr(d));

	public void Delete()
		=> File.Delete(BasePath);

	public void CreateDirectory()
		=> Directory.CreateDirectory(BasePath);

	public Stream OpenRead()
		=> new FileStream(BasePath, FileMode.Open, FileAccess.Read);

	public Stream OpenWrite()
		=> new FileStream(BasePath, FileMode.Open, FileAccess.Write);
}
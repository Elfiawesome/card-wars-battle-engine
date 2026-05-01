namespace CardWars.Core.FileSystem;

public interface IPathAddr
{
	public string BasePath { get; }

	public static IPathAddr operator /(IPathAddr path, string part) => path.Combine(part);
	public static IPathAddr operator /(IPathAddr path, IPathAddr part) => path.Combine(part);

	public string ToPath() => BasePath;
	public IPathAddr Combine(IPathAddr part) => Combine([part.ToPath()]);
	public IPathAddr Combine(string part) => Combine([part]);
	public IPathAddr Combine(params string[] parts);
	public bool FileExists();
	public bool IsFile();
	public bool IsDirectory();
	public IEnumerable<IPathAddr> GetFiles();
	public IEnumerable<IPathAddr> GetDirectories();
	public void Delete();
	public void CreateDirectory();
	public Stream OpenRead();
	public Stream OpenWrite();
}
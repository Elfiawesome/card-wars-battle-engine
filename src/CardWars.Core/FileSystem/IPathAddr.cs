namespace CardWars.Core.FileSystem;

public interface IPathAddr
{
	public string Path { get; }
	public string Name { get; }
	public string Extension { get; }

	public static IPathAddr operator /(IPathAddr path, string part) => path.Combine(part);
	public static IPathAddr operator /(IPathAddr path, IPathAddr part) => path.Combine(part);

	public string ToPath() => Path;

	public IPathAddr Combine(params string[] parts);
	public IPathAddr Combine(IPathAddr part) => Combine([part.ToPath()]);
	public IPathAddr Combine(string part) => Combine([part]);


	public bool Exists { get; }
	public bool IsFile { get; }
	public bool IsDirectory { get; }

	public IEnumerable<IPathAddr> GetFiles(string searchPattern = "*");
	public IEnumerable<IPathAddr> GetDirectories();

	public void Delete();
	public void CreateDirectory();

	public Stream OpenRead();
	public Stream OpenWrite();

	public string ReadAllText();
	public void WriteAllText(string content);
}
namespace CardWars.Core.FileSystem;

public class LocalFileSystem(string rootPath) : IFileSystem
{
	public IPathAddr Root { get; } = new LocalPathAddr(rootPath);
	public IPathAddr GetPath(string relativePath) => Root.Combine(relativePath);
}
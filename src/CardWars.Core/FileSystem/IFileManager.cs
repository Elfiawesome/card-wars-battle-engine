namespace CardWars.Core.FileSystem;

public interface IFileSystem
{
	public IPathAddr Root { get; }
	public IPathAddr GetPath(string relativePath);
}
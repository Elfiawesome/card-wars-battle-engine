namespace CardWars.Core.FileSystem;

public class FileSystem(string root) : IFileSystem
{
	public IPathAddr BasePath { get; } = new PathAddr(root);
}
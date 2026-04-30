namespace CardWars.Core.FileSystem;

public interface IFileSystem
{
	public string RootPath { get; set; }
	public bool FileExists(string path);
	public bool DirectoryExists(string path);
	public void CreateDirectory(string path);
	public Stream OpenRead(string path);
	public Stream OpenWrite(string path);
	public void DeleteFile(string path);
	public IEnumerable<string> GetFiles(string directory, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly);
	public IEnumerable<string> GetDirectories(string directory);
}
namespace CardWars.Core.Storage;

public class GodotFileProvider : IFileProvider
{
	public bool Exists(string path) => throw new NotImplementedException("GodotFileProvider is not yet implemented.");
	public bool IsFile(string path) => throw new NotImplementedException("GodotFileProvider is not yet implemented.");
	public bool IsDirectory(string path) => throw new NotImplementedException("GodotFileProvider is not yet implemented.");
	public Stream OpenRead(string path) => throw new NotImplementedException("GodotFileProvider is not yet implemented.");
	public Stream OpenWrite(string path) => throw new NotImplementedException("GodotFileProvider is not yet implemented.");
	public string ReadAllText(string path) => throw new NotImplementedException("GodotFileProvider is not yet implemented.");
	public void WriteAllText(string path, string content) => throw new NotImplementedException("GodotFileProvider is not yet implemented.");
	public void CreateDirectory(string path) => throw new NotImplementedException("GodotFileProvider is not yet implemented.");
	public void Delete(string path) => throw new NotImplementedException("GodotFileProvider is not yet implemented.");
	public string[] GetFiles(string directoryPath, string pattern = "*") => throw new NotImplementedException("GodotFileProvider is not yet implemented.");
	public string[] GetDirectories(string directoryPath) => throw new NotImplementedException("GodotFileProvider is not yet implemented.");
	public IEnumerable<(string filePath, string relativePath)> Walk(string directoryPath) => throw new NotImplementedException("GodotFileProvider is not yet implemented.");
	public string Combine(string path1, string path2) => System.IO.Path.Combine(path1, path2);
	public string GetFileName(string path) => throw new NotImplementedException("GodotFileProvider is not yet implemented.");
	public string GetExtension(string path) => throw new NotImplementedException("GodotFileProvider is not yet implemented.");
	public string GetFullPath(string path) => throw new NotImplementedException("GodotFileProvider is not yet implemented.");
	public string NormalizePath(string path) => throw new NotImplementedException("GodotFileProvider is not yet implemented.");
	public string GetDirectoryName(string path) => throw new NotImplementedException("GodotFileProvider is not yet implemented.");
	public string GetFileNameWithoutExtension(string path) => throw new NotImplementedException("GodotFileProvider is not yet implemented.");
}

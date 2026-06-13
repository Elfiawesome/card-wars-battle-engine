namespace CardWars.Core.Storage;

public interface IFileProvider
{
	bool Exists(string path);
	bool IsFile(string path);
	bool IsDirectory(string path);
	Stream OpenRead(string path);
	Stream OpenWrite(string path);
	string ReadAllText(string path);
	void WriteAllText(string path, string content);
	void CreateDirectory(string path);
	void Delete(string path);
	string[] GetFiles(string directoryPath, string pattern = "*");
	string[] GetDirectories(string directoryPath);
	IEnumerable<(string filePath, string relativePath)> Walk(string directoryPath);
	string Combine(string path1, string path2);

	string GetFileName(string path);
	string GetExtension(string path);
	string GetFullPath(string path);
	string NormalizePath(string path);
	string GetDirectoryName(string path);
	string GetFileNameWithoutExtension(string path);
	string WithExtension(string path, string extension);

	static char PathSeparator { get; }
	string[] SplitPath(string path);
	string JoinPath(params string[] parts);
}

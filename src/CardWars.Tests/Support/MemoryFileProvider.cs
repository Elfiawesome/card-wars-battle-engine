using System.Text;
using CardWars.Core.Storage;

namespace CardWars.Tests.Support;

internal sealed class MemoryFileProvider : IFileProvider
{
	private readonly Dictionary<string, string> _files = new(StringComparer.OrdinalIgnoreCase);

	public static char PathSeparator => '/';

	public void AddFile(string path, string content) => _files[NormalizePath(path)] = content;

	public bool Exists(string path) => _files.ContainsKey(NormalizePath(path));

	public bool IsFile(string path) => Exists(path);

	public bool IsDirectory(string path)
	{
		var prefix = NormalizePath(path).TrimEnd(PathSeparator) + PathSeparator;
		return _files.Keys.Any(file => file.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
	}

	public Stream OpenRead(string path) => new MemoryStream(Encoding.UTF8.GetBytes(ReadAllText(path)));

	public Stream OpenWrite(string path) => new MemoryStream();

	public string ReadAllText(string path) => _files[NormalizePath(path)];

	public void WriteAllText(string path, string content) => _files[NormalizePath(path)] = content;

	public void CreateDirectory(string path)
	{
	}

	public void Delete(string path) => _files.Remove(NormalizePath(path));

	public string[] GetFiles(string directoryPath, string pattern = "*")
	{
		var prefix = NormalizePath(directoryPath).TrimEnd(PathSeparator) + PathSeparator;
		return _files.Keys
			.Where(file => file.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
			.Where(file => !file[prefix.Length..].Contains(PathSeparator))
			.ToArray();
	}

	public string[] GetDirectories(string directoryPath) => [];

	public IEnumerable<(string filePath, string relativePath)> Walk(string directoryPath)
	{
		var prefix = NormalizePath(directoryPath).TrimEnd(PathSeparator) + PathSeparator;
		foreach (var file in _files.Keys.Where(file => file.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
			yield return (file, file[prefix.Length..]);
	}

	public string Combine(string path1, string path2)
		=> $"{path1.TrimEnd(PathSeparator)}{PathSeparator}{path2.TrimStart(PathSeparator)}";

	public string GetFileName(string path) => path[(path.LastIndexOf(PathSeparator) + 1)..];

	public string GetExtension(string path)
	{
		var name = GetFileName(path);
		var index = name.LastIndexOf('.');
		return index >= 0 ? name[index..] : "";
	}

	public string GetFullPath(string path) => NormalizePath(path);

	public string NormalizePath(string path) => path.Replace('\\', PathSeparator);

	public string GetDirectoryName(string path)
	{
		var normalized = NormalizePath(path);
		var index = normalized.LastIndexOf(PathSeparator);
		return index >= 0 ? normalized[..index] : "";
	}

	public string GetFileNameWithoutExtension(string path)
	{
		var name = GetFileName(path);
		var index = name.LastIndexOf('.');
		return index >= 0 ? name[..index] : name;
	}

	public string WithExtension(string path, string extension)
	{
		var withoutExtension = path[..^GetExtension(path).Length];
		return NormalizePath(withoutExtension + extension);
	}

	public string[] SplitPath(string path) => NormalizePath(path).Trim(PathSeparator).Split(PathSeparator);

	public string JoinPath(params string[] parts) => string.Join(PathSeparator, parts.Where(part => part.Length > 0));
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CardWars.Core.Storage;

namespace CardWars.Client.scripts.core.storage;

public class GDFileProvider : IFileProvider
{
	public static char PathSeparator => '/';

	public bool Exists(string path)
	{
		var norm = NormalizePath(path);
		return Godot.FileAccess.FileExists(norm) || Godot.DirAccess.DirExistsAbsolute(norm);
	}

	public bool IsFile(string path)
		=> Godot.FileAccess.FileExists(NormalizePath(path));

	public bool IsDirectory(string path)
		=> Godot.DirAccess.DirExistsAbsolute(NormalizePath(path));

	public Stream OpenRead(string path)
	{
		var norm = NormalizePath(path);
		using var file = Godot.FileAccess.Open(norm, Godot.FileAccess.ModeFlags.Read)
			?? throw new FileNotFoundException($"Cannot open file for read: {norm} (Godot Error: {Godot.FileAccess.GetOpenError()})");
		var length = (int)file.GetLength();
		var buffer = file.GetBuffer(length);
		return new MemoryStream(buffer, writable: false);
	}

	public Stream OpenWrite(string path)
	{
		var norm = NormalizePath(path);
		// Ensure parent directory exists before writing
		var parent = GetDirectoryName(norm);
		if (!string.IsNullOrEmpty(parent) && !Godot.DirAccess.DirExistsAbsolute(parent))
		{
			CreateDirectory(parent);
		}

		return new WriteBackMemoryStream(bytes =>
		{
			using var file = Godot.FileAccess.Open(norm, Godot.FileAccess.ModeFlags.Write)
				?? throw new IOException($"Failed to open file for writing: {norm} (Godot Error: {Godot.FileAccess.GetOpenError()})");
			file.StoreBuffer(bytes);
		});
	}

	public string ReadAllText(string path)
	{
		var norm = NormalizePath(path);
		using var file = Godot.FileAccess.Open(norm, Godot.FileAccess.ModeFlags.Read)
			?? throw new FileNotFoundException($"File not found: {norm} (Godot Error: {Godot.FileAccess.GetOpenError()})");
		return file.GetAsText();
	}

	public void WriteAllText(string path, string content)
	{
		var norm = NormalizePath(path);
		var parent = GetDirectoryName(norm);
		if (!string.IsNullOrEmpty(parent) && !Godot.DirAccess.DirExistsAbsolute(parent))
		{
			CreateDirectory(parent);
		}

		using var file = Godot.FileAccess.Open(norm, Godot.FileAccess.ModeFlags.Write)
			?? throw new IOException($"Failed to open file for write: {norm} (Godot Error: {Godot.FileAccess.GetOpenError()})");
		file.StoreString(content);
	}

	public void CreateDirectory(string path)
	{
		var norm = NormalizePath(path);
		if (Godot.DirAccess.DirExistsAbsolute(norm)) return;

		var err = Godot.DirAccess.MakeDirRecursiveAbsolute(norm);
		if (err != Godot.Error.Ok)
			throw new IOException($"Failed to create directory '{norm}': {err}");
	}

	public void Delete(string path)
	{
		var norm = NormalizePath(path);
		if (Godot.FileAccess.FileExists(norm))
		{
			using var dir = Godot.DirAccess.Open(GetDirectoryName(norm));
			dir?.Remove(GetFileName(norm));
		}
		else if (Godot.DirAccess.DirExistsAbsolute(norm))
		{
			DeleteDirectoryRecursive(norm);
		}
	}

	private void DeleteDirectoryRecursive(string path)
	{
		foreach (var file in GetFiles(path))
		{
			using var dir = Godot.DirAccess.Open(GetDirectoryName(file));
			dir?.Remove(GetFileName(file));
		}

		foreach (var subDir in GetDirectories(path))
		{
			DeleteDirectoryRecursive(subDir);
		}

		using var parentDir = Godot.DirAccess.Open(GetDirectoryName(path));
		parentDir?.Remove(GetFileName(path));
	}

	public string[] GetFiles(string directoryPath, string pattern = "*")
	{
		var norm = NormalizePath(directoryPath);
		if (!Godot.DirAccess.DirExistsAbsolute(norm)) return [];

		var files = Godot.DirAccess.GetFilesAt(norm);
		var regex = WildcardRegex(pattern);

		return files
			.Where(f => !f.EndsWith(".import") && regex.IsMatch(f))
			.Select(f => Combine(norm, f))
			.ToArray();
	}

	public string[] GetDirectories(string directoryPath)
	{
		var norm = NormalizePath(directoryPath);
		if (!Godot.DirAccess.DirExistsAbsolute(norm)) return [];

		return Godot.DirAccess.GetDirectoriesAt(norm)
			.Select(d => Combine(norm, d))
			.ToArray();
	}

	public IEnumerable<(string filePath, string relativePath)> Walk(string directoryPath)
	{
		var root = NormalizePath(directoryPath);
		return WalkInternal(root, root);
	}

	private IEnumerable<(string filePath, string relativePath)> WalkInternal(string rootPath, string currentPath)
	{
		foreach (var file in GetFiles(currentPath))
		{
			var rel = file[rootPath.Length..].TrimStart(PathSeparator);
			yield return (file, rel);
		}

		foreach (var dir in GetDirectories(currentPath))
		{
			foreach (var item in WalkInternal(rootPath, dir))
				yield return item;
		}
	}

	public string Combine(string path1, string path2)
	{
		if (string.IsNullOrEmpty(path1)) return NormalizePath(path2);
		if (string.IsNullOrEmpty(path2)) return NormalizePath(path1);

		var p1 = NormalizePath(path1).TrimEnd(PathSeparator);
		var p2 = NormalizePath(path2).TrimStart(PathSeparator);
		return $"{p1}/{p2}";
	}

	public string GetFileName(string path)
	{
		var norm = NormalizePath(path).TrimEnd(PathSeparator);
		var idx = norm.LastIndexOf(PathSeparator);
		return idx >= 0 ? norm[(idx + 1)..] : norm;
	}

	public string GetExtension(string path)
	{
		var name = GetFileName(path);
		var idx = name.LastIndexOf('.');
		return idx >= 0 ? name[idx..] : string.Empty;
	}

	public string GetFullPath(string path)
	{
		var norm = NormalizePath(path);
		if (norm.StartsWith("res://") || norm.StartsWith("user://"))
			return norm;

		return Godot.ProjectSettings.GlobalizePath(norm);
	}

	public string NormalizePath(string path)
	{
		if (string.IsNullOrEmpty(path)) return string.Empty;

		// Replace Windows backslashes with Godot standard forward slashes
		var replaced = path.Replace('\\', PathSeparator);

		// Handle double forward slashes while preserving uri schemes (res://, user://)
		if (replaced.StartsWith("res://"))
			return "res://" + Regex.Replace(replaced[6..], "/{2,}", "/").TrimEnd(PathSeparator);
		if (replaced.StartsWith("user://"))
			return "user://" + Regex.Replace(replaced[7..], "/{2,}", "/").TrimEnd(PathSeparator);

		return Regex.Replace(replaced, "/{2,}", "/").TrimEnd(PathSeparator);
	}

	public string GetDirectoryName(string path)
	{
		var norm = NormalizePath(path);
		if (norm is "res://" or "user://" or "") return string.Empty;

		var idx = norm.LastIndexOf(PathSeparator);
		if (idx < 0) return string.Empty;

		// Preserve "res://" or "user://" if the directory is the root
		if (idx == 5 && norm.StartsWith("res://")) return "res://";
		if (idx == 6 && norm.StartsWith("user://")) return "user://";

		return norm[..idx];
	}

	public string GetFileNameWithoutExtension(string path)
	{
		var fileName = GetFileName(path);
		var idx = fileName.LastIndexOf('.');
		return idx >= 0 ? fileName[..idx] : fileName;
	}

	public string WithExtension(string path, string extension)
	{
		var ext = extension.StartsWith('.') ? extension : "." + extension;
		var currentExt = GetExtension(path);
		var without = currentExt.Length > 0 ? path[..^currentExt.Length] : path;
		return without + ext;
	}

	public string[] SplitPath(string path)
	{
		var norm = NormalizePath(path);
		string prefix = "";
		if (norm.StartsWith("res://"))
		{
			prefix = "res://";
			norm = norm[6..];
		}
		else if (norm.StartsWith("user://"))
		{
			prefix = "user://";
			norm = norm[7..];
		}

		var parts = norm.Split(PathSeparator, StringSplitOptions.RemoveEmptyEntries);
		if (prefix.Length > 0 && parts.Length > 0)
		{
			parts[0] = prefix + parts[0];
		}
		return parts;
	}

	public string JoinPath(params string[] parts)
		=> string.Join(PathSeparator, parts.Select(NormalizePath).Where(p => p.Length > 0));

	private static Regex WildcardRegex(string pattern)
		=> new("^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$",
			RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

	private sealed class WriteBackMemoryStream(Action<byte[]> onDispose) : MemoryStream
	{
		private bool _disposed;

		protected override void Dispose(bool disposing)
		{
			if (!_disposed && disposing)
			{
				_disposed = true;
				onDispose(ToArray());
			}
			base.Dispose(disposing);
		}
	}
}
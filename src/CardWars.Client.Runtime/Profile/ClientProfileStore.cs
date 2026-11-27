using System;
using System.Text;
using CardWars.Core.Data;
using CardWars.Core.Data.Tags;
using CardWars.Core.Logging;
using CardWars.Core.Storage;
using CardWars.Vanilla.Shared;

namespace CardWars.Client.Runtime.Profile;

public sealed class ClientProfileStore
{
	private readonly StoragePath _root;
	private readonly DataTagContext _dataTags;
	private readonly Logger _logger;

	public ClientProfileStore(StoragePath root, DataTagContext dataTags, Logger logger)
	{
		_root = root;
		_dataTags = dataTags;
		_logger = logger;
		if (!_root.Exists) _root.CreateDirectory();
	}

	// TODO: For now we do profiles by username but later on username is independent from the profile....
	public PlayerProfile? Load(string username)
	{
		var path = GetPath(username);
		if (!path.Exists) return null;

		try
		{
			var tag = _dataTags.Deserialize<CompoundTag>(path.ReadAllText());
			if (tag is null)
			{
				_logger.Warn($"Client profile [{username}] could not be read");
				return null;
			}

			return _dataTags.FromTag<PlayerProfile>(tag);
		}
		catch (Exception ex)
		{
			_logger.Warn($"Client profile [{username}] could not be loaded: {ex.Message}");
			return null;
		}
	}

	public void Save(string username, PlayerProfile profile)
	{
		try
		{
			var json = _dataTags.Serialize(_dataTags.ToTag(profile));
			GetPath(username).WriteAllText(json);
		}
		catch (Exception ex)
		{
			_logger.Error($"Client profile [{username}] could not be saved: {ex.Message}");
		}
	}

	private StoragePath GetPath(string username) => _root.Combine(Sanitize(username) + ".json");

	private static string Sanitize(string username)
	{
		var builder = new StringBuilder(username.Length);

		foreach (var character in username)
			builder.Append(char.IsLetterOrDigit(character) || character is '_' or '-' or '.' ? character : '_');

		return builder.ToString();
	}
}

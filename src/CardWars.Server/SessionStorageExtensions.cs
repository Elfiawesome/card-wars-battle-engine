using CardWars.Core.Data;
using CardWars.Core.Storage;

namespace CardWars.Server;

public static class SessionStorageExtensions
{
	public static DataTag? LoadPlayer(this SessionStorage session, Guid playerId)
		=> session.LoadData(session.PlayersDir.Combine($"{playerId}").WithExtension("json"));

	public static void SavePlayer(this SessionStorage session, Guid playerId, CompoundTag data)
		=> session.SaveData(session.PlayersDir.Combine($"{playerId}").WithExtension("json"), data);

	public static void SaveUsernameMapping(this SessionStorage session, string username, Guid id)
	{
		var usernames = LoadUsernames(session);
		usernames.Set(username, id);
		session.SaveData(session.UsernamesFile, usernames);
	}

	public static Guid? UsernameToPlayerId(this SessionStorage session, string username)
	{
		var file = LoadUsernames(session);
		if (!file.Has(username)) return null;
		return file.GetGuid(username);
	}

	public static DataTag? LoadWorldSave(this SessionStorage session, Guid instanceId)
		=> session.LoadData(session.WorldsDir.Combine($"{instanceId}").WithExtension("json"));

	public static void SaveWorldSave(this SessionStorage session, Guid instanceId, DataTag data)
		=> session.SaveData(session.WorldsDir.Combine($"{instanceId}").WithExtension("json"), data);

	private static CompoundTag LoadUsernames(SessionStorage session)
	{
		if (session.UsernamesFile.Exists)
			return DataTagSerializer.Deserialize<CompoundTag>(session.UsernamesFile.ReadAllText()) ?? new CompoundTag();
		return new CompoundTag();
	}
}

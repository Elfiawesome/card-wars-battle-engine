using CardWars.Core.Data;

namespace CardWars.Core.Persistance;

public interface ISessionStorage
{
	public T Get<T>(string path) where T : DataTag => Get<T>([path]);
	public T Get<T>(string[] path) where T : DataTag;

	public Stream Get(string path) => Get(path);
	public Stream Get(string[] path);
}
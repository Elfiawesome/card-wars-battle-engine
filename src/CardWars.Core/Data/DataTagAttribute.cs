namespace CardWars.Core.Data;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class DataTagAttribute : Attribute
{
	public string? Key { get; }
	public bool Ignore { get; set; } = false;

	public DataTagAttribute() { Key = null; }
	public DataTagAttribute(string key) { Key = key; }
}
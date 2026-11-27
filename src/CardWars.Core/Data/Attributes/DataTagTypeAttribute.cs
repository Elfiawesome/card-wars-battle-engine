namespace CardWars.Core.Data.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class DataTagTypeAttribute : Attribute
{
	public DataTagTypeAttribute(string id) => Id = id;
	public DataTagTypeAttribute() { }
	public string? Id { get; }
}
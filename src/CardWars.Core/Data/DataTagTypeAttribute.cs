namespace CardWars.Core.Data;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class DataTagTypeAttribute(string id) : Attribute
{
	public string Id { get; } = id;
}
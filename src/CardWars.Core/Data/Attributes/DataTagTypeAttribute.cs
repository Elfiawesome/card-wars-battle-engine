using CardWars.Core.Registry;

namespace CardWars.Core.Data.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class DataTagTypeAttribute : Attribute
{
	public DataTagTypeAttribute(string resourceId) { Id = ResourceId.Parse(resourceId); }
	public DataTagTypeAttribute() { }
	public ResourceId? Id { get; } = null;
}
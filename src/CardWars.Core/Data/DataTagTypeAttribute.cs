using CardWars.Core.Registry;

namespace CardWars.Core.Data;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class DataTagTypeAttribute : Attribute
{
	public DataTagTypeAttribute(ResourceId id) { Id = id; }
	public DataTagTypeAttribute() { }
	public ResourceId? Id { get; } = null;
}
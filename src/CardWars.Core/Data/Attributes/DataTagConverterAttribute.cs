namespace CardWars.Core.Data.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class DataTagConverterAttribute(Type converterType) : Attribute
{
	public Type ConverterType { get; } = converterType;
}
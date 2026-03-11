namespace CardWars.Core.Data;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class DataTagConverterAttribute(Type converterType) : Attribute
{
	public Type ConverterType { get; } = converterType;
}
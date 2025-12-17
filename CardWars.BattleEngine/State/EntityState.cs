namespace CardWars.BattleEngine.State;

public abstract class EntityState<TId>
	where TId : IStateId
{
	public readonly TId Id;

	public EntityState(TId id)
	{
		Id = id;
	}

	public T? GetStat<T>(string name)
	{
		foreach (var property in GetType().GetProperties())
		{
			foreach (var attribute in Attribute.GetCustomAttributes(property))
			{
				if (attribute is EntityStatMappingAttribute entityStatMappingAttribute)
				{
					if (entityStatMappingAttribute.Name == name)
					{
						var value = property.GetValue(this);
						if (value is T specificValue)
						{
							return specificValue;
						}
					}
				}
			}
		}
		return default;
	}
}

public interface IStateId
{
	public Guid Id { get; set; }
}


[AttributeUsage(AttributeTargets.Property)]
public class EntityStatMappingAttribute(string Name) : Attribute
{
	public string Name { get; set; } = Name;
}
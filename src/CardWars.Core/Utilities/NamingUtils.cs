using System.Text;

namespace CardWars.Core.Utilities;

public static class NamingUtils
{
	public static string ToSnakeCase(string name)
	{
		if (string.IsNullOrEmpty(name)) return name;

		var sb = new StringBuilder(name.Length + 4);
		for (int i = 0; i < name.Length; i++)
		{
			var c = name[i];
			if (i > 0 && char.IsUpper(c))
			{
				bool prevIsLower = char.IsLower(name[i - 1]);
				bool nextIsLower = i + 1 < name.Length && char.IsLower(name[i + 1]);
				if (prevIsLower || nextIsLower)
					sb.Append('_');
			}
			sb.Append(char.ToLowerInvariant(c));
		}
		return sb.ToString();
	}
}
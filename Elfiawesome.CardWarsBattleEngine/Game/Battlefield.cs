using System.Numerics;

namespace Elfiawesome.CardWarsBattleEngine.Game;

public class Battlefield
{
	public enum ArrangementType
	{
		Centered = 0, // Default: Meaning that the items are arranged by rows
		TrueGrid
	}

	public readonly Guid Id;
	private readonly Dictionary<Vector2, UnitSlot> _grid = [];
	public IReadOnlyDictionary<Vector2, UnitSlot> Grid => _grid;
	private ArrangementType _arrangementType = ArrangementType.Centered;

	public Battlefield(Guid id, int width, int height)
	{
		Id = id;

		for (var x = 0; x < width; x++)
		{
			for (var y = 0; y < height; y++)
			{
				_grid[new Vector2(x, y)] = new UnitSlot();
			}
		}
	}
}
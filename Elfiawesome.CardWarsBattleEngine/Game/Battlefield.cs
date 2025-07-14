using System.Numerics;

namespace Elfiawesome.CardWarsBattleEngine.Game;

public class Battlefield
{
	// By default Battlefields should look like this
	// X X X
	//   X
	// But in some scenarios, we may want custom design/layout for the battlefield
	// X X X
	// X   X
	//   X
	// And mid game, we might want to add new slots in the battlefield, so it can extend on the sides the top etc (where Y is the new slots)
	// Y X X X Y
	//   Y X Y
	// or (in not symmetry way too)
	// X X X Y
	// Y X

	public readonly Guid Id;
	public IReadOnlyDictionary<BattlefieldPos, UnitSlot> Grid => _grid;

	private readonly Dictionary<BattlefieldPos, UnitSlot> _grid = [];
	private static readonly BattlefieldPos[] _defaultLayout = [
		new(0, 0), new(1, 0), new(2, 0),
		new(1, 1)
	];

	public Battlefield(Guid id)
	{
		Id = id;
	}

	public void InitializeGrid()
	{
		foreach (var pos in _defaultLayout)
		{
			_grid.TryAdd(pos, new UnitSlot());
		}
	}
}

public readonly record struct BattlefieldPos(int X, int Y);
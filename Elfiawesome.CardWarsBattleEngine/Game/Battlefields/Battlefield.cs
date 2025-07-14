using Elfiawesome.CardWarsBattleEngine.Game.UnitSlots;

namespace Elfiawesome.CardWarsBattleEngine.Game.Battlefields;

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

	public readonly BattlefieldId Id;
	public IReadOnlyDictionary<UnitSlotPos, UnitSlot> Grid => _grid;

	private readonly Dictionary<UnitSlotPos, UnitSlot> _grid = [];
	private static readonly UnitSlotPos[] _defaultLayout = [
		new(0, 0), new(1, 0), new(2, 0),
		new(1, 1)
	];

	public Battlefield(BattlefieldId id)
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
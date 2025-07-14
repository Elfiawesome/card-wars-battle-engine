namespace Elfiawesome.CardWarsBattleEngine.Game.Entities;

public class Battlefield : Entity<BattlefieldId>
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

	public IReadOnlyDictionary<UnitSlotPos, UnitSlot> Grid => _grid;
	public static readonly UnitSlotPos[] DefaultLayout = [
		new(0, 0), new(1, 0), new(2, 0),
		new(1, 1)
	];

	private readonly Dictionary<UnitSlotPos, UnitSlot> _grid = [];

	public Battlefield(BattlefieldId id) : base(id)
	{
		
	}

	public void AddUnitSlot(UnitSlotPos position)
	{
		_grid[position] = new UnitSlot(position);
	}
}

public readonly record struct BattlefieldId(long Id);
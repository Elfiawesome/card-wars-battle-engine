using System.Collections.Generic;
using CardWars.BattleEngine.State;
using Godot;

namespace CardWars.Client;

public partial class Battlefield : Node3D
{
	public Dictionary<EntityId, UnitSlot> unitSlots = [];
	public void AddUnitSlot(EntityId entityId, UnitSlot unitSlot)
	{
		AddChild(unitSlot);
		unitSlots.Add(entityId, unitSlot);
	}

	public void ModifyUnitSlotPosition(EntityId entityId, Vector2 pos)
	{
		unitSlots[entityId].Position = new Vector3(pos.X * 2, pos.Y * 2, 0);
	}
}

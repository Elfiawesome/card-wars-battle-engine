using Godot;

namespace CardWars.Client.scenes.vanilla.instance.battle;

public partial class BattlefieldNode : EntityNode3D
{
	public float Width { get; private set; } = UnitSlotNode.CellWidth;
	public float Depth { get; private set; } = UnitSlotNode.CellDepth;
	public Vector3 BoundsCenter { get; private set; } = Vector3.Zero;

	private Area3D? _collisionBox;
	private CollisionShape3D? _collisionBoxShape;


	public float Radius => Mathf.Sqrt(Width * Width + Depth * Depth) * 0.5f;

	public override void _Ready()
	{
		_collisionBox = GetNode<Area3D>("Area3D");
		_collisionBoxShape = GetNode<CollisionShape3D>("Area3D/CollisionShape3D");
		// _collisionBox.InputEvent += onCollisionInputEvent;
		_collisionBox.MouseEntered += () => { OnMouseEntered?.Invoke(); };
		_collisionBox.MouseExited += () => { OnMouseExited?.Invoke(); };
	}


	public void RefreshBounds()
	{
		var minX = float.PositiveInfinity;
		var maxX = float.NegativeInfinity;
		var minZ = float.PositiveInfinity;
		var maxZ = float.NegativeInfinity;
		var any = false;

		foreach (var child in GetChildren())
		{
			if (child is not UnitSlotNode slot) continue;

			var position = slot.TargetPosition;
			minX = Mathf.Min(minX, position.X - UnitSlotNode.CellWidth * 0.5f);
			maxX = Mathf.Max(maxX, position.X + UnitSlotNode.CellWidth * 0.5f);
			minZ = Mathf.Min(minZ, position.Z - UnitSlotNode.CellDepth * 0.5f);
			maxZ = Mathf.Max(maxZ, position.Z + UnitSlotNode.CellDepth * 0.5f);
			any = true;
		}

		if (!any)
		{
			Width = UnitSlotNode.CellWidth;
			Depth = UnitSlotNode.CellDepth;
			BoundsCenter = Vector3.Zero;
			return;
		}

		Width = maxX - minX;
		Depth = maxZ - minZ;
		BoundsCenter = new Vector3((minX + maxX) * 0.5f, 0f, (minZ + maxZ) * 0.5f);

		// How to amke this work?
		if (_collisionBoxShape is not null)
		{
			_collisionBoxShape.Shape ??= new BoxShape3D();
			if (_collisionBoxShape.Shape is BoxShape3D boxShape3D)
			{
				boxShape3D.Size = new Vector3(Width, 0.05f, Depth);
				_collisionBoxShape.Position = BoundsCenter - new Vector3(0, 0.01f, 0);
			}
		}
	}

	public void PlaceAt(Vector3 targetCenter, float facing)
	{
		Position = targetCenter - BoundsCenter.Rotated(Vector3.Up, facing);
		Rotation = new Vector3(0f, facing, 0f);
	}
}

using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Logging;
using CardWars.Core.Registry;
using Godot;

namespace CardWars.Client.scenes.vanilla.instance.battle;

public partial class Card3D : EntityNode3D
{
	public SubViewport? CardViewport => GetNode<SubViewport>("CardViewport");
	public Area3D? CollisionBox => GetNode<Area3D>("Area3D");
	public CollisionShape3D? CollisionShape => GetNode<CollisionShape3D>("Area3D/CollisionShape3D");
	public BoxShape3D BoxShape = new();
	public CardDisplay? Display;
	public ResourceId DisplayId { get; private set; }

	public override void _Ready()
	{
		if (CollisionBox is not null)
		{
			CollisionBox.Visible = false;
			CollisionBox.InputRayPickable = false;
			CollisionBox.InputEvent += OnTargetInput;
			CollisionBox.MouseEntered += () => OnMouseEntered?.Invoke();
			CollisionBox.MouseExited += () => OnMouseExited?.Invoke();
		}
		if (CollisionShape is not null)
		{
			CollisionShape.Shape = BoxShape;
		}
	}

	public void SetHitBox(bool value)
	{
		if (CollisionBox is null) return;

		CollisionBox.Visible = value;
		CollisionBox.InputRayPickable = value;
	}

	public void SetCard(GenericCard card)
	{
		if (Display is { } display)
		{
			display.SetData(card.Data);
			BoxShape.Size = new Vector3(display.Size.X, 100.0f, display.Size.Y) / 100f;
			if (CardViewport is not null) CardViewport.Size = (Vector2I)display.Size;
		}
	}

	public void SetCardDisplay(ResourceId cardDisplayId, CardDisplay cardDisplay)
	{
		if (cardDisplayId == DisplayId) return;
		if (Display is not null) { CardViewport?.RemoveChild(Display); Display.QueueFree(); }

		Display = cardDisplay;
		DisplayId = cardDisplayId;
		CardViewport?.AddChild(Display);
	}

	public override HighlightBounds? GetHighlightBounds()
	{
		var width = BoxShape.Size.X * CollisionBox?.Scale.X ?? 0.0f;
		var height = BoxShape.Size.Z * CollisionBox?.Scale.Z ?? 0.0f;
		return new HighlightBounds(
			new Vector3(0f, 0.01f, 0f),
			new Vector2(width, height));
	}

	private void OnTargetInput(Node cameraNode, InputEvent @event, Vector3 eventPosition, Vector3 normal, long shapeIndex)
	{
		if (@event is not InputEventMouseButton inputEvent
			|| !inputEvent.Pressed
			|| inputEvent.ButtonIndex != MouseButton.Left) return;

		OnMouseClicked?.Invoke();
	}
}

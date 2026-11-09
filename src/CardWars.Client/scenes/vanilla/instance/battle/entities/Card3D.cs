using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Registry;
using Godot;

namespace CardWars.Client.scenes.vanilla.instance.battle;

public partial class Card3D : EntityNode3D
{
	private Area3D? _collisionBox;
	private CollisionShape3D? _collisionBoxShape;

	public SubViewport? CardViewport => GetNode<SubViewport>("CardViewport");
	public CardDisplay? Display;
	public ResourceId DisplayId { get; private set; }

	public override void _Ready()
	{
		_collisionBox = GetNode<Area3D>("Area3D");
		_collisionBoxShape = GetNode<CollisionShape3D>("Area3D/CollisionShape3D");

		_collisionBox.Visible = false;
		_collisionBox.InputRayPickable = false;
		_collisionBox.InputEvent += OnTargetInput;
		_collisionBox.MouseEntered += () => OnMouseEntered?.Invoke();
		_collisionBox.MouseExited += () => OnMouseExited?.Invoke();
	}

	public void SetHitBox(bool value)
	{
		if (_collisionBox is null) return;

		_collisionBox.Visible = value;
		_collisionBox.InputRayPickable = value;
	}

	public void SetCard(GenericCard card)
	{
		if (Display is { } display)
			display.SetData(card.Data);
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
		if (GetData<GenericCard>() is not { IsHero: true }) return null;

		const float cardWidth = 0.82f;
		const float cardHeight = 1.1f;
		var angle = Rotation.Y;
		var sin = Mathf.Abs(Mathf.Sin(angle));
		var cos = Mathf.Abs(Mathf.Cos(angle));

		var width = cardWidth * cos + cardHeight * sin;
		var height = cardWidth * sin + cardHeight * cos;

		return new HighlightBounds(
			new Vector3(0f, 0.01f, 0f),
			new Vector2(width, height),
			-angle);
	}

	private void OnTargetInput(Node cameraNode, InputEvent @event, Vector3 eventPosition, Vector3 normal, long shapeIndex)
	{
		if (@event is not InputEventMouseButton inputEvent
			|| !inputEvent.Pressed
			|| inputEvent.ButtonIndex != MouseButton.Left) return;

		OnMouseClicked?.Invoke();
	}
}

using CardWars.BattleEngine.Vanilla.Entity;
using Godot;

namespace CardWars.Client.scenes.vanilla.instance.battle;

public partial class Card3D : EntityNode3D
{
	private Area3D? _collisionBox;
	private CollisionShape3D? _collisionBoxShape;

	public CardDisplay? Display => GetNodeOrNull<CardDisplay>("SubViewport/CardDisplay");

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

	public override HighlightBounds? GetHighlightBounds()
		=> GetData<GenericCard>() is { IsHero: true }
			? new(new Vector3(0f, 0.01f, 0f), new Vector2(1.1f, 0.82f))
			: null;

	private void OnTargetInput(Node cameraNode, InputEvent @event, Vector3 eventPosition, Vector3 normal, long shapeIndex)
	{
		if (@event is not InputEventMouseButton inputEvent
			|| !inputEvent.Pressed
			|| inputEvent.ButtonIndex != MouseButton.Left) return;

		OnMouseClicked?.Invoke();
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.Input;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.BattleEngine.Vanilla.Features;
using CardWars.Client.scenes.core.game_session;
using CardWars.Client.scripts.core.packet;
using CardWars.Client.scripts.vanilla.registry;
using CardWars.Client.scripts.vanilla.systems;
using CardWars.Core.Logging;
using CardWars.Core.Network.Packet;
using CardWars.Core.Registry;
using CardWars.Vanilla.Shared.Packet;
using Godot;

namespace CardWars.Client.scenes.vanilla.instance.battle;

public partial class BattleInstance : ClientInstance
{
	public BattleRegistry? BattleRegistry => ClientRegistry?.GetExtension<BattleRegistry>();

	[Export] public Control? ButtonsOverlayNode;
	[Export] public HandManager? HandManagerNode;
	[Export] public MouseControl? MouseControlNode;
	[Export] public Node3D? PlayspaceNode;
	[Export] public Camera? CameraNode;

	public BattleAnimationHandler AnimationHandler = new();

	public GameState State = new();
	private TurnState _oldTurnState = new();

	public IReadOnlyDictionary<EntityId, EntityNode3D> EntityNodes => _entityNodes;
	private readonly Dictionary<EntityId, EntityNode3D> _entityNodes = [];

	public EntityNode3D? Hovered3DTarget = new();
	public Action<EntityNode3D>? OnHovered3DTargetClicked;

	// Test random node TODO: REMOVE LTR
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey inputEventKey)
		{
			if (inputEventKey.Keycode == Key.Space && inputEventKey.Pressed)
			{
				SendInput(new EndTurnRequestInput());
			}
			if (inputEventKey.Keycode == Key.Q && inputEventKey.Pressed)
			{
				var decks = State.Where(e => e is Deck);
				var myDeck = decks.First(d => ((Deck)d).OwnerPlayerId == MyPlayerId);
				SendInput(new DrawCardRequestInput(myDeck.Id, MyPlayerId));
			}
		}
	}

	public override void _Ready()
	{
		if (HandManagerNode is not null) HandManagerNode.BattleInstance = this;
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		AnimationHandler?.Process(this, delta);
	}

	public void FocusOn(EntityId entityId)
	{
		if (MouseControlNode is null) return;
		MouseControlNode.FocusOn(GetEntityNode<EntityNode3D>(entityId));
	}

	private bool TryFocusOn(EntityId entityId)
	{
		if (MouseControlNode?.CameraNode is null) return false;

		var node = GetEntityNode<EntityNode3D>(entityId);
		if (node is null) return false;

		MouseControlNode.FocusOn(node);
		return true;
	}

	public override void OnPacket(IPacket packet, PacketContextClient context)
	{
		switch (packet)
		{
			case S2C_BattleBlockBatch battleBlockBatch:
				ProcessBlockBatch(battleBlockBatch.Batch);
				break;
			case S2C_BattleSyncSnapshot battleSyncSnapshot:
				// USe packet here since snapshot is not really a atomic change but a snapshot. The team switching
				// we can ignore first since even if switch team we can assume the next cards on enemy will be covered
				State.FromSnapshot(battleSyncSnapshot.GameStateSnapshot);
				SyncState();
				break;
		}
	}

	public void ProcessBlockBatch(BlockBatch batch)
	{
		foreach (var block in batch.Blocks)
		{
			BattleEngineRegistry?.BlockHandlers.Execute(State, block);
		}
		SyncState();
		// Play animation
		var parsedAnimationId = ResourceId.Parse(batch.AnimationId);
		if (!parsedAnimationId.IsEmpty)
		{
			var animation = BattleRegistry?.Animations.Create(parsedAnimationId);
			if (animation is not null) { animation.Data = batch.AnimationArgs; AnimationHandler.QueueAnimation(animation); }
		}
	}

	public void SyncState()
	{
		if (BattleRegistry is null) return;
		if (PlayspaceNode is null) return;

		// Handle Entity Clenup & Handling
		BattleRegistry.EntityViewHandlers.Execute(this, State);

		// Handle Layout
		var defaultId = BattleRegistry.DefaultLayoutId;
		var layoutHandler = BattleRegistry.LayoutHandler.Get(State.Layout.Layout) ?? BattleRegistry?.LayoutHandler.Get(defaultId);
		layoutHandler?.Compute(this);

		// Set Camera
		UpdateCamera();
	}

	private void UpdateCamera()
	{
		var turn = State.Turn;
		if (turn.TurnOrder.Count == 0 || turn.TurnIndex >= turn.TurnOrder.Count) return;
		if (_oldTurnState is { } old && IsSameTurn(old, turn)) return;

		var playerTurnId = turn.TurnOrder[turn.TurnIndex];
		if (State.Get(playerTurnId) is not Player player) return;

		var battlefieldId = player.BattlefieldIds.FirstOrDefault();
		if (battlefieldId.IsNone) return;
		if (!TryFocusOn(battlefieldId)) return;

		_oldTurnState = turn.Clone();
	}

	private static bool IsSameTurn(TurnState left, TurnState right) // TODO: bruh move this into turnstate itself...
		=> left.TurnIndex == right.TurnIndex
		   && left.TurnNumber == right.TurnNumber
		   && left.Phase == right.Phase
		   && left.TurnOrder.SequenceEqual(right.TurnOrder);

	public bool HasEntityNode(EntityId id) => _entityNodes.ContainsKey(id);

	public IReadOnlyList<EntityId> GetEntityNodeIds<T>() where T : EntityNode3D
		=> [.. _entityNodes.Where(kv => kv.Value is T).Select(kv => kv.Key)];

	public IReadOnlyList<T> GetEntityNodes<T>() where T : EntityNode3D
		=> [.. _entityNodes.Where(kv => kv.Value is T).Select(kv => kv.Value).Cast<T>()];

	public T? GetEntityNode<T>(EntityId id) where T : EntityNode3D
		=> _entityNodes.TryGetValue(id, out var node) && node is T typed ? typed : null;

	public T? GetOrCreateEntityNode<T>(ResourceId sceneId, EntityId id) where T : EntityNode3D
	{
		if (_entityNodes.TryGetValue(id, out var existing) && existing is T existingTyped) return existingTyped;
		var node = BattleRegistry?.EntityScene.Instantiate<T>(sceneId);
		if (node is null) return null;

		node.Name = id.ToString();
		_entityNodes[id] = node;
		node.Setup(this, id);
		return node;
	}

	public bool FreeEntityNode(EntityId id)
	{
		// For now Idk if i want to handle at a handler level or instance level
		if (!_entityNodes.Remove(id, out var node)) return false;
		node.QueueFree();
		return true;
	}

	// Keeps a node under its owner's node; falls back to the playspace.
	public void AttachNodeToOwner(EntityNode3D node, EntityId? ownerId)
	{
		if (PlayspaceNode is null) return;
		var target = ownerId is { } id && GetEntityNode<EntityNode3D>(id) is { } ownerNode ? ownerNode : PlayspaceNode;
		var current = node.GetParent();
		if (current == target) return;

		if (current is Node formerParent) formerParent.RemoveChild(node);
		target.AddChild(node);
	}

	public void SendInput(IInput input)
		=> SendPacket(new C2S_BattleInput() { Input = input });
}

public partial class EntityNode3D : Node3D
{
	public Action? OnMouseEntered;
	public Action? OnMouseExited;
	public Action? OnMouseClicked;

	public BattleInstance? BattleInstance { get; set; }
	public EntityId EntityId { get; set; } = EntityId.None;
	public T? GetData<T>() where T : IEntity
		=> BattleInstance?.State.Get(EntityId) is T typed ? typed : default;

	private bool _hasHookedMouseEvents = false;

	public void Setup(BattleInstance battleInstance, EntityId entityId)
	{
		BattleInstance = battleInstance;
		EntityId = entityId;
	}

	public void HookMouseEvents()
	{
		if (BattleInstance is { } instance)
		{
			if (!_hasHookedMouseEvents)
			{
				OnMouseEntered += () => { instance.Hovered3DTarget = this; };
				OnMouseExited += () => { instance.Hovered3DTarget = null; };
				OnMouseClicked += () => { instance.OnHovered3DTargetClicked?.Invoke(this); };
				_hasHookedMouseEvents = true;
			}
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.Input;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.BattleEngine.Vanilla.Features;
using CardWars.Client.scenes.core.game_session;
using CardWars.Client.scripts.core.packet;
using CardWars.Client.scripts.vanilla.registry;
using CardWars.Client.scripts.vanilla.systems;
using CardWars.Core.Network.Packet;
using CardWars.Core.Registry;
using CardWars.Vanilla.Shared;
using CardWars.Vanilla.Shared.Packet;
using Godot;

namespace CardWars.Client.scenes.vanilla.instance.battle;

public partial class BattleInstance : ClientInstance
{
	public BattleRegistry? BattleRegistry => ClientRegistry?.GetExtension<BattleRegistry>();

	[Export] public Control? ButtonsOverlayNode;
	[Export] public HandManager? HandManagerNode;
	[Export] public MouseControl? MouseControlNode;
	[Export] public AttackController? AttackControllerNode;
	[Export] public Node3D? PlayspaceNode;
	[Export] public Camera? CameraNode;

	public BattleAnimationHandler AnimationHandler = new();
	public TargetHighlighter TargetHighlighter = new();

	public GameState State = new();
	private TurnState _oldTurnState = new();

	public IReadOnlyDictionary<EntityId, EntityNode3D> EntityNodes => _entityNodes;
	private readonly Dictionary<EntityId, EntityNode3D> _entityNodes = [];

	public IReadOnlyDictionary<EntityId, EntityNode3D> PendingFreeNodes => _pendingEntityNodes;
	private readonly Dictionary<EntityId, EntityNode3D> _pendingEntityNodes = [];

	public EntityNode3D? Hovered3DTarget;
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
		if (AttackControllerNode is not null) AttackControllerNode.Setup(this);
		AnimationHandler.Completed += ReleasePendingNodes;
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		AnimationHandler.Process(this, delta);
		TargetHighlighter.Process(this);
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
				ReleasePendingNodes();
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
			if (animation is not null) { animation.Data = batch.AnimationArgs; AnimationHandler.QueueAnimation(animation); return; }
		}
		if (!AnimationHandler.IsBusy) ReleasePendingNodes();
	}

	public void SyncState()
	{
		if (BattleRegistry is null) return;
		if (PlayspaceNode is null) return;

		// Handle Entity Clenup & Handling
		BattleRegistry.EntityViewHandlers.Execute(this, State);
		HandManagerNode?.RefreshTargeting();
		TargetHighlighter.Invalidate();

		// Handle Layout
		var defaultId = BattleRegistry.DefaultLayoutId;
		var layoutHandler = BattleRegistry.LayoutHandler.Get(State.Layout.Layout) ?? BattleRegistry?.LayoutHandler.Get(defaultId);
		layoutHandler?.Compute(this);

		// Set Camera
		UpdateCamera();
		UpdateGameOver();
		AttackControllerNode?.OnStateSynced();

		// Update pt label TODO: Temp...
		GetNode<Label>("UI/ButtonsOverlay/MarginContainer/VBoxContainer/Label").Text = $"Pt: {State.Get<Player>(MyPlayerId)?.Pt ?? 0}";
	}

	private void UpdateGameOver()
	{
		// TODO: Temporary...
		var overlayNode = GetNode<Control>("UI/Overlay");
		var resultLabel = GetNode<Label>("UI/Overlay/MarginContainer/Panel/ResultLabel");
		if (overlayNode is not null) overlayNode.Visible = State.IsGameOver;
		if (resultLabel is null) return;

		resultLabel.Text = State.IsGameOver
			? State.IsDraw
				? "Draw"
				: State.WinningTeam is { } team ? $"Team {team} Wins!" : "Game Over"
			: "";
	}

	private void UpdateCamera()
	{
		var turn = State.Turn;
		if (turn.TurnOrder.Count == 0 || turn.TurnIndex >= turn.TurnOrder.Count) return;
		if (_oldTurnState is { } old && turn.Equals(old)) return;

		var playerTurnId = turn.TurnOrder[turn.TurnIndex];
		if (State.Get(playerTurnId) is not Player player) return;

		var battlefieldId = player.BattlefieldIds.FirstOrDefault();
		if (battlefieldId.IsNone) return;
		if (!TryFocusOn(battlefieldId)) return;

		_oldTurnState = turn.Clone();
	}

	// --- Entity Nodes Accessors ---
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

	// --- Pending Entity Nodes Accessors ---
	public bool HasPendingEntityNode(EntityId id) => _pendingEntityNodes.ContainsKey(id);

	public IReadOnlyList<EntityId> GetPendingEntityNodeIds<T>() where T : EntityNode3D
		=> [.. _pendingEntityNodes.Where(kv => kv.Value is T).Select(kv => kv.Key)];

	public IReadOnlyList<T> GetPendingEntityNodes<T>() where T : EntityNode3D
		=> [.. _pendingEntityNodes.Where(kv => kv.Value is T).Select(kv => kv.Value).Cast<T>()];

	public T? GetPendingEntityNode<T>(EntityId id) where T : EntityNode3D
		=> _pendingEntityNodes.TryGetValue(id, out var node) && node is T typed ? typed : null;

	// Entity Node freeing ssytems
	public bool FreeEntityNode(EntityId id)
	{
		// For now Idk if i want to handle at a handler level or instance level
		if (!_entityNodes.Remove(id, out var node)) return false;
		_pendingEntityNodes.Add(id, node);
		return true;
	}

	private void ReleasePendingNodes()
	{
		foreach (var node in _pendingEntityNodes)
		{
			if (IsInstanceValid(node.Value)) node.Value.QueueFree();
		}
		_pendingEntityNodes.Clear();
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

public readonly record struct HighlightBounds(Vector3 Center, Vector2 Size, float Rotation = 0f);

public partial class EntityNode3D : Node3D
{
	public Action? OnMouseEntered;
	public Action? OnMouseExited;
	public Action? OnMouseClicked;

	public BattleInstance? BattleInstance { get; set; } // TODO: Rename to something else ig?
	public EntityId EntityId { get; set; } = EntityId.None;
	public bool IsValidTarget { get; private set; }

	private TargetHighlight? _targetHighlight;

	public virtual HighlightBounds? GetHighlightBounds() => null;

	public void SetValidTarget(bool valid)
	{
		IsValidTarget = valid;

		if (!valid)
		{
			if (_targetHighlight is not null)
			{
				RemoveChild(_targetHighlight);
				_targetHighlight.QueueFree();
				_targetHighlight = null;
			}

			return;
		}

		if (GetHighlightBounds() is not { } bounds) return;

		if (_targetHighlight is null)
		{
			_targetHighlight = BattleInstance?.ClientRegistry?.UserInterface.Instantiate<TargetHighlight>(SharedIds.UI.TargetHighlight);
			if (_targetHighlight is null) return;

			AddChild(_targetHighlight);
		}

		_targetHighlight.Configure(bounds);
		_targetHighlight.SetHighlighted(true);
	}

	public override void _Process(double delta)
	{
		if (IsValidTarget)
		{
			_targetHighlight?.SetHovered(BattleInstance?.Hovered3DTarget == this);
		}
	}

	public T? GetData<T>(EntityId? entityId = null) where T : IEntity
		=> BattleInstance?.State.Get(entityId ?? EntityId) is T typed ? typed : default;

	public bool TryGetData<T>(out T? data, EntityId? entityId = null) where T : IEntity
	{
		data = GetData<T>(entityId ?? EntityId);
		return data is not null;
	}

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
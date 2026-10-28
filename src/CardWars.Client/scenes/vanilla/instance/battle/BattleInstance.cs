using System;
using System.Collections.Generic;
using System.Linq;
using CardWars.BattleEngine.Behaviour.DataDriven;
using CardWars.BattleEngine.Behaviour.DataDriven.Expression;
using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.Input;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.BattleEngine.Vanilla.Features;
using CardWars.Client.scenes.core.game_session;
using CardWars.Client.scripts.core.packet;
using CardWars.Client.scripts.vanilla.registry;
using CardWars.Core.Data.Mapping;
using CardWars.Core.Data.Serialization.Json;
using CardWars.Core.Data.Tags;
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

	public IReadOnlyDictionary<EntityId, Node3D> EntityNodes => _entityNodes;
	private readonly Dictionary<EntityId, Node3D> _entityNodes = [];

	public Node3D? Hovered3DTarget;
	public Action<Node3D>? OnHovered3DTargetClicked;

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
		// // Start of TEST DEBUG
		// if (HandManagerNode != null) HandManagerNode.BattleInstance = this;

		// var o = new BehaviourInstruction(
		// 	[
		// 		new EventInstructions(
		// 			ResourceId.Vanilla("huh"),
		// 			[
		// 				new DeclarationLine("i",
		// 					new AddExpressionSpec(
		// 						new GivenValueExpressionSpec(new StringTag("Hello World: Times: ")),
		// 						new MultiplyExpressionSpec(
		// 							new GivenValueExpressionSpec(new IntTag(5)),
		// 							new GivenValueExpressionSpec(new IntTag(3))
		// 						)
		// 					)
		// 				),
		// 			]
		// 		)
		// 	],
		// []
		// );
		// var s = DataTagSerializer.Serialize(DataTagMapper.ToTag(o));
		// Log.Info("deserialized value");
		// Log.Info(s);

		// var d = DataTagMapper.FromTag<BehaviourInstruction>(DataTagSerializer.Deserialize<CompoundTag>(s) ?? new CompoundTag());
		// Log.Info("serialized value");
		// Log.Info(d);
		// // End of TEST DEBUG
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		AnimationHandler?.Process(this, delta);
	}

	public void FocusOn(EntityId entityId)
	{
		if (MouseControlNode == null) return;
		MouseControlNode.FocusOn(GetEntityNode<Node3D>(entityId));
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
			if (animation != null) { animation.Data = batch.AnimationArgs; AnimationHandler.QueueAnimation(animation); }
		}
	}

	public void SyncState()
	{
		if (BattleRegistry == null) return;
		if (PlayspaceNode == null) return;

		// Handle Entity Clenup & Handling
		BattleRegistry.EntityViewHandlers.Execute(this, State);

		// Handle Layout
		var defaultId = BattleRegistry.DefaultLayoutId;
		var layoutHandler = BattleRegistry.LayoutHandler.Get(State.Layout.Layout) ?? BattleRegistry?.LayoutHandler.Get(defaultId);
		layoutHandler?.Compute(this);

		// Set Camera
		if (State.Turn.TurnOrder.Count > 0 && State.Turn.TurnIndex < State.Turn.TurnOrder.Count)
		{
			var playerTurnId = State.Turn.TurnOrder[State.Turn.TurnIndex];
			if (_oldTurnState.TurnIndex != State.Turn.TurnIndex) //TODO: oldTurnState feels wrong and not a good way?
			{
				var e = State.Get(playerTurnId);
				if (e is Player player)
				{
					var battlefieldId = player.BattlefieldIds.FirstOrDefault();
					FocusOn(battlefieldId);
				}
				_oldTurnState = State.Turn.Copy();
			}
			else
			{
			}
		}
	}

	public bool HasEntityNode(EntityId id) => _entityNodes.ContainsKey(id);

	public IReadOnlyList<EntityId> GetEntityNodeIds<T>() where T : Node3D
		=> [.. _entityNodes.Where(kv => kv.Value is T).Select(kv => kv.Key)];

	public IReadOnlyList<T> GetEntityNodes<T>() where T : Node3D
		=> [.. _entityNodes.Where(kv => kv.Value is T).Select(kv => kv.Value).Cast<T>()];

	public T? GetEntityNode<T>(EntityId id) where T : Node3D
		=> _entityNodes.TryGetValue(id, out var node) ? node as T : null;

	public T? GetOrCreateEntityNode<T>(ResourceId sceneId, EntityId id) where T : Node3D
	{
		if (_entityNodes.TryGetValue(id, out var existing)) return existing as T;
		var node = BattleRegistry?.EntityScene.Instantiate<T>(sceneId);
		if (node == null) return null;

		node.Name = id.ToString();
		_entityNodes[id] = node;
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
	public void AttachNodeToOwner(Node3D node, EntityId? ownerId)
	{
		if (PlayspaceNode == null) return;
		var target = ownerId is { } id && GetEntityNode<Node3D>(id) is { } ownerNode ? ownerNode : PlayspaceNode;
		var current = node.GetParent();
		if (current == target) return;

		if (current is Node formerParent) formerParent.RemoveChild(node);
		target.AddChild(node);
	}

	public void SendInput(IInput input)
		=> SendPacket(new C2S_BattleInput() { Input = input });
}

using System.Collections.Generic;
using System.Linq;
using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.Client.scenes.core.game_session;
using CardWars.Client.scripts.core.packet;
using CardWars.Client.scripts.vanilla.layout;
using CardWars.Client.scripts.vanilla.registry;
using CardWars.Core.Logging;
using CardWars.Core.Network.Packet;
using CardWars.Core.Registry;
using CardWars.Vanilla.Shared.Packet;
using Godot;

namespace CardWars.Client.scenes.vanilla.instance.battle;

public partial class BattleInstance : ClientInstance
{
	public BattleRegistry? BattleRegistry => ClientRegistry?.GetExtension<BattleRegistry>();

	public Control? UINode;
	public HandManager? HandManagerNode;
	public Node3D? PlayspaceNode;
	public Camera3D? CameraNode;
	public Control? MouseControlNode;

	public GameState State = new();
	public IReadOnlyDictionary<EntityId, Node3D> EntityNodes => _entityNodes;
	private readonly Dictionary<EntityId, Node3D> _entityNodes = [];



	public override void _Ready()
	{
		UINode = GetNode<Control>("UI");
		HandManagerNode = GetNode<HandManager>("UI/HandManager");
		PlayspaceNode = GetNode<Node3D>("Playspace");
		CameraNode = GetNode<Camera3D>("Camera3D");
		MouseControlNode = GetNode<Control>("UI/HandManager/MouseControl");

		HandManagerNode.BattleInstance = this;
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
	}

	public void SyncState()
	{
		var currentIds = State.All.Select(e => e.Id).ToHashSet();
		var toRemove = _entityNodes.Keys.Except(currentIds).ToList();
		foreach (var id in toRemove)
		{
			var node = _entityNodes[id];
			node.QueueFree(); // DONT DO THIS!! Set this to make it run animations first then get rid of it
			_entityNodes.Remove(id);
		}


		if (PlayspaceNode == null) return;

		foreach (var entity in State.All)
		{
			BattleRegistry?.EntityViewHandlers.Execute(this, entity);
		}

		if (BattleRegistry == null) { return; }
		var defaultId = BattleRegistry?.DefaultLayoutId ?? ResourceId.Empty;
		IBattleLayoutHandler? layoutHandler = BattleRegistry?.LayoutHandler.Get(State.Layout.Layout)
											 ?? BattleRegistry?.LayoutHandler.Get(defaultId);
		if (layoutHandler == null) { return; }
		layoutHandler.Compute(this);

		Log.Info(State);
	}

	public bool HasEntityNode(EntityId id) => _entityNodes.ContainsKey(id);

	public T? GetEntityNode<T>(EntityId id) where T : Node3D
		=> _entityNodes.TryGetValue(id, out var node) ? node as T : null;

	// Returns the existing node, or creates + tracks a fresh one (unparented).
	public T? GetOrCreateEntityNode<T>(ResourceId sceneId, EntityId id) where T : Node3D
	{
		if (_entityNodes.TryGetValue(id, out var existing)) return existing as T;
		var node = BattleRegistry?.EntityScene.Instantiate<T>(sceneId);
		if (node == null) return null;

		node.Name = id.ToString();
		_entityNodes[id] = node;
		return node;
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
}

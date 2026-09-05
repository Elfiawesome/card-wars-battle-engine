using System;
using System.Collections.Generic;
using System.Linq;
using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.Input;
using CardWars.Core.Network.Transport;
using Godot;

namespace CardWars.Client;

public partial class CardBattle : Node3D
{
	public IConnection? Connection { get; set; }
	public Action<IInput>? OnInputSubmit;
	private readonly List<IBlock> _blocks = [];

	private HandCardContainer? _handCardContainer;

	public override void _Ready()
	{
		_handCardContainer = GetNode<HandCardContainer>("UI/HandCardContainer");
	}

	public void OnBlockBatch(BlockBatch batch)
	{
		foreach (var b in batch.Blocks)
		{
			Core.Logging.Logger.Info(b.GetType().Name);
			switch (b) { }
		}
	}

	public override void _Process(double delta)
	{
	}
}

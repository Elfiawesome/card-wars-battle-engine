using System;
using System.Collections.Generic;
using System.Linq;
using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.Input;
using CardWars.Core.Network.Transport;
using Godot;

namespace CardWars.Client;

public partial class CardBattle : CanvasLayer
{
	public IConnection? Connection { get; set; }
	public Action<IInput>? OnInputSubmit;
	private readonly List<IBlock> blocks = [];

	private Label _debugLabel;

	public override void _Ready()
	{
		_debugLabel = GetNode<Label>("DebugLabel");
	}

	public void OnBlockBatch(BlockBatch batch)
	{
		foreach (var blk in batch.Blocks)
		{
			blocks.Add(blk);
		}
		_debugLabel.Text = string.Join(", ", blocks.Select(b => $"[{b.GetType().Name}]").ToList());
	}

	public override void _Process(double delta)
	{
	}
}

using System;
using Godot;

namespace CardWars.Client;

public partial class CardDisplay : Control
{
	public Action<CardDisplay>? OnCardMouseEntered;
	public Action<CardDisplay>? OnCardMouseExited;

	public Tween? currentAnimationTween;

	public override void _Ready()
	{
		MouseEntered += () => { OnCardMouseEntered?.Invoke(this); };
		MouseExited += () => { OnCardMouseExited?.Invoke(this); };
	}

	public override void _Process(double delta)
	{
	}
}

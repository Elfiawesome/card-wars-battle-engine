using CardWars.BattleEngine.Vanilla.Entity;
using Godot;

namespace CardWars.Client.scenes.vanilla.instance.battle;

public partial class Card3D : EntityNode3D
{
	public CardDisplay? Display => GetNodeOrNull<CardDisplay>("SubViewport/CardDisplay");

	public void SetCard(GenericCard card)
	{
		if (Display is { } display) display.SetData(card.Data);
	}
}

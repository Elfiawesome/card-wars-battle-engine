using CardWars.Client.scenes.core.game_session;
using CardWars.Core.Data.Tags;
using Godot;

namespace CardWars.Client.scenes.cardwars.ui.card_display;

public abstract partial class CardDisplay : Control
{
	public DisplayType Display { get; protected set; } = DisplayType.Full;
	public enum DisplayType { Full, Simplified }

	public Tween? currentAnimationTween;

	public virtual void SetData(CompoundTag data, ClientInstanceRegistry registry) { }
	public virtual void SetDisplay(DisplayType displayType) { }
}

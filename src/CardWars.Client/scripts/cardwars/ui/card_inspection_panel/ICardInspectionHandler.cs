using System.Collections.Generic;
using CardWars.Client.scenes.core.game_session;
using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;

namespace CardWars.Client.scripts.cardwars.ui.card_inspection_panel;

public readonly record struct CardInspectionContext(
	CompoundTag Data,
	ClientInstanceRegistry Registry
);

public sealed record CardInspectionGrid(
	int Width,
	IReadOnlyList<CardInspectionGridCell> Cells
);

public readonly record struct CardInspectionGridCell(
	bool Filled,
	bool Crossed,
	string Text
);

public sealed record CardInspectionData(
	string Title,
	string Text,
	ResourceId? IconId,
	CardInspectionGrid? Grid,
	string Color = "#ffffff"
);

public interface ICardInspectionHandler
{
	IReadOnlyList<CardInspectionData> GetInspectionPanels(CardInspectionContext context);
}

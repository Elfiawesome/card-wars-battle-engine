using System.Collections.Generic;
using CardWars.Client.scenes.core.game_session;
using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;

namespace CardWars.Client.scripts.cardwars.ui.card_inspection_panel;

public readonly record struct CardInspectionContext(
	CompoundTag Data,
	ClientInstanceRegistry Registry
);

public sealed record InspectionGrid(
	int Width,
	IReadOnlyList<InspectionGridCell> Cells
);

public readonly record struct InspectionGridCell(
	bool Filled,
	bool Crossed,
	string Text
);

public sealed record InspectionData(
	string Title,
	string Text,
	ResourceId? IconId,
	InspectionGrid? Grid,
	string Color = "#ffffff"
);

public interface ICardInspectionHandler
{
	IReadOnlyList<InspectionData> GetInspectionPanels(CardInspectionContext context);
}

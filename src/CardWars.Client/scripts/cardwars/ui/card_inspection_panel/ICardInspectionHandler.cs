using System.Collections.Generic;
using CardWars.Core.Data.Tags;

namespace CardWars.Client.scripts.cardwars.ui.card_inspection_panel;

public interface ICardInspectionHandler
{
	public List<InspectionData> GetInspectionPanels(CompoundTag cardData);

	public void InstantiatePanels(List<InspectionData> panels)
	{
		
	}
}

public record InspectionData
{
	
}
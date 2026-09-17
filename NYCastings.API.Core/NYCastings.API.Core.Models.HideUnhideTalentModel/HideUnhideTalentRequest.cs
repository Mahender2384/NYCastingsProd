namespace NYCastings.API.Core.Models.HideUnhideTalentModel;

public class HideUnhideTalentRequest
{
	public int ClientId { get; set; }

	public int TalentId { get; set; }

	public bool Hide { get; set; }
}

namespace NYCastings.API.Core.Models.UserAffiliationModel;

public class UserAffiliationModel
{
	public int UserAffiliationId { get; set; }

	public int UserId { get; set; }

	public int AffiliationId { get; set; }

	public string? AffiliationName { get; set; }

	public bool ActiveFlag { get; set; }
}

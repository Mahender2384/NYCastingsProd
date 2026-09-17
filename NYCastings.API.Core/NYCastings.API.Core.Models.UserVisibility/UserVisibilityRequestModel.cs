namespace NYCastings.API.Core.Models.UserVisibility;

public class UserVisibilityRequestModel
{
	public int TalentId { get; set; }

	public int ClientId { get; set; }

	public bool? IsHide { get; set; }

	public bool? IsBlock { get; set; }
}

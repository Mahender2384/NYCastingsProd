namespace NYCastings.API.Core.Models.TalentFullProfileModel;

public class UserTalentModel
{
	public int IdnUserTalent { get; set; }

	public int IdnUser { get; set; }

	public int IdnTalent { get; set; }

	public string? NamTalent { get; set; }

	public bool ActiveFlag { get; set; }
}

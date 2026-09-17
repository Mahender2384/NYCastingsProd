namespace NYCastings.API.Core.Models.UserEmail;

public class RoleAlertEmailRequest
{
	public string? CityChoices { get; set; }

	public string? Subject { get; set; }

	public string? ProjectTitle { get; set; }

	public string? CastingDirectorName { get; set; }

	public string? TalentGender { get; set; }

	public string? TalentAgeRange { get; set; }

	public string? LocationList { get; set; }
}

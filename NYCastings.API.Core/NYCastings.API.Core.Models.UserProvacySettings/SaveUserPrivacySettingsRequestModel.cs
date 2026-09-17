namespace NYCastings.API.Core.Models.UserProvacySettings;

public class SaveUserPrivacySettingsRequestModel
{
	public int UserId { get; set; }

	public bool? IsPublicSearchOptOut { get; set; }

	public bool? IsSearchEmailOptOut { get; set; }

	public bool? IsResumeViewedEmailOptOut { get; set; }
}

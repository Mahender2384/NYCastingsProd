using Microsoft.AspNetCore.Http;

namespace NYCastings.API.Core.Models.DirectSubmitHistoryRequest;

public class DirectSubmitHistoryRequest
{
	public int UserID { get; set; }

	public int Type { get; set; }

	public int NoticeID { get; set; }

	public string? From { get; set; }

	public string? To { get; set; }

	public string? NoticeTitle { get; set; }

	public string? CoverMsg { get; set; }

	public int HtmlFormat { get; set; }

	public int RoleId { get; set; }

	public string? MediaURL { get; set; }

	public string? VideoPath { get; set; }

	public string? TalentName { get; set; }

	public string? DirectorName { get; set; }

	public string? SubmissionLink { get; set; }

	public IFormFile? MediaFile { get; set; }

	public string? ImageURL { get; set; }
}

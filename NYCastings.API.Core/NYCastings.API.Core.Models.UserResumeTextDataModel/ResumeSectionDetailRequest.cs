namespace NYCastings.API.Core.Models.UserResumeTextDataModel;

public class ResumeSectionDetailRequest
{
	public int? ResumeSectionDetailId { get; set; }

	public int ResumeSectionId { get; set; }

	public string? Title { get; set; }

	public string? Role { get; set; }

	public string? Company { get; set; }

	public string? Url { get; set; }

	public int UserId { get; set; }

	public int SortOrder { get; set; }

	public bool ActiveFlag { get; set; }
}

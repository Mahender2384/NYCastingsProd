namespace NYCastings.API.Core.Models.UserResumeTextDataModel;

public class ResumeSectionRequest
{
	public int? ResumeSectionId { get; set; }

	public int UserId { get; set; }

	public int UserBy { get; set; }

	public string? SectionHeading { get; set; }

	public int SectionSortOrder { get; set; }

	public bool ActiveFlag { get; set; }
}

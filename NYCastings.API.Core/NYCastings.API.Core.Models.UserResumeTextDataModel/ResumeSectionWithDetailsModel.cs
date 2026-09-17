using System;

namespace NYCastings.API.Core.Models.UserResumeTextDataModel;

public class ResumeSectionWithDetailsModel
{
	public int ResumeSectionId { get; set; }

	public string? SectionHeading { get; set; }

	public short? SectionSortOrder { get; set; }

	public int? ResumeSectionDetailId { get; set; }

	public string? Title { get; set; }

	public string? Role { get; set; }

	public string? Company { get; set; }

	public string? Url { get; set; }

	public short? DetailSortOrder { get; set; }

	public DateTime? DetailDateCreated { get; set; }
}

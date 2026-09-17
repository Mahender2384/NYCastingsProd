namespace NYCastings.API.Core.Models.UserResumeTextDataModel;

public class ResumeTextDataModel
{
	public int? ResumeTextDataId { get; set; }

	public int UserId { get; set; }

	public byte IsInternal { get; set; }

	public string? ResumeTextHeading { get; set; }

	public string? ResumeTextDetails { get; set; }

	public int SortOrder { get; set; }

	public int UserBy { get; set; }

	public bool ActiveFlag { get; set; }
}

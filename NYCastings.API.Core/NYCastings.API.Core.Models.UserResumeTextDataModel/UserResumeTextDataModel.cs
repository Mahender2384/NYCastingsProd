using System;

namespace NYCastings.API.Core.Models.UserResumeTextDataModel;

public class UserResumeTextDataModel
{
	public int ResumeTextDataId { get; set; }

	public int UserId { get; set; }

	public byte IsInternal { get; set; }

	public string? ResumeTextHeading { get; set; }

	public string? ResumeTextDetails { get; set; }

	public short? SortOrder { get; set; }

	public int? CreatedUserId { get; set; }

	public int? UpdatedUserId { get; set; }

	public DateTime? DateRecordCreated { get; set; }

	public DateTime? DateRecordUpdated { get; set; }

	public bool ActiveFlag { get; set; }
}

using System;
using System.Collections.Generic;

namespace NYCastings.API.Core.Models.UnapprovedNoticeResponseModel;

public class UnapprovedNoticeModel
{
	public int NoticeId { get; set; }

	public string? ProjectType { get; set; }

	public string? Union { get; set; }

	public DateTime? NoticeStartDate { get; set; }

	public DateTime? NoticeEndDate { get; set; }

	public bool ProtectFlag { get; set; }

	public string? Title { get; set; }

	public string? Pay { get; set; }

	public int? Rate { get; set; }

	public string? Category { get; set; }

	public string? LocationCodes { get; set; }

	public string? NoticeDescription { get; set; }

	public string? NoticeShortDescription { get; set; }

	public string? NoticeSubmittedByEmail { get; set; }

	public string? DirectorName { get; set; }

	public string? NoticeStatus { get; set; }

	public DateTime? NoticeCreatedDate { get; set; }

	public DateTime? NoticeUpdatedDate { get; set; }

	public List<UnapprovedRoleModel>? Roles { get; set; }
}

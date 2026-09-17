using System;
using System.Collections.Generic;

namespace NYCastings.API.Core.Models.Filtered_Casting_Notices;

public class CastingNoticeResponseModel
{
	public int NoticeId { get; set; }

	public string? ProjectType { get; set; }

	public string? Union { get; set; }

	public DateTime? NoticeStartDate { get; set; }

	public DateTime? NoticeEndDate { get; set; }

	public DateTime? NoticeUpdatedDate { get; set; }

	public bool ProtectFlag { get; set; }

	public string? Title { get; set; }

	public string? Pay { get; set; }

	public int PaymentType { get; set; }

	public string? Category { get; set; }

	public string? LocationCodes { get; set; }

	public string? NoticeDescription { get; set; }

	public string? NoticeShortDescription { get; set; }

	public string? NoticeSubmittedByEmail { get; set; }

	public string? DirectorName { get; set; }

	public string? PicturesandDocumentsName { get; set; }

	public string? PicturesandDocumentsPath { get; set; }

	public string? PhotoReferences { get; set; }

	public string? PhotoReferencesPath { get; set; }

	public string? Scripts { get; set; }

	public string? ScriptsPath { get; set; }

	public bool? RushCall { get; set; }

	public bool? Favorite { get; set; }

	public int RoleCount { get; set; }

	public List<RoleModel> Roles { get; set; } = new List<RoleModel>();
}

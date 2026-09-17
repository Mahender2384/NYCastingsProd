using System;
using System.Collections.Generic;

namespace NYCastings.API.Core.Models.NewCastingNoticeModel;

public class GetNewCastingNoticeDetails
{
	public int NoticeId { get; set; }

	public string? ContactName { get; set; }

	public string? Company { get; set; }

	public string? ContactPhone { get; set; }

	public string? ContactEmail { get; set; }

	public string? ProjectTitle { get; set; }

	public string? ProjectDescription { get; set; }

	public string? ProjectType { get; set; }

	public string? AuditionLocations { get; set; }

	public string? CastingOrShootDatesInfo { get; set; }

	public string? UnionStatus { get; set; }

	public string? PayScale { get; set; }

	public int PaymentType { get; set; }

	public string? Category { get; set; }

	public DateTime? NoticeStartDate { get; set; }

	public DateTime? NoticeEndDate { get; set; }

	public string? EmailAddress { get; set; }

	public string? PicturesandDocumentsName { get; set; }

	public string? PicturesandDocumentsPath { get; set; }

	public string? PhotoReferences { get; set; }

	public string? PhotoReferencesPath { get; set; }

	public string? Scripts { get; set; }

	public string? ScriptsPath { get; set; }

	public DateTime? NoticeCreatedDate { get; set; }

	public DateTime? NoticeUpdatedDate { get; set; }

	public string? NoticeStatus { get; set; }

	public string? DirectorName { get; set; }

	public bool? RushCall { get; set; }

	public int? CreatedUserId { get; set; }

	public int? UpdatedUserId { get; set; }

	public List<RoleModel> Roles { get; set; } = new List<RoleModel>();

	public bool? Favorite { get; set; }
}

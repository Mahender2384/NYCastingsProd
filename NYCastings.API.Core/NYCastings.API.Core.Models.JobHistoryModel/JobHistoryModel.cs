using System;

namespace NYCastings.API.Core.Models.JobHistoryModel;

public class JobHistoryModel
{
	public int UserId { get; set; }

	public string? CoverMessage { get; set; }

	public DateTime DateRecordSubmitted { get; set; }

	public string? VideoPath { get; set; }

	public string? MediaURL { get; set; }

	public int NoticeId { get; set; }

	public string? ProjectType { get; set; }

	public string? Union { get; set; }

	public DateTime NoticeStartDate { get; set; }

	public DateTime? NoticeEndDate { get; set; }

	public bool? ProtectFlag { get; set; }

	public string? Title { get; set; }

	public string? Pay { get; set; }

	public int Rate { get; set; }

	public string? Category { get; set; }

	public string? LocationCodes { get; set; }

	public string? NoticeDescription { get; set; }

	public string? NoticeShortDescription { get; set; }

	public int RoleId { get; set; }

	public string? RoleName { get; set; }

	public string? Sex { get; set; }

	public string? Ethnicity { get; set; }

	public string? RoleUnion { get; set; }

	public int AgeStart { get; set; }

	public int AgeEnd { get; set; }

	public string? RoleType { get; set; }

	public string? RoleDetails { get; set; }

	public string? ImageURL { get; set; }
}

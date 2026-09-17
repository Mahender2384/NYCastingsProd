using System;
using Microsoft.AspNetCore.Http;

namespace NYCastings.API.Core.Models.NewCastingNoticeModel;

public class AddNewCastingNoticeDetails
{
	public int NoticeId { get; set; }

	public int UserId { get; set; }

	public int? UpdatedUserId { get; set; }

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

	public DateTime? RemovalDate { get; set; }

	public string? EmailAddress { get; set; }

	public IFormFile? PicturesFile { get; set; }

	public IFormFile? ReferencesFile { get; set; }

	public IFormFile? ScriptsFile { get; set; }

	public string? PicturesandDocumentsName { get; set; }

	public string? PicturesandDocumentsPath { get; set; }

	public string? PhotoReferences { get; set; }

	public string? PhotoReferencesPath { get; set; }

	public string? Scripts { get; set; }

	public string? ScriptsPath { get; set; }

	public string? JobCategory { get; set; }

	public bool? RushCall { get; set; }

	public int PaymentType { get; set; }
}

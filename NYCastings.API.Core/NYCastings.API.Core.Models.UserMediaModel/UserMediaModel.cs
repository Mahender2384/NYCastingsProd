using System;

namespace NYCastings.API.Core.Models.UserMediaModel;

public class UserMediaModel
{
	public int? MediaId { get; set; }

	public int UserId { get; set; }

	public string? MediaType { get; set; }

	public string? MediaName { get; set; }

	public string? MediaStyle { get; set; }

	public string? MediaTitle { get; set; }

	public DateTime? DateRecordCreated { get; set; }

	public int? CreatedUserId { get; set; }

	public DateTime? DateRecordUpdated { get; set; }

	public int? ActiveFlag { get; set; }

	public int? PicSort { get; set; }
}

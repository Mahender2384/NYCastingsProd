using System;
using System.Collections.Generic;

namespace NYCastings.API.Core.Models.Filtered_Casting_Notices;

public class CastingNoticeFilterRequest
{
	public List<string> Locations { get; set; }

	public List<string> Ethnicities { get; set; }

	public List<string> Unions { get; set; }

	public List<string> JobCategories { get; set; }

	public List<string> PayLevels { get; set; }

	public List<string> Sex { get; set; }

	public int? MinAge { get; set; }

	public int? MaxAge { get; set; }

	public int page { get; set; }

	public int pageSize { get; set; }

	public string? SortOrder { get; set; }

	public int UserId { get; set; }

	public string? Title { get; set; }

	public bool? RushCall { get; set; }

	public List<string> RoleTypes { get; set; }

	public int? PaymentType { get; set; }

	public bool? Favorite { get; set; }

	public DateTime? StartDate { get; set; }

	public DateTime? EndDate { get; set; }
}

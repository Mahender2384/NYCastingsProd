using System.Collections.Generic;

namespace NYCastings.API.Core.Models.SaveCastingSearchModel;

public class SaveCastingSearchRequest
{
	public string? Email { get; set; }

	public List<string>? Locations { get; set; }

	public List<string>? Ethnicities { get; set; }

	public List<string>? Unions { get; set; }

	public List<string>? JobCategories { get; set; }

	public List<string>? PayLevels { get; set; }

	public List<string>? Sex { get; set; }

	public List<string>? RoleTypes { get; set; }

	public string? Title { get; set; }

	public int? MinAge { get; set; }

	public int? MaxAge { get; set; }

	public string? SortOrder { get; set; }

	public bool? RushCall { get; set; }

	public List<string>? JobType { get; set; }
}

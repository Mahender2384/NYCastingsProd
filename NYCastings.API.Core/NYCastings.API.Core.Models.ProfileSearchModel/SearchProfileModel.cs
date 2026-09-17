using System;

namespace NYCastings.API.Core.Models.ProfileSearchModel;

public class SearchProfileModel
{
	public int UserId { get; set; }

	public string? FirstName { get; set; }

	public string? LastName { get; set; }

	public string? Union { get; set; }

	public int AgeStart { get; set; }

	public int AgeEnd { get; set; }

	public string? TalentImage { get; set; }

	public bool HasAudio { get; set; }

	public bool HasVideo { get; set; }

	public DateTime RecordCreatedDate { get; set; }

	public int MostLiked { get; set; }

	public bool Favorite { get; set; }

	public string? TalentEmail { get; set; }

	public string? Sex { get; set; }

	public int Experience { get; set; }
}

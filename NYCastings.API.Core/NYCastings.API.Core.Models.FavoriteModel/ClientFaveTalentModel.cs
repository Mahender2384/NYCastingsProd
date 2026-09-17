using System;

namespace NYCastings.API.Core.Models.FavoriteModel;

public class ClientFaveTalentModel
{
	public int UserId { get; set; }

	public string? FirstName { get; set; }

	public string? LastName { get; set; }

	public string? Union { get; set; }

	public int AgeStart { get; set; }

	public int AgeEnd { get; set; }

	public int HeightStart { get; set; }

	public int HeightEnd { get; set; }

	public string? TalentImage { get; set; }

	public string? City { get; set; }

	public string? LocationCode { get; set; }

	public int? MostLiked { get; set; }

	public DateTime? FavoratedDate { get; set; }

	public bool HasAudio { get; set; }

	public bool HasVideo { get; set; }
}

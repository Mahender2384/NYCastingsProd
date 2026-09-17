namespace NYCastings.API.Core.Models.ProfileSearchModel;

public class SearchProfileRequest
{
	public string? Experience { get; set; }

	public bool? MostRecent { get; set; }

	public bool? MostLikes { get; set; }

	public string? TypeofTalent { get; set; }

	public string? Union { get; set; }

	public string? Sex { get; set; }

	public string? Ethnicity { get; set; }

	public int? AgeStart { get; set; }

	public int? AgeEnd { get; set; }

	public string? SpecialSkills { get; set; }

	public string? TalentLastName { get; set; }

	public string? EyeColor { get; set; }

	public string? HairColor { get; set; }

	public int? HeightStart { get; set; }

	public int? HeightEnd { get; set; }

	public int? WeightStart { get; set; }

	public int? WeightEnd { get; set; }

	public string? VocalRange { get; set; }

	public string? VocalType { get; set; }

	public bool? MustHaveVideo { get; set; }

	public bool? MustHaveAudio { get; set; }

	public string? Location { get; set; }

	public int DirectorId { get; set; }

	public double? Distance { get; set; }
}

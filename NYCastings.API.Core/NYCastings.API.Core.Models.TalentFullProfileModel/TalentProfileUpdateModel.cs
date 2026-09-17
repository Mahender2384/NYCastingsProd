namespace NYCastings.API.Core.Models.TalentFullProfileModel;

public class TalentProfileUpdateModel
{
	public int UserId { get; set; }

	public string? LastName { get; set; }

	public string? FirstName { get; set; }

	public string? Sex { get; set; }

	public string? PrimaryContactNumber { get; set; }

	public string? StateCode { get; set; }

	public string? Email { get; set; }

	public int? AgeRangeFrom { get; set; }

	public int? AgeRangeTo { get; set; }

	public int? EyeColorCode { get; set; }

	public int? HairColorCode { get; set; }

	public int? Weight { get; set; }

	public int? HeightStart { get; set; }

	public int? HeightEnd { get; set; }

	public int? VocalRangeID { get; set; }

	public string? Dress { get; set; }

	public string? Bust { get; set; }

	public string? Waist { get; set; }

	public string? Hips { get; set; }

	public string? Shoe { get; set; }

	public string? MaleSuit { get; set; }

	public string? MaleShirt { get; set; }

	public string? MaleWaist { get; set; }

	public string? MaleInseam { get; set; }

	public string? MaleShoe { get; set; }

	public int? EthnicityCode { get; set; }

	public string? MiddleName { get; set; }

	public bool? HasDriverLicencse { get; set; }

	public bool? HasPassport { get; set; }

	public string? FaceBook { get; set; }

	public string? Instagram { get; set; }

	public string? TikTok { get; set; }

	public string? PersonalWebSite { get; set; }

	public string? PaymentLabel1 { get; set; }

	public string? PaymentLink1 { get; set; }

	public string? PaymentLabel2 { get; set; }

	public string? PaymentLink2 { get; set; }

	public string? PaymentLabel3 { get; set; }

	public string? PaymentLink3 { get; set; }

	public string? YouTube { get; set; }

	public string? IMDB { get; set; }

	public string? SnapChat { get; set; }

	public string? SecondaryContactNumber { get; set; }

	public string? City { get; set; }

	public float? Lattitude { get; set; }

	public float? Longitude { get; set; }
}

using System;

namespace NYCastings.API.Core.Models.UserNotifications;

public class UserNotificationPrefsRequest
{
	public int Uid { get; set; }

	public int UserId { get; set; }

	public string? UserMail { get; set; }

	public string? UserMail2 { get; set; }

	public string? UserCell { get; set; }

	public string? CellProvider { get; set; }

	public string? JobCategories { get; set; }

	public string? UserSex { get; set; }

	public int? AgeFrom { get; set; }

	public int? AgeTo { get; set; }

	public string? UnionStatus { get; set; }

	public string? Ethnicity { get; set; }

	public int? HeightFrom { get; set; }

	public int? HeightTo { get; set; }

	public int? MenSuitFrom { get; set; }

	public int? MenSuitTo { get; set; }

	public string? MenShirt { get; set; }

	public decimal? MenNeckFrom { get; set; }

	public decimal? MenNeckTo { get; set; }

	public int? MenSleeveFrom { get; set; }

	public int? MenSleeveTo { get; set; }

	public int? MenWaistFrom { get; set; }

	public int? MenWaistTo { get; set; }

	public int? MenInseamFrom { get; set; }

	public int? MenInseamTo { get; set; }

	public decimal? MenShoeFrom { get; set; }

	public decimal? MenShoeTo { get; set; }

	public int? WomenDressFrom { get; set; }

	public int? WomenDressTo { get; set; }

	public int? BustFrom { get; set; }

	public int? BustTo { get; set; }

	public string? CupSize { get; set; }

	public int? WomenWaistFrom { get; set; }

	public int? WomenWaistTo { get; set; }

	public int? WomenHipsFrom { get; set; }

	public int? WomenHipsTo { get; set; }

	public decimal? WomenShoeFrom { get; set; }

	public decimal? WomenShoeTo { get; set; }

	public string? ChildSize { get; set; }

	public decimal? ShoeChildFrom { get; set; }

	public decimal? ShoeChildTo { get; set; }

	public string? RoleType { get; set; }

	public string? HairColor { get; set; }

	public byte? ActiveStatus { get; set; }

	public DateTime? DateCreated { get; set; }

	public int? WeightFrom { get; set; }

	public int? WeightTo { get; set; }

	public string? PayStatus { get; set; }

	public int? IpayStatus { get; set; }

	public bool? ActiveCell { get; set; }

	public string? CityChoice { get; set; }
}

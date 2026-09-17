using System;

namespace NYCastings.API.Core.Models.UserNotifications;

public class UserNotifPrefsModel
{
	public int UserId { get; set; }

	public string? UserEmail { get; set; }

	public string? UserEmail2 { get; set; }

	public string? UserCell { get; set; }

	public string? CellProvider { get; set; }

	public string? JobCategories { get; set; }

	public string? UserSex { get; set; }

	public int MinAge { get; set; }

	public int MaxAge { get; set; }

	public string? UnionStatus { get; set; }

	public string? Ethnicity { get; set; }

	public int MinHeight { get; set; }

	public int MaxHeight { get; set; }

	public int MenSuitMin { get; set; }

	public int MenSuitMax { get; set; }

	public string? MenShirt { get; set; }

	public decimal MenNeckMin { get; set; }

	public decimal MenNeckMax { get; set; }

	public int MenSleeveMin { get; set; }

	public int MenSleeveMax { get; set; }

	public int MenWaistMin { get; set; }

	public int MenWaistMax { get; set; }

	public int MenInseamMin { get; set; }

	public int MenInseamMax { get; set; }

	public decimal MenShoeMin { get; set; }

	public decimal MenShoeMax { get; set; }

	public int WomenDressMin { get; set; }

	public int WomenDressMax { get; set; }

	public int BustMin { get; set; }

	public int BustMax { get; set; }

	public string? CupSize { get; set; }

	public int WomenWaistMin { get; set; }

	public int WomenWaistMax { get; set; }

	public int WomenHipsMin { get; set; }

	public int WomenHipsMax { get; set; }

	public decimal WomenShoeMin { get; set; }

	public decimal WomenShoeMax { get; set; }

	public string? ChildSize { get; set; }

	public decimal ChildShoeMin { get; set; }

	public decimal ChildShoeMax { get; set; }

	public string? RoleType { get; set; }

	public string? HairColor { get; set; }

	public byte ActiveStatus { get; set; }

	public DateTime DateCreated { get; set; }

	public int WeightMin { get; set; }

	public int WeightMax { get; set; }

	public string? StrPay { get; set; }

	public int IPay { get; set; }

	public byte ActiveCell { get; set; }

	public string? CityChoice { get; set; }
}

using System;

namespace NYCastings.API.Core.Models.RoleModal;

public class GetRoleDetails
{
	public int RoleId { get; set; }

	public int NoticeId { get; set; }

	public string? RoleUnion { get; set; }

	public string? RoleName { get; set; }

	public string? Sex { get; set; }

	public int? AgeStart { get; set; }

	public int? AgeEnd { get; set; }

	public string? Ethnicity { get; set; }

	public string? RoleType { get; set; }

	public string? Payment { get; set; }

	public int? ReelRequired { get; set; }

	public int? AudioReelRequired { get; set; }

	public int? HeightStart { get; set; }

	public int? HeightEnd { get; set; }

	public int? WeightStart { get; set; }

	public int? WeightEnd { get; set; }

	public string? EyeColor { get; set; }

	public int? SuitOne { get; set; }

	public int? SuitTwo { get; set; }

	public string? Shirt { get; set; }

	public decimal? NeckOne { get; set; }

	public decimal? NeckTwo { get; set; }

	public int? SleeveOne { get; set; }

	public int? SleeveTwo { get; set; }

	public int? WaistOne { get; set; }

	public int? WaistTwo { get; set; }

	public int? InseamOne { get; set; }

	public int? InseamTwo { get; set; }

	public decimal? ShoeOne { get; set; }

	public decimal? ShoeTwo { get; set; }

	public int? DressOne { get; set; }

	public int? DressTwo { get; set; }

	public int? BustOne { get; set; }

	public int? BustTwo { get; set; }

	public string? Cup { get; set; }

	public int? FemalWaistOne { get; set; }

	public int? FemalWaistTwo { get; set; }

	public int? HipsOne { get; set; }

	public int? HipsTwo { get; set; }

	public decimal? FemaleShoeOne { get; set; }

	public decimal? FemaleShoeTwo { get; set; }

	public string? ChildSize { get; set; }

	public decimal? ChildShoeOne { get; set; }

	public decimal? ChildShoeTwo { get; set; }

	public string? OtherMeasurements { get; set; }

	public string? VocalRange { get; set; }

	public string? VocalStyle { get; set; }

	public string? MoreAboutRole { get; set; }

	public string? PhotoorScriptName { get; set; }

	public string? PhotoorScriptPath { get; set; }

	public string? ProjectId { get; set; }

	public DateTime? RecordCreatedDate { get; set; }

	public bool? ActiveFlag { get; set; }

	public bool? IsModified { get; set; }

	public DateTime? DateModified { get; set; }

	public string? ModificationMessage { get; set; }

	public string? HairColor { get; set; }
}

using System;

namespace NYCastings.API.Core.Models.UserBillingModel;

public class UserBillingModel
{
	public int UserBillingId { get; set; }

	public int UserId { get; set; }

	public int ProductRecurId { get; set; }

	public int RoleId { get; set; }

	public string? UserName { get; set; }

	public DateTime? NextBillDate { get; set; }

	public DateTime? DateRecordUpdated { get; set; }

	public bool ActiveFlag { get; set; }
}

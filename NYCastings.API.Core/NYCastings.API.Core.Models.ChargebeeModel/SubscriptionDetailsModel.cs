using System;

namespace NYCastings.API.Core.Models.ChargebeeModel;

public class SubscriptionDetailsModel
{
	public string CustomerEmail { get; set; }

	public string CustomerName { get; set; }

	public string PlanName { get; set; }

	public string SubscriptionId { get; set; }

	public string Status { get; set; }

	public DateTime? NextBillingDate { get; set; }

	public DateTime? CreatedDate { get; set; }

	public DateTime? UpdatedDate { get; set; }

	public decimal Amount { get; set; }
}

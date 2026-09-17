using System;

namespace NYCastings.API.Core.Models.ChargebeeModel;

public class PaymentSaveRequest
{
	public string InvoiceId { get; set; }

	public string SubscriptionId { get; set; }

	public string CustomerId { get; set; }

	public string Email { get; set; }

	public string PlanId { get; set; }

	public string ItemPriceId { get; set; }

	public double Amount { get; set; }

	public DateTime TermStart { get; set; }

	public DateTime TermEnd { get; set; }
}

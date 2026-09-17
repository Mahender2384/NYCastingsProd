namespace NYCastings.API.Core.Models.ChargebeeModel;

public class ChargebeePlanModel
{
	public int Id { get; set; }

	public string? PlanId { get; set; }

	public string? PlanName { get; set; }

	public string? BillingPeriod { get; set; }

	public decimal Price { get; set; }

	public string? Currency { get; set; }

	public string? ChargeModel { get; set; }
}

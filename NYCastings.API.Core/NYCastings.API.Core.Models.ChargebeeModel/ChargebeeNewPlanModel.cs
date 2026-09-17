namespace NYCastings.API.Core.Models.ChargebeeModel;

public class ChargebeeNewPlanModel
{
	public string PlanId { get; set; }

	public string Name { get; set; }

	public decimal Price { get; set; }

	public string CurrencyCode { get; set; }

	public int Period { get; set; }

	public string PeriodUnit { get; set; }

	public string Description { get; set; }
}

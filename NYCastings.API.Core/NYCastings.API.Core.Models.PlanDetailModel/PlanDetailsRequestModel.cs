namespace NYCastings.API.Core.Models.PlanDetailModel;

public class PlanDetailsRequestModel
{
	public int ProductRecurId { get; set; }

	public string? Product { get; set; }

	public string? ProductName { get; set; }

	public decimal ProductPrice { get; set; }

	public int UserId { get; set; }

	public bool ActiveFlag { get; set; }
}

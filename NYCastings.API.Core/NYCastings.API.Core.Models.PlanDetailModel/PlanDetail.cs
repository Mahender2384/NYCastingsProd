using System;

namespace NYCastings.API.Core.Models.PlanDetailModel;

public class PlanDetail
{
	public int ProductRecurId { get; set; }

	public string? Product { get; set; }

	public string? ProductName { get; set; }

	public decimal ProductPrice { get; set; }

	public DateTime RecordCreatedDate { get; set; }

	public DateTime RecordUpdatedDate { get; set; }

	public int UserId { get; set; }

	public bool ActiveFlag { get; set; }
}

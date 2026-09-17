using System.Collections.Generic;

namespace NYCastings.API.Core.Models.UserBillingModel;

public class PaginationResponse<T>
{
	public string? Status { get; set; }

	public IEnumerable<T>? Data { get; set; }

	public int Count { get; set; }
}

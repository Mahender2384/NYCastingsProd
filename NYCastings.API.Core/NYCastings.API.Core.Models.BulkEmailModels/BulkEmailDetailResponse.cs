using System.Collections.Generic;

namespace NYCastings.API.Core.Models.BulkEmailModels;

public class BulkEmailDetailResponse
{
	public int TotalCount { get; set; }

	public int Page { get; set; }

	public int PageSize { get; set; }

	public List<BulkEmailDetailModel> Items { get; set; } = new List<BulkEmailDetailModel>();
}

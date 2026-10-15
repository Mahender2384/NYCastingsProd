using System.Collections.Generic;

namespace NYCastings.API.Core.Models.BulkEmailModels;

public class BulkEmailBatchResponse
{
	public int TotalCount { get; set; }

	public int Page { get; set; }

	public int PageSize { get; set; }

	public List<BulkEmailBatchModel> Items { get; set; } = new List<BulkEmailBatchModel>();
}

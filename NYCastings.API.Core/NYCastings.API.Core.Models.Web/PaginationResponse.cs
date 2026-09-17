using System.Collections.Generic;
using System.Net;

namespace NYCastings.API.Core.Models.Web;

public class PaginationResponse
{
	public string Status { get; set; }

	public string Message { get; set; }

	public object Data { get; set; }

	public long Count { get; set; }

	public HttpStatusCode httpStatusCode { get; set; }

	public int TotalCount { get; set; }

	public IEnumerable<T> AllData { get; set; }

	public int UserId { get; set; }

	public string UserName { get; set; } = string.Empty;
}

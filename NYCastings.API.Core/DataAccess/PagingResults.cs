using System.Collections.Generic;

namespace NYCasting.Core.Models.DataAccess;

public class PagingResults<T>
{
	public IEnumerable<T> Results { get; set; }

	public long TotalCount { get; set; }
}

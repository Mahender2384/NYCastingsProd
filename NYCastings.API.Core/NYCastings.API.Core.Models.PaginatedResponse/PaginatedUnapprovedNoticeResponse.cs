using System.Collections.Generic;
using NYCastings.API.Core.Models.UnapprovedNoticeResponseModel;

namespace NYCastings.API.Core.Models.PaginatedResponse;

public class PaginatedUnapprovedNoticeResponse
{
	public List<UnapprovedNoticeModel>? Notices { get; set; }

	public int TotalCount { get; set; }
}

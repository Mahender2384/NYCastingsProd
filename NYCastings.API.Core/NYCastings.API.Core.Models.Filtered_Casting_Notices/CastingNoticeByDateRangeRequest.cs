using System;

namespace NYCastings.API.Core.Models.Filtered_Casting_Notices;

public class CastingNoticeByDateRangeRequest
{
	public int? UserId { get; set; }

	public DateTime? StartDate { get; set; }

	public DateTime? EndDate { get; set; }

	public int page { get; set; } = 1;

	public int pageSize { get; set; } = 10;
}

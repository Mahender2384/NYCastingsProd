namespace NYCastings.API.Core.Models.ExpiredNoticeRequest;

public class ExpiredNoticeRequest
{
	public int UserId { get; set; }

	public int Page { get; set; } = 1;

	public int PageSize { get; set; } = 10;
}

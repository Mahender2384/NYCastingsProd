namespace NYCastings.API.Core.Models.NoticeApprovalRequestModel;

public class NoticeStatusCountResponseModel
{
	public int ApprovedCount { get; set; }

	public int RejectedCount { get; set; }

	public int PendingCount { get; set; }

	public int TotalNotices { get; set; }
}

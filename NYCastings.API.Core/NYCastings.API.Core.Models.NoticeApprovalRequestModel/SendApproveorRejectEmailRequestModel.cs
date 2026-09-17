namespace NYCastings.API.Core.Models.NoticeApprovalRequestModel;

public class SendApproveorRejectEmailRequestModel
{
	public int NoticeId { get; set; }

	public bool IsApproved { get; set; }

	public string RejectionReason { get; set; }
}

using System;

namespace NYCastings.API.Core.Models.BulkEmailModels;

public class BulkEmailBatchModel
{
	public string BatchType { get; set; }

	public int? NoticeId { get; set; }

	public string NoticeTitle { get; set; }

	public string Subject { get; set; }

	public DateTime QueuedAt { get; set; }

	public int TotalEmails { get; set; }

	public int SentCount { get; set; }

	public int PendingCount { get; set; }

	public int FailedCount { get; set; }

	public DateTime? LastSentAt { get; set; }
}

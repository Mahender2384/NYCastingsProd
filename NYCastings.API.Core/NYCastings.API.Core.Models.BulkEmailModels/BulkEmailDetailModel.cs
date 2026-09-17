using System;

namespace NYCastings.API.Core.Models.BulkEmailModels;

public class BulkEmailDetailModel
{
	public long Id { get; set; }

	public int? NoticeId { get; set; }

	public int? RoleId { get; set; }

	public string ToEmail { get; set; }

	public string Subject { get; set; }

	public string Status { get; set; }

	public int RetryCount { get; set; }

	public DateTime CreatedAt { get; set; }

	public DateTime? ProcessedAt { get; set; }

	public DateTime? SentAt { get; set; }

	public string ErrorMessage { get; set; }
}

using System;

namespace NYCastings.API.Core.Models.ResumeViewsModel;

public class UserResumeViewModel
{
	public long? MessageId { get; set; }

	public string? FromEmail { get; set; }

	public string? ToEmail { get; set; }

	public string? Subject { get; set; }

	public string? Message { get; set; }

	public DateTime Date { get; set; }

	public string? Status { get; set; }

	public long NoticeId { get; set; }

	public int UserId { get; set; }

	public bool ActiveFlag { get; set; }

	public string? PhoneNumber { get; set; }
}

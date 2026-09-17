using System;

namespace NYCastings.API.Core.Models.ResumeViewsModel;

public class ResumeMessageModel
{
	public int MessageId { get; set; }

	public string? FromEmail { get; set; }

	public string? ToEmail { get; set; }

	public string? Subject { get; set; }

	public string? Message { get; set; }

	public DateTime Date { get; set; }

	public string? Status { get; set; }

	public long? NoticeId { get; set; }

	public int? UserId { get; set; }

	public byte? ActiveFlag { get; set; }

	public string? PhoneNumber { get; set; }
}

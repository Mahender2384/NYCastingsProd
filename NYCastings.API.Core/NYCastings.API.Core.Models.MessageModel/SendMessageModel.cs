using Microsoft.AspNetCore.Http;

namespace NYCastings.API.Core.Models.MessageModel;

public class SendMessageModel
{
	public string? MessageText { get; set; }

	public string? FromEmail { get; set; }

	public string? MessageSubject { get; set; }

	public string? ToEmail { get; set; }

	public int? NoticeId { get; set; }

	public string? RecipientName { get; set; }

	public int? TalentId { get; set; }

	public string? FileURL { get; set; }

	public IFormFile? File { get; set; }

	public int? ParentId { get; set; }

	public string? MessageFrom { get; set; }

	public bool CanReply { get; set; }

	public string? MessageFromName { get; set; }
}

using System;

namespace NYCastings.API.Core.Models.MessageModel;

public class MessageModel
{
	public int Id { get; set; }

	public string? MsgText { get; set; }

	public string? FromEmail { get; set; }

	public DateTime ConversationDate { get; set; }

	public string? Subject { get; set; }

	public string? MessageReceipt { get; set; }

	public string? JobCategory { get; set; }

	public string? TalentImage { get; set; }

	public string? FileURL { get; set; }

	public string? DirectorImage { get; set; }

	public bool? CanReply { get; set; }

	public string? SenderEmail { get; set; }

	public string? RecipientEmail { get; set; }
}

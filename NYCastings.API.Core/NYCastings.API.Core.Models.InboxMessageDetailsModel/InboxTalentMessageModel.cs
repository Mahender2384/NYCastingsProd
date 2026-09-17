using System;

namespace NYCastings.API.Core.Models.InboxMessageDetailsModel;

public class InboxTalentMessageModel
{
	public int LatestMessageId { get; set; }

	public int NoticeId { get; set; }

	public string? NoticeTitle { get; set; }

	public DateTime ConversationDate { get; set; }

	public string? MessageText { get; set; }

	public string? MessageSubject { get; set; }

	public string? TalentName { get; set; }

	public int OriginalMessageId { get; set; }

	public int TalentId { get; set; }

	public string? Status { get; set; }

	public string? TalentImage { get; set; }

	public string? DirectorImage { get; set; }
}

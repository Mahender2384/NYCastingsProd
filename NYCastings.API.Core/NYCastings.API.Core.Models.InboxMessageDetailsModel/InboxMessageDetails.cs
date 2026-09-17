using System;

namespace NYCastings.API.Core.Models.InboxMessageDetailsModel;

public class InboxMessageDetails
{
	public int NoticeId { get; set; }

	public string? NoticeTitle { get; set; }

	public int LatestMessageId { get; set; }

	public int MessageId { get; set; }

	public DateTime ConversationDate { get; set; }

	public string? MessageText { get; set; }

	public string? MessageSubject { get; set; }

	public string? TalentName { get; set; }

	public int OriginalMessageId { get; set; }

	public int TalentId { get; set; }

	public string? Status { get; set; }

	public string? StatusOfTalent { get; set; }

	public string? TalentImageURL { get; set; }

	public string? DirectorImageURL { get; set; }
}

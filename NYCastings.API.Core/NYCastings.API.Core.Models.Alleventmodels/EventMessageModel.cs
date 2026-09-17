using System;

namespace NYCastings.API.Core.Models.Alleventmodels;

public class EventMessageModel
{
	public int EventMessageId { get; set; }

	public int EventId { get; set; }

	public string EventName { get; set; }

	public int AudienceTypeId { get; set; }

	public string AudienceName { get; set; }

	public string Subject { get; set; }

	public string MessageBody { get; set; }

	public DateTime CreatedAt { get; set; }

	public DateTime? UpdatedAt { get; set; }

	public DateTime? LastSentAt { get; set; }
}

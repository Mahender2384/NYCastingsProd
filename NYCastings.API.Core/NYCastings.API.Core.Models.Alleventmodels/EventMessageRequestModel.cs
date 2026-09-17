namespace NYCastings.API.Core.Models.Alleventmodels;

public class EventMessageRequestModel
{
	public int EventMessageId { get; set; }

	public int EventId { get; set; }

	public int AudienceTypeId { get; set; }

	public string Subject { get; set; }

	public string MessageBody { get; set; }

	public int UserId { get; set; }
}

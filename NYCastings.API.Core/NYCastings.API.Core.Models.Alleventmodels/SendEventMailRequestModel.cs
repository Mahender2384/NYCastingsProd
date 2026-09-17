namespace NYCastings.API.Core.Models.Alleventmodels;

public class SendEventMailRequestModel
{
	public int AudienceTypeId { get; set; }

	public int EventMessageId { get; set; }

	public string Subject { get; set; }

	public string MessageBody { get; set; }
}

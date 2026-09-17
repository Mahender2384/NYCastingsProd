namespace NYCastings.API.Core.Models.ChargebeeModel;

public class ChargebeeWebhookModel
{
	public string id { get; set; }

	public string event_type { get; set; }

	public WebhookContent content { get; set; }
}

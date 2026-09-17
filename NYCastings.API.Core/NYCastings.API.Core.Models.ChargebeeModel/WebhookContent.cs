namespace NYCastings.API.Core.Models.ChargebeeModel;

public class WebhookContent
{
	public WebhookCustomer customer { get; set; }

	public WebhookSubscription subscription { get; set; }
}

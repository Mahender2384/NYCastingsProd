namespace NYCastings.API.Core.Models.ChargebeeModel;

public class SubscriberDeactivationResult
{
	public string Email { get; set; }

	public string ChargebeeStatus { get; set; }

	public string Action { get; set; }

	public string Message { get; set; }
}

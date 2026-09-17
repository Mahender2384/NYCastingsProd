namespace NYCastings.API.Core.Models.NotificationMessageModel;

public class NotificationMessageRequest
{
	public int Id { get; set; }

	public string? Title { get; set; }

	public string? Message { get; set; }

	public string? ForRole { get; set; }

	public bool ActiveFlag { get; set; } = true;
}

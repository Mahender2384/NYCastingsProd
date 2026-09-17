using System;

namespace NYCastings.API.Core.Models.NotificationMessageModel;

public class NotificationMessageResponse
{
	public int NotificationId { get; set; }

	public string? Title { get; set; }

	public string? Message { get; set; }

	public string? ForRole { get; set; }

	public DateTime CreatedAt { get; set; }

	public bool ActiveFlag { get; set; }
}

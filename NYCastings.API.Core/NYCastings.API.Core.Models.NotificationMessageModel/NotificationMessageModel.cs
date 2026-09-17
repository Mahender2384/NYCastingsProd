using System;

namespace NYCastings.API.Core.Models.NotificationMessageModel;

public class NotificationMessageModel
{
	public int ID { get; set; }

	public string? Title { get; set; }

	public string? Message { get; set; }

	public DateTime CreatedAt { get; set; }
}

using System;

namespace NYCastings.API.Core.Models.Alleventmodels;

public class EventModel
{
	public int EventId { get; set; }

	public string EventName { get; set; }

	public string EventDescription { get; set; }

	public DateTime CreatedAt { get; set; }

	public DateTime? UpdatedAt { get; set; }
}

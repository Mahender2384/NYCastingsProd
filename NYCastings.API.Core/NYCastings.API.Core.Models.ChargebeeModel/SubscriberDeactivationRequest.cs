using System.Collections.Generic;

namespace NYCastings.API.Core.Models.ChargebeeModel;

public class SubscriberDeactivationRequest
{
	public List<string> Emails { get; set; }

	public bool ApplyChanges { get; set; }
}

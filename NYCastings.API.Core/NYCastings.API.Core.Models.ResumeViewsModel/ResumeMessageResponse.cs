using System.Collections.Generic;

namespace NYCastings.API.Core.Models.ResumeViewsModel;

public class ResumeMessageResponse
{
	public List<ResumeMessageModel>? Messages { get; set; }

	public ResumeMessageCounts? Counts { get; set; }
}

using System.Collections.Generic;

namespace NYCastings.API.Core.Models.ArchiveMessageDetailsModel;

public class ArchiveMessageRequest
{
	public List<int> OriginalMsgIds { get; set; }

	public bool ActiveFlag { get; set; }

	public string? ArchiveFrom { get; set; }
}

using System.Collections.Generic;

namespace NYCastings.API.Core.Models.DeleteMessagesModel;

public class DeleteMessages
{
	public List<int> OriginalMsgIds { get; set; }

	public bool ActiveFlag { get; set; }

	public bool PermanentArchive { get; set; }

	public string UserId { get; set; }
}

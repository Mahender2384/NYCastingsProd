using System.Collections.Generic;

namespace NYCastings.API.Core.Models.TalentFullProfileModel;

public class UserTalentUpdateModel
{
	public int UserId { get; set; }

	public int UserBy { get; set; }

	public List<int>? TalentIds { get; set; }
}

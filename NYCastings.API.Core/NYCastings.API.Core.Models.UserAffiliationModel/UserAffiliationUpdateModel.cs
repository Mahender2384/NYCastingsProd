using System.Collections.Generic;

namespace NYCastings.API.Core.Models.UserAffiliationModel;

public class UserAffiliationUpdateModel
{
	public int UserId { get; set; }

	public int UserBy { get; set; }

	public List<int> AffiliationIds { get; set; } = new List<int>();
}

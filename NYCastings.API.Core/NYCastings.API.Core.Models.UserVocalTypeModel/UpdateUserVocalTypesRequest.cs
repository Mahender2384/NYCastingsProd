using System.Collections.Generic;

namespace NYCastings.API.Core.Models.UserVocalTypeModel;

public class UpdateUserVocalTypesRequest
{
	public int UserId { get; set; }

	public int UserBy { get; set; }

	public List<int> VocalTypeIds { get; set; }
}

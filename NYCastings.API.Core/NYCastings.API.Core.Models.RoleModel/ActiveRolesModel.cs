using System;

namespace NYCastings.API.Core.Models.RoleModel;

public class ActiveRolesModel
{
	public int RoleId { get; set; }

	public string? RoleName { get; set; }

	public DateTime RecordCreatedDate { get; set; }

	public bool ActiveFlag { get; set; }
}

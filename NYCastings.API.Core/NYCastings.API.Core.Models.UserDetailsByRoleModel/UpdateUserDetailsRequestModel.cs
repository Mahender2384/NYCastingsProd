namespace NYCastings.API.Core.Models.UserDetailsByRoleModel;

public class UpdateUserDetailsRequestModel
{
	public int UserId { get; set; }

	public string? FirstName { get; set; }

	public string? LastName { get; set; }

	public string? LoginName { get; set; }

	public string? Email { get; set; }

	public string? Password { get; set; }

	public bool? ActiveFlag { get; set; }

	public bool? IsHide { get; set; }

	public bool? IsBlock { get; set; }
}

namespace NYCastings.API.Core.Models.UserDetailsByRoleModel;

public class UserDetailsByRoleModel
{
	public int UserId { get; set; }

	public string? RoleName { get; set; }

	public string? FirstName { get; set; }

	public string? LastName { get; set; }

	public string? LoginName { get; set; }

	public string? Email { get; set; }

	public string? UserStatus { get; set; }

	public string? Password { get; set; }

	public string? TalentImage { get; set; }

	public int? UserCredits { get; set; }
}

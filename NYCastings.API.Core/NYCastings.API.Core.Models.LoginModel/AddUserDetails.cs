using System.Collections.Generic;

namespace NYCastings.API.Core.Models.LoginModel;

public class AddUserDetails
{
	public string? FirstName { get; set; }

	public string? LastName { get; set; }

	public string? Email { get; set; }

	public string? UserName { get; set; }

	public string? Password { get; set; }

	public int? AccountType { get; set; }

	public string? State { get; set; }

	public string? Phone { get; set; }

	public string? CompanyName { get; set; }

	public List<int> AffiliationName { get; set; } = new List<int>();
}

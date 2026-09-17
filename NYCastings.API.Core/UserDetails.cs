using System;

public class UserDetails
{
	public string? UserFirstName { get; set; }

	public string? UserLastName { get; set; }

	public string? UserRole { get; set; }

	public int RoleId { get; set; }

	public int UserId { get; set; }

	public DateTime? RecordCreatedDate { get; set; }

	public string? City { get; set; }

	public string? State { get; set; }

	public string? Email { get; set; }

	public string? Login { get; set; }

	public bool? ActiveFlag { get; set; }

	public string? CompanyName { get; set; }

	public string? PhoneNumber { get; set; }

	public bool VerifiedDirector { get; set; }

	public int TotalCredits { get; set; }
}

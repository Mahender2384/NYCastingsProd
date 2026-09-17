using Microsoft.AspNetCore.Http;

namespace NYCastings.API.Core.Models.ClientDetailsModel;

public class UpdateClientDetailsModel
{
	public int UserId { get; set; }

	public string? Password { get; set; }

	public string? ContactEmail { get; set; }

	public string? FirstName { get; set; }

	public string? LastName { get; set; }

	public string? PhonNumber { get; set; }

	public string? CompanyName { get; set; }

	public string? Address1 { get; set; }

	public string? Address2 { get; set; }

	public string? City { get; set; }

	public string? State1 { get; set; }

	public string? Zip { get; set; }

	public IFormFile? Logo { get; set; }

	public string? FileURL { get; set; }
}

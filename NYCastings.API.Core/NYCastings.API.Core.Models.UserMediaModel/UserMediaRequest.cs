using Microsoft.AspNetCore.Http;

namespace NYCastings.API.Core.Models.UserMediaModel;

public class UserMediaRequest
{
	public int? MediaId { get; set; }

	public int UserId { get; set; }

	public string? MediaType { get; set; }

	public string? MediaStyle { get; set; }

	public string? MediaTitle { get; set; }

	public int PicSort { get; set; }

	public bool ActiveFlag { get; set; }

	public IFormFile? MediaFile { get; set; }

	public int UserBy { get; set; }

	public string? MediaLink { get; set; }
}

namespace NYCastings.API.Core.Models.UserVocalTypeModel;

public class UserVocalTypeModel
{
	public int UserVocalTypeId { get; set; }

	public int UserId { get; set; }

	public int VocalTypeId { get; set; }

	public string? VocalTypeName { get; set; }

	public bool ActiveFlag { get; set; }
}

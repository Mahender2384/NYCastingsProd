namespace NYCastings.API.Core.Models.UpdateUserinListModel;

public class UpdateUserListModel
{
	public int UserId { get; set; }

	public int RoleId { get; set; }

	public char CurrentList { get; set; }

	public char TargetList { get; set; }
}

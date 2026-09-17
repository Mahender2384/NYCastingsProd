namespace NYCastings.API.Core.Contracts.JobCategoryModel;

public class JobCategory
{
	public int JobCategoryId { get; set; }

	public string? JobCategoryName { get; set; }

	public byte HasDailySheet { get; set; }

	public short DailySheetOrder { get; set; }

	public bool ActiveFlag { get; set; }
}

namespace NYCastings.API.Core.Models.FileUploadModel;

public class UploadedFileResult
{
	public string FileName { get; set; }

	public string Url { get; set; }

	public long SizeBytes { get; set; }
}

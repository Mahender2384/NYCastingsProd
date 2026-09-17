namespace NYCastings.API.Core.Models.NotesModel;

public class SaveTalentFavoriteNoticeRequestModel
{
	public int TalentId { get; set; }

	public int NoticeId { get; set; }

	public bool? IsFavorite { get; set; }
}

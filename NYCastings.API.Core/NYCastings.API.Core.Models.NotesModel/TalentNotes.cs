using System;

namespace NYCastings.API.Core.Models.NotesModel;

public class TalentNotes
{
	public int? NoteId { get; set; }

	public int TalentId { get; set; }

	public int DirectorId { get; set; }

	public string? NoteText { get; set; }

	public DateTime CreatedAt { get; set; }
}

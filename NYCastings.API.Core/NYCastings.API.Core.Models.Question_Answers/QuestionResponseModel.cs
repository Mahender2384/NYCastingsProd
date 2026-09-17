using System;

namespace NYCastings.API.Core.Models.Question_Answers;

public class QuestionResponseModel
{
	public int QuestionId { get; set; }

	public string QuestionText { get; set; }

	public string AnswerText { get; set; }

	public DateTime CreatedDate { get; set; }

	public string Category { get; set; }

	public int? Position { get; set; }
}

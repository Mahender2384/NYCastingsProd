namespace NYCastings.API.Core.Models.Question_Answers;

public class QuestionModel
{
	public int QuestionId { get; set; }

	public string QuestionText { get; set; }

	public string AnswerText { get; set; }

	public int UserId { get; set; }

	public string Category { get; set; }

	public int Position { get; set; }
}

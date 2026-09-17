namespace NYCastings.API.Infrastructure.Helpers;

public class ErrorResponse
{
	public string Message { get; set; }

	public string CorrelationId { get; set; }

	public string RequestPath { get; set; }
}

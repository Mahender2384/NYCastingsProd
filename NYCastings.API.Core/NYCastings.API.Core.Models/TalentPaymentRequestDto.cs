namespace NYCastings.API.Core.Models;

public class TalentPaymentRequestDto
{
	public string? Token { get; set; }

	public string? CustomerId { get; set; }

	public long AmountInCents { get; set; }

	public string? Currency { get; set; }
}

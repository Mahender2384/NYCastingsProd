using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using NYCasting.Core.Models;
using NYCasting.Infrastructure.DataAccess;
using SendGrid;
using SendGrid.Helpers.Mail;

public class EmailService
{
	private readonly DbManager _dbManager;

	private const string SendGridApiKey = "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";

	private const string FromEmailAlerts = "alerts@directsubmit.com";

	private const string FromEmailCasting = "casting@directsubmit.com";

	private const string FromEmailHelp = "help@directsubmit.com";

	private const string FromName = "DirectSubmit";

	private readonly List<string> DefaultCcEmails = new List<string> { "subscriptions@nycastings.com", "Casting@DirectSubmit.com" };

	private readonly List<string> DefaultBccEmails = new List<string> { "directsubmit-developer@aptimized.com", "photo212@aol.com" };

	private readonly List<string> NoticeSubmitCcEmails = new List<string> { "Casting@DirectSubmit.com", "photo212@aol.com" };

	private const int DelayBetweenEmailsMs = 200;

	private const int MaxRetries = 3;

	private const int BatchSize = 100;

	public EmailService(IOptions<ConnectionString> dbConfig)
	{
		_dbManager = new DbManager(dbConfig.Value.NYCasting);
	}

	public bool SendEmail(string toEmail, string subject, string body)
	{
		try
		{
			SendGridClient client = new SendGridClient("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");
			EmailAddress emailAddress = new EmailAddress("help@directsubmit.com", "DirectSubmit");
			EmailAddress to = new EmailAddress(toEmail);
			SendGridMessage msg = MailHelper.CreateSingleEmail(emailAddress, to, subject, null, body);
			foreach (string bcc in DefaultCcEmails)
			{
				msg.AddBcc(new EmailAddress(bcc));
			}
			Response response = client.SendEmailAsync(msg).GetAwaiter().GetResult();
			if (response.StatusCode == HttpStatusCode.Accepted || response.StatusCode == HttpStatusCode.OK)
			{
				Console.WriteLine("✅ Email sent to " + toEmail);
				return true;
			}
			string responseBody = response.Body.ReadAsStringAsync().GetAwaiter().GetResult();
			Console.WriteLine($"❌ SendGrid error: {response.StatusCode} - {responseBody}");
			return false;
		}
		catch (Exception ex)
		{
			Console.WriteLine("❌ Email exception: " + ex.Message);
			return false;
		}
	}

	public bool SendTalentWelcomeEmail(string toEmail, string subject, string body)
	{
		try
		{
			SendGridClient sendGridClient = new SendGridClient("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");
			EmailAddress emailAddress = new EmailAddress("alerts@directsubmit.com", "DirectSubmit");
			EmailAddress to = new EmailAddress(toEmail);
			SendGridMessage msg = MailHelper.CreateSingleEmail(emailAddress, to, subject, null, body);
			Response response = sendGridClient.SendEmailAsync(msg).GetAwaiter().GetResult();
			if (response.StatusCode == HttpStatusCode.Accepted || response.StatusCode == HttpStatusCode.OK)
			{
				Console.WriteLine("✅ Talent welcome email sent to " + toEmail);
				return true;
			}
			string responseBody = response.Body.ReadAsStringAsync().GetAwaiter().GetResult();
			Console.WriteLine($"❌ SendGrid error: {response.StatusCode} - {responseBody}");
			return false;
		}
		catch (Exception ex)
		{
			Console.WriteLine("❌ Email exception: " + ex.Message);
			return false;
		}
	}

	public bool SendDirectorWelcomeEmail(string toEmail, string subject, string body)
	{
		try
		{
			SendGridClient sendGridClient = new SendGridClient("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");
			EmailAddress emailAddress = new EmailAddress("casting@directsubmit.com", "DirectSubmit");
			EmailAddress to = new EmailAddress(toEmail);
			SendGridMessage msg = MailHelper.CreateSingleEmail(emailAddress, to, subject, null, body);
			Response response = sendGridClient.SendEmailAsync(msg).GetAwaiter().GetResult();
			if (response.StatusCode == HttpStatusCode.Accepted || response.StatusCode == HttpStatusCode.OK)
			{
				Console.WriteLine("✅ Director welcome email sent to " + toEmail);
				return true;
			}
			string responseBody = response.Body.ReadAsStringAsync().GetAwaiter().GetResult();
			Console.WriteLine($"❌ SendGrid error: {response.StatusCode} - {responseBody}");
			return false;
		}
		catch (Exception ex)
		{
			Console.WriteLine("❌ Email exception: " + ex.Message);
			return false;
		}
	}

	public bool SendEmailForNoticeSubmit(string toEmail, string subject, string body)
	{
		try
		{
			SendGridClient client = new SendGridClient("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");
			EmailAddress emailAddress = new EmailAddress("casting@directsubmit.com", "DirectSubmit");
			EmailAddress to = new EmailAddress(toEmail);
			SendGridMessage msg = MailHelper.CreateSingleEmail(emailAddress, to, subject, null, body);
			foreach (string bcc in NoticeSubmitCcEmails)
			{
				msg.AddBcc(new EmailAddress(bcc));
			}
			Response response = client.SendEmailAsync(msg).GetAwaiter().GetResult();
			if (response.StatusCode == HttpStatusCode.Accepted || response.StatusCode == HttpStatusCode.OK)
			{
				Console.WriteLine("✅ Notice submit email sent to " + toEmail);
				return true;
			}
			string responseBody = response.Body.ReadAsStringAsync().GetAwaiter().GetResult();
			Console.WriteLine($"❌ SendGrid error: {response.StatusCode} - {responseBody}");
			return false;
		}
		catch (Exception ex)
		{
			Console.WriteLine("❌ Email exception: " + ex.Message);
			return false;
		}
	}

	public bool SendRoleAlertEmail(string toEmail, string subject, string body)
	{
		try
		{
			SendGridClient sendGridClient = new SendGridClient("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");
			EmailAddress emailAddress = new EmailAddress("alerts@directsubmit.com", "DirectSubmit");
			EmailAddress to = new EmailAddress(toEmail);
			SendGridMessage msg = MailHelper.CreateSingleEmail(emailAddress, to, subject, null, body);
			Response response = sendGridClient.SendEmailAsync(msg).GetAwaiter().GetResult();
			if (response.StatusCode == HttpStatusCode.Accepted || response.StatusCode == HttpStatusCode.OK)
			{
				Console.WriteLine("✅ Role Alert email sent to " + toEmail);
				return true;
			}
			string responseBody = response.Body.ReadAsStringAsync().GetAwaiter().GetResult();
			Console.WriteLine($"❌ SendGrid error: {response.StatusCode} - {responseBody}");
			return false;
		}
		catch (Exception ex)
		{
			Console.WriteLine("❌ Email exception: " + ex.Message);
			return false;
		}
	}

	public async Task SendBulkEmailsAsync(List<string> recipients, string subject, string htmlBody)
	{
		int totalSent = 0;
		foreach (List<string> batch in GetBatches(recipients, 100))
		{
			foreach (string recipient in batch)
			{
				int attempt = 0;
				bool sent = false;
				while (attempt < 3 && !sent)
				{
					try
					{
						SendGridClient client = new SendGridClient("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");
						EmailAddress emailAddress = new EmailAddress("alerts@directsubmit.com", "DirectSubmit");
						EmailAddress to = new EmailAddress(recipient);
						SendGridMessage msg = MailHelper.CreateSingleEmail(emailAddress, to, subject, null, htmlBody);
						foreach (string bcc in DefaultBccEmails)
						{
							msg.AddBcc(new EmailAddress(bcc));
						}
						Response response = await client.SendEmailAsync(msg);
						if (response.StatusCode == HttpStatusCode.Accepted || response.StatusCode == HttpStatusCode.OK)
						{
							sent = true;
							totalSent++;
							Console.WriteLine("✅ Sent: " + recipient);
							continue;
						}
						throw new Exception($"SendGrid status: {response.StatusCode}");
					}
					catch (Exception ex)
					{
						attempt++;
						Console.WriteLine($"❌ Failed: {recipient} (Attempt {attempt}) - {ex.Message}");
						if (attempt < 3)
						{
							await Task.Delay(1000);
						}
					}
				}
				await Task.Delay(200);
			}
		}
		Console.WriteLine($"\ud83c\udf89 Finished sending. Total sent: {totalSent}");
	}

	public async Task SendQueuedEmailsAsync()
	{
		DataTable dt = _dbManager.ReadData("USP_EMAILQUEUE_DEQUEUE", CommandType.StoredProcedure, new Dictionary<string, object> { { "@BatchSize", 500 } });
		if (dt == null || dt.Rows.Count == 0)
		{
			return;
		}
		SemaphoreSlim semaphore = new SemaphoreSlim(20);
		await Task.WhenAll(dt.Rows.Cast<DataRow>().Select((Func<DataRow, Task>)async delegate(DataRow row)
		{
			await semaphore.WaitAsync();
			try
			{
				long id = Convert.ToInt64(row["Id"]);
				string toEmail = row["ToEmail"]?.ToString()?.Trim() ?? "";
				string subject = row["Subject"]?.ToString() ?? "";
				string body = row["Body"]?.ToString() ?? "";
				bool success = SendRoleAlertEmail(toEmail, subject, body);
				UpdateQueueStatus(id, success, success ? null : "SendGrid send failed");
			}
			catch (Exception ex)
			{
				long id2 = Convert.ToInt64(row["Id"]);
				string msg = ((ex.Message.Length > 900) ? ex.Message.Substring(0, 900) : ex.Message);
				UpdateQueueStatus(id2, success: false, msg);
			}
			finally
			{
				semaphore.Release();
				await Task.Delay(200);
			}
		}));
	}

	public Task PurgeOldQueuedEmailsAsync()
	{
		_dbManager.ReadData("USP_EMAILQUEUE_PURGE", CommandType.StoredProcedure, new Dictionary<string, object> { { "@RetentionDays", 30 } });
		return Task.CompletedTask;
	}

	private void UpdateQueueStatus(long id, bool success, string? errorMessage)
	{
		_dbManager.InsertOrUpdateData("USP_EMAILQUEUE_UPDATE_STATUS", CommandType.StoredProcedure, new Dictionary<string, object>
		{
			{ "@Id", id },
			{ "@Success", success },
			{
				"@ErrorMessage",
				string.IsNullOrEmpty(errorMessage) ? ((IConvertible)DBNull.Value) : ((IConvertible)errorMessage)
			}
		});
	}

	private List<List<string>> GetBatches(List<string> list, int batchSize)
	{
		List<List<string>> batches = new List<List<string>>();
		for (int i = 0; i < list.Count; i += batchSize)
		{
			batches.Add(list.GetRange(i, Math.Min(batchSize, list.Count - i)));
		}
		return batches;
	}

	public bool SendSubscriptionActiveEmail(string toEmail, string userName, string talentName = null)
	{
		string subject = "Your DirectSubmit Subscription is Active!";
		string body = BuildSubscriptionEmailBody(isActive: true, userName, talentName);
		return SendSubscriptionEmail(toEmail, subject, body);
	}

	public bool SendSubscriptionInactiveEmail(string toEmail, string userName, string talentName = null)
	{
		string subject = "Your DirectSubmit Subscription Has Been Cancelled";
		string body = BuildSubscriptionEmailBody(isActive: false, userName, talentName);
		return SendSubscriptionEmail(toEmail, subject, body);
	}

	private bool SendSubscriptionEmail(string toEmail, string subject, string body)
	{
		try
		{
			SendGridClient sendGridClient = new SendGridClient("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");
			EmailAddress emailAddress = new EmailAddress("help@directsubmit.com", "DirectSubmit");
			EmailAddress to = new EmailAddress(toEmail);
			SendGridMessage msg = MailHelper.CreateSingleEmail(emailAddress, to, subject, null, body);
			Response response = sendGridClient.SendEmailAsync(msg).GetAwaiter().GetResult();
			return response.StatusCode == HttpStatusCode.Accepted || response.StatusCode == HttpStatusCode.OK;
		}
		catch (Exception ex)
		{
			Console.WriteLine("❌ Subscription email exception: " + ex.Message);
			return false;
		}
	}

	private string BuildSubscriptionEmailBody(bool isActive, string userName, string talentName = null)
	{
		string displayName = (string.IsNullOrWhiteSpace(talentName) ? "there" : talentName);
		string safeUser = (string.IsNullOrWhiteSpace(userName) ? "" : userName);
		string heading = (isActive ? "Your subscription is active!" : "Your subscription has been cancelled");
		string bodyContent = ((!isActive) ? $"\r\n            <p style='margin:0 0 16px;'>Hi {displayName},</p>\r\n            <p style='margin:0 0 16px;'>\r\n              This is to confirm that your DirectSubmit / NYCastings subscription\r\n              has been cancelled and your account is now inactive.\r\n            </p>\r\n            <p style='margin:0 0 16px;'>\r\n              We&apos;re sorry to see you go. If you&apos;d like to continue submitting\r\n              to casting notices, you can reactivate your subscription anytime.\r\n            </p>\r\n            <p style='margin:0 0 4px;'>Talent Name: {displayName}</p>\r\n            <p style='margin:0 0 20px;'>User: {safeUser}</p>\r\n            <p style='margin:0 0 24px;'>\r\n              <a href='https://directsubmit.nycastings.com/login'\r\n                 style='background:#789eeb;color:#ffffff;padding:12px 28px;\r\n                        text-decoration:none;border-radius:4px;font-size:14px;\r\n                        font-weight:bold;display:inline-block;'>\r\n                Reactivate Your Subscription\r\n              </a>\r\n            </p>" : $"\r\n            <p style='margin:0 0 16px;'>Hey {displayName},</p>\r\n            <p style='margin:0 0 16px;'>Congratulations on your new subscription!</p>\r\n            <p style='margin:0 0 16px;'>\r\n              We want to welcome you to DirectSubmit / NYCastings and we sincerely\r\n              hope that it will help you book more work!\r\n            </p>\r\n            <p style='margin:0 0 16px;'><strong>Your info was received, now just make sure that you fill out your\r\n              resume right away and start submitting to the casting notices.</strong></p>\r\n            <p style='margin:0 0 4px;'>Talent Name: {displayName}</p>\r\n            <p style='margin:0 0 4px;'>User: {safeUser}</p>\r\n            <p style='margin:0 0 20px;'>Password: ********</p>\r\n            <p style='margin:0 0 10px;'>Here are some things to help you get started:</p>\r\n            <p style='margin:0 0 24px;'>\r\n              <strong>1.</strong> Create your Resume as best as possible before submitting to anything.\r\n              You only have 1 chance to create a first impression!\r\n            </p>\r\n            <p style='margin:0 0 24px;'>\r\n              <a href='https://directsubmit.nycastings.com/login'\r\n                 style='background:#789eeb;color:#ffffff;padding:12px 28px;\r\n                        text-decoration:none;border-radius:4px;font-size:14px;\r\n                        font-weight:bold;display:inline-block;'>\r\n                Login to Your Account\r\n              </a>\r\n            </p>");
		return $"<!DOCTYPE html>\r\n        <html>\r\n        <head>\r\n          <meta charset='UTF-8'/>\r\n          <meta name='viewport' content='width=device-width, initial-scale=1.0'/>\r\n        </head>\r\n        <body style='margin:0;padding:0;background:#ffffff;font-family:Arial,sans-serif;font-size:14px;color:#333333;'>\r\n        <table width='100%' cellpadding='0' cellspacing='0' border='0' style='background:#ffffff;'>\r\n        <tr><td align='center' style='padding:20px 0;'>\r\n          <table width='600' cellpadding='0' cellspacing='0' border='0' style='background:#ffffff;'>\r\n            <tr>\r\n              <td style='padding:20px 0 10px;text-align:center;'>\r\n                <img src='https://files.constantcontact.com/82886fe2301/302cc6b2-5b9a-4673-8b39-143010d6b262.jpg?rdr=true'\r\n                     alt='DirectSubmit' width='300'\r\n                     style='display:inline-block;max-width:300px;border:0;'/>\r\n              </td>\r\n            </tr>\r\n            <tr>\r\n              <td style='padding:10px 30px 0;'>\r\n                <h2 style='margin:0 0 16px;font-size:20px;color:#333333;'>{heading}</h2>\r\n              </td>\r\n            </tr>\r\n            <tr>\r\n              <td style='padding:0 30px 20px;font-family:Arial,sans-serif;font-size:14px;color:#333333;line-height:1.6;'>\r\n                {bodyContent}\r\n              </td>\r\n            </tr>\r\n            <tr>\r\n              <td style='padding:0 30px;'>\r\n                <hr style='border:none;border-top:1px solid #dddddd;margin:0;'/>\r\n              </td>\r\n            </tr>\r\n            <tr>\r\n              <td style='padding:14px 20px;text-align:center;font-family:Arial,sans-serif;font-size:12px;color:#888888;line-height:1.6;'>\r\n                <p style='margin:0 0 4px;'>\r\n                  By using the DirectSubmit platform, you agree to the terms and conditions of our\r\n                  <a href='https://directsubmit.nycastings.com/terms-and-condition' target='_blank' style='color:#789eeb;text-decoration:underline;'>Terms of Service</a>\r\n                  and\r\n                  <a href='https://directsubmit.nycastings.com/privacy-policy' target='_blank' style='color:#789eeb;text-decoration:underline;'>Privacy Policy</a>.\r\n                  &copy; DirectSubmit\r\n                </p>\r\n                <p style='margin:0;text-align:center;background:#333333;padding:10px;font-size:13px;color:#ffffff;'>\r\n                  <a href='https://directsubmit.nycastings.com' style='color:#789eeb;text-decoration:none;'>DirectSubmit.com</a>\r\n                  &nbsp;|&nbsp; 1480 Vine St. &nbsp;|&nbsp; Los Angeles, CA 90028\r\n                </p>\r\n              </td>\r\n            </tr>\r\n          </table>\r\n        </td></tr>\r\n        </table>\r\n        </body>\r\n        </html>";
	}
}

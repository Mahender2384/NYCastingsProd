using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NYCastings.API.Core.Contracts.SearchNoticeInterface;
using NYCastings.API.Core.Models.DirectSubmitHistoryRequest;
using NYCastings.API.Core.Models.ExpiredNoticeRequest;
using NYCastings.API.Core.Models.Filtered_Casting_Notices;
using NYCastings.API.Core.Models.JobHistoryModel;
using NYCastings.API.Core.Models.LocationModel;
using NYCastings.API.Core.Models.ResumeViewsModel;
using NYCastings.API.Core.Models.SaveCastingSearchModel;
using NYCastings.API.Core.Models.UserEmail;
using NYCastings.API.Core.Models.UserNotifications;
using NYCastings.API.Core.Models.Web;

namespace NYCastings.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SearchNoticeController : ControllerBase
{
	private readonly ILogger<UserController> _logger;

	private readonly ISearchNoticeInterface _searchNoticeInterface;

	public SearchNoticeController(ILogger<UserController> logger, ISearchNoticeInterface searchNoticeInterface)
	{
		_logger = logger;
		_searchNoticeInterface = searchNoticeInterface;
	}

	[HttpPost("GetFilteredNotices")]
	public IActionResult GetFilteredNotices([FromBody] CastingNoticeFilterRequest request)
	{
		try
		{
			_logger.LogInformation("Fetching casting notices with filters: {@Request}", request);
			IEnumerable<CastingNoticeResponseModel> result = _searchNoticeInterface.GetFilteredCastingNotices(request);
			List<CastingNoticeResponseModel> paginatedResults = result.Skip((request.page - 1) * request.pageSize).Take(request.pageSize).ToList();
			return Ok(new PaginationResponse
			{
				Status = "Ok",
				Data = paginatedResults,
				Count = result.Count()
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error retrieving casting notices");
			return StatusCode(500, ex);
		}
	}

	[HttpPost("GetExpiredFilteredNotices")]
	public IActionResult GetExpiredFilteredNotices([FromBody] ExpiredNoticeRequest request)
	{
		try
		{
			if (request == null || request.UserId <= 0)
			{
				return BadRequest("Invalid request.");
			}
			_logger.LogInformation("Fetching expired casting notices for UserId: {UserId}", request.UserId);
			IEnumerable<CastingNoticeResponseModel> result = _searchNoticeInterface.GetExpiredFilteredCastingNotices(request.UserId);
			int totalCount = result.Count();
			List<CastingNoticeResponseModel> paginatedResults = result.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList();
			return Ok(new PaginationResponse
			{
				Status = "Ok",
				Data = paginatedResults,
				Count = totalCount
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error retrieving expired casting notices for UserId: {UserId}", request?.UserId);
			return StatusCode(500, "Internal server error.");
		}
	}

	[HttpGet("GetLocationsByStateAbbreviations/{stateAbbrCsv}")]
	public IActionResult GetLocationsByStateAbbreviations(string stateAbbrCsv)
	{
		try
		{
			_logger.LogInformation("Starting retrieval of locations for state abbreviations: {StateAbbrCsv}", stateAbbrCsv);
			IEnumerable<LocationModel> result = _searchNoticeInterface.GetLocationsByStateAbbreviations(stateAbbrCsv);
			_logger.LogInformation("Successfully retrieved locations for state abbreviations: {StateAbbrCsv}", stateAbbrCsv);
			return Ok(result);
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error occurred while retrieving locations for state abbreviations: {StateAbbrCsv}", stateAbbrCsv);
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpPost("SubmitaJob")]
	public async Task<IActionResult> SubmitDirectHistory([FromForm] DirectSubmitHistoryRequest model)
	{
		try
		{
			string fileBase64 = null;
			if (model.MediaFile != null)
			{
				using MemoryStream ms = new MemoryStream();
				await model.MediaFile.CopyToAsync(ms);
				byte[] fileBytes = ms.ToArray();
				fileBase64 = Convert.ToBase64String(fileBytes);
			}
			var fullPayload = new
			{
				UserID = model.UserID,
				Type = model.Type,
				NoticeID = model.NoticeID,
				From = model.From,
				To = model.To,
				NoticeTitle = model.NoticeTitle,
				CoverMsg = model.CoverMsg,
				HtmlFormat = model.HtmlFormat,
				RoleId = model.RoleId,
				MediaURL = model.MediaURL,
				VideoPath = model.VideoPath,
				TalentName = model.TalentName,
				DirectorName = model.DirectorName,
				SubmissionLink = model.SubmissionLink,
				ImageURL = model.ImageURL,
				MediaFile = ((model.MediaFile != null) ? new
				{
					FileName = model.MediaFile.FileName,
					ContentType = model.MediaFile.ContentType,
					Length = model.MediaFile.Length,
					FileContentBase64 = fileBase64
				} : null)
			};
			string payloadJson = JsonConvert.SerializeObject(fullPayload);
			_logger.LogInformation("Full payload: {Payload}", payloadJson);
			Task<string> message = _searchNoticeInterface.SubmitDirectHistory(model);
			return Ok(new
			{
				Message = message
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error submitting direct history");
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpGet("GetUserJobHistory/{userId}")]
	public IActionResult GetUserJobHistory(int userId)
	{
		try
		{
			_logger.LogInformation("Retrieving job history for userId: {UserId}", userId);
			IEnumerable<JobHistoryModel> result = _searchNoticeInterface.GetUserJobHistory(userId);
			_logger.LogInformation("Successfully retrieved job history for userId: {UserId}", userId);
			return Ok(result);
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error retrieving job history for userId: {UserId}", userId);
			return StatusCode(500, "An error occurred while retrieving job history.");
		}
	}

	[HttpGet("GetUserResumeMessagesWithCounts")]
	public IActionResult GetUserResumeMessagesWithCounts(int userId, int pageNumber, int pageSize)
	{
		try
		{
			_logger.LogInformation("Retrieving resume messages and counts for userId: {UserId}, Page: {PageNumber}, PageSize: {PageSize}", userId, pageNumber, pageSize);
			ResumeMessageResponse result = _searchNoticeInterface.GetResumeMessagesWithStatusCounts(userId);
			List<ResumeMessageModel> allMessages = result.Messages.ToList();
			int totalCount = allMessages.Count;
			List<ResumeMessageModel> pagedMessages = allMessages.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
			PaginationResponse paginationResponse = new PaginationResponse
			{
				Status = "Success",
				Message = "Resume messages retrieved successfully.",
				Data = new ResumeMessageResponse
				{
					Messages = pagedMessages,
					Counts = result.Counts
				},
				Count = pagedMessages.Count,
				TotalCount = totalCount,
				httpStatusCode = HttpStatusCode.OK
			};
			_logger.LogInformation("Successfully returned paginated messages for userId: {UserId}, Page: {PageNumber}, PageSize: {PageSize}", userId, pageNumber, pageSize);
			return Ok(paginationResponse);
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error retrieving resume messages and counts for userId: {UserId}", userId);
			PaginationResponse errorResponse = new PaginationResponse
			{
				Status = "Error",
				Message = "An error occurred while retrieving resume messages and counts.",
				Data = null,
				Count = 0L,
				TotalCount = 0,
				AllData = null,
				httpStatusCode = HttpStatusCode.InternalServerError
			};
			return StatusCode(500, errorResponse);
		}
	}

	[HttpPost("add-or-updateResumeViews")]
	public IActionResult AddOrUpdateUserResumeView([FromBody] UserResumeViewModel model)
	{
		try
		{
			_logger.LogInformation("Attempting to save resume view for UserId: {UserId}, MessageId: {MessageId}", model.UserId, model.MessageId);
			if (_searchNoticeInterface.AddOrUpdateUserResumeView(model))
			{
				_logger.LogInformation("Successfully saved resume view for UserId: {UserId}, MessageId: {MessageId}", model.UserId, model.MessageId);
				return Ok("User resume view saved successfully.");
			}
			_logger.LogError("Failed to save resume view for UserId: {UserId}, MessageId: {MessageId}", model.UserId, model.MessageId);
			return StatusCode(500, "Failed to save user resume view.");
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Exception occurred while saving resume view for UserId: {UserId}, MessageId: {MessageId}", model.UserId, model.MessageId);
			return StatusCode(500, "An unexpected error occurred.");
		}
	}

	[HttpPost("add-or-updateUserNotificationPrefs")]
	public IActionResult AddOrUpdateUserNotificationPrefs([FromBody] UserNotificationPrefsRequest model)
	{
		try
		{
			_logger.LogInformation("AddOrUpdateUserNotificationPrefs: Attempting to save preferences for UserId: {UserId}", model.UserId);
			if (_searchNoticeInterface.AddOrUpdateUserNotificationPrefs(model))
			{
				_logger.LogInformation("AddOrUpdateUserNotificationPrefs: Successfully saved preferences for UserId: {UserId}", model.UserId);
				return Ok(new
				{
					message = "User notification preferences saved successfully."
				});
			}
			_logger.LogError("AddOrUpdateUserNotificationPrefs: Failed to save preferences for UserId: {UserId}", model.UserId);
			return StatusCode(500, "An error occurred while saving preferences.");
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "AddOrUpdateUserNotificationPrefs: Exception occurred for UserId: {UserId}", model.UserId);
			return StatusCode(500, "An internal error occurred.");
		}
	}

	[HttpGet("GetUserNotificationPrefs")]
	public IActionResult GetUserNotifPrefsByUserId(int userId)
	{
		_logger.LogInformation("GetUserNotifPrefsByUserId called with userId: {UserId}", userId);
		try
		{
			IEnumerable<UserNotifPrefsModel> result = _searchNoticeInterface.GetUserNotifPrefsByUserId(userId);
			if (!result.Any())
			{
				_logger.LogWarning("No notification preferences found for userId: {UserId}", userId);
				return Ok(new
				{
					message = "There are no preference for this user"
				});
			}
			_logger.LogInformation("Notification preferences retrieved successfully for userId: {UserId}", userId);
			return Ok(result);
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error retrieving notification preferences for userId: {UserId}", userId);
			return StatusCode(500, "An error occurred while retrieving user notification preferences.");
		}
	}

	[HttpPost("SendRoleAlertEmails")]
	public IActionResult SendCityRoleAlertEmails([FromBody] RoleAlertEmailRequest request)
	{
		_logger.LogInformation("SendCityRoleAlertEmails called with cityChoices: {CityChoices}", request.CityChoices);
		try
		{
			Task.Run(async delegate
			{
				try
				{
					await _searchNoticeInterface.SendCityBasedRoleAlertEmails(request);
					_logger.LogInformation("Background email sending completed.");
				}
				catch (Exception exception2)
				{
					_logger.LogError(exception2, "Error during background email sending.");
				}
			});
			return Ok(new
			{
				message = "Emails are being sent in the background."
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error initiating city-based role alert emails.");
			return StatusCode(500, "An error occurred while initiating email sending.");
		}
	}

	[HttpPost("SaveCastingSearch")]
	public PaginationResponse SaveCastingSearch([FromBody] SaveCastingSearchRequest model)
	{
		try
		{
			bool result = _searchNoticeInterface.SaveUserCastingSearch(model);
			return new PaginationResponse
			{
				Status = "Ok",
				Data = result
			};
		}
		catch (Exception ex)
		{
			return new PaginationResponse
			{
				Message = ex.Message
			};
		}
	}

	[HttpGet("GetCastingSearch")]
	public PaginationResponse GetCastingSearch(string email)
	{
		try
		{
			SaveCastingSearchRequest result = _searchNoticeInterface.GetUserCastingSearch(email);
			return new PaginationResponse
			{
				Status = "Ok",
				Data = result
			};
		}
		catch (Exception ex)
		{
			return new PaginationResponse
			{
				Message = ex.Message
			};
		}
	}
}

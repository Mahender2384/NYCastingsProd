using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NYCastings.API.Core.Contracts.JobCategoryModel;
using NYCastings.API.Core.Contracts.UserInterface;
using NYCastings.API.Core.Models.ChangePasswordRequestModel;
using NYCastings.API.Core.Models.CountryCodeModel;
using NYCastings.API.Core.Models.EthnicityModel;
using NYCastings.API.Core.Models.EyeColorModel;
using NYCastings.API.Core.Models.HairColorModel;
using NYCastings.API.Core.Models.LoginModel;
using NYCastings.API.Core.Models.NotificationMessageModel;
using NYCastings.API.Core.Models.PayTypeModel;
using NYCastings.API.Core.Models.RepresentationHeadingModel;
using NYCastings.API.Core.Models.TalentModel;
using NYCastings.API.Core.Models.UserAffiliationModel;
using NYCastings.API.Core.Models.UserNavigationStageResponse;
using NYCastings.API.Core.Models.UserProvacySettings;
using NYCastings.API.Core.Models.UserVocalTypeModel;
using NYCastings.API.Core.Models.VocalRangeModel;
using NYCastings.API.Core.Models.Web;

namespace NYCastings.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
	private readonly ILogger<UserController> _logger;

	private readonly IUserService _userService;

	public UserController(ILogger<UserController> logger, IUserService userService)
	{
		_logger = logger;
		_userService = userService;
	}

	[HttpGet("Authenticate")]
	public object Authenticate(string username, string password)
	{
		try
		{
			_logger.LogInformation("Authenticate method called");
			object results = _userService.Authenticate(username, password);
			_logger.LogInformation("User Authenticated successfully");
			return results;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while Authenticating user details");
			return new PaginationResponse
			{
				Status = "Error",
				Message = ex.Message
			};
		}
	}

	[Authorize]
	[HttpGet("GetUserdetails")]
	public PaginationResponse GetUserDetails(string username, string password)
	{
		try
		{
			_logger.LogInformation("GetUserdetails method called");
			UserDetails results = _userService.GetUserDetails(username, password);
			_logger.LogInformation("User details fetched successfully");
			return new PaginationResponse
			{
				Status = "Ok",
				Data = results
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while getting user details");
			return new PaginationResponse
			{
				Status = "Error",
				Message = ex.Message
			};
		}
	}

	[HttpPost("AddNewUserDetails")]
	public PaginationResponse AddNewUserDetails(AddUserDetails addUserDetails)
	{
		try
		{
			_logger.LogInformation("AddNewUserDetails method called");
			bool results = _userService.AddUserDetails(addUserDetails);
			_logger.LogInformation("User details added successfully");
			return new PaginationResponse
			{
				Status = "Ok",
				Data = results
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while adding user details");
			return new PaginationResponse
			{
				Status = "Error",
				Message = ex.Message
			};
		}
	}

	[HttpGet("GetLocations")]
	public IActionResult GetLocations()
	{
		try
		{
			_logger.LogInformation("GetLocations method called at {Timestamp}", DateTime.UtcNow);
			IEnumerable<LocationModel> locations = _userService.GetLocations();
			return Ok(new
			{
				Status = "Ok",
				Data = locations
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while fetching locations at {Timestamp}", DateTime.UtcNow);
			return StatusCode(500, new
			{
				Status = "Error",
				Message = ex.Message
			});
		}
	}

	[HttpGet("get-all-ethnicities")]
	public IActionResult GetAllEthnicities()
	{
		try
		{
			IEnumerable<EthnicityModel> data = _userService.GetAllEthnicities();
			return Ok(new
			{
				status = "Ok",
				data = data
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while fetching ethnicity list");
			return StatusCode(500, new
			{
				status = "Error",
				message = ex.Message
			});
		}
	}

	[HttpGet("get-vocal-ranges")]
	public IActionResult GetVocalRanges()
	{
		try
		{
			IEnumerable<VocalRangeModel> result = _userService.GetAllVocalRanges();
			return Ok(new
			{
				status = "Ok",
				data = result
			});
		}
		catch (Exception ex)
		{
			return StatusCode(500, new
			{
				status = "Error",
				message = ex.Message
			});
		}
	}

	[HttpGet("get-eye-colors")]
	public IActionResult GetEyeColors()
	{
		try
		{
			IEnumerable<EyeColorModel> result = _userService.GetAllEyeColors();
			return Ok(new
			{
				status = "Ok",
				data = result
			});
		}
		catch (Exception ex)
		{
			return StatusCode(500, new
			{
				status = "Error",
				message = ex.Message
			});
		}
	}

	[HttpGet("get-hair-colors")]
	public IActionResult GetHairColors()
	{
		try
		{
			IEnumerable<HairColorModel> result = _userService.GetAllHairColors();
			return Ok(new
			{
				status = "Ok",
				data = result
			});
		}
		catch (Exception ex)
		{
			return StatusCode(500, new
			{
				status = "Error",
				message = ex.Message
			});
		}
	}

	[HttpGet("active-talents")]
	public IActionResult GetActiveTalents()
	{
		try
		{
			IEnumerable<TalentModel> result = _userService.GetActiveTalents();
			return Ok(result);
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error fetching active talents.");
			return StatusCode(500, "Internal server error.");
		}
	}

	[HttpGet("allAffiliations")]
	public IActionResult GetAllAffiliations()
	{
		try
		{
			IEnumerable<AffiliationModel> affiliations = _userService.GetAllAffiliations();
			return Ok(affiliations);
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error fetching affiliations");
			return StatusCode(500, "Internal server error");
		}
	}

	[HttpGet("GetVocalTypes")]
	public IActionResult GetVocalTypes()
	{
		try
		{
			IEnumerable<VocalTypeModel> result = _userService.GetVocalTypes();
			return Ok(result);
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error occurred while retrieving vocal types.");
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpGet("GetUserNavigation/{userId}")]
	public IActionResult GetStage(int userId)
	{
		_logger.LogInformation("Received request to get navigation stage for UserId: {UserId}", userId);
		if (userId <= 0)
		{
			_logger.LogWarning("Invalid UserId received: {UserId}", userId);
			return BadRequest("Invalid User ID.");
		}
		try
		{
			UserNavigationStageResponse result = _userService.GetUserNavigationStage(userId);
			if (result == null)
			{
				_logger.LogInformation("No navigation stage found for UserId: {UserId}", userId);
				return NotFound("User not found.");
			}
			_logger.LogInformation("Returning navigation stage {Stage} for UserId: {UserId}", result.Stage, userId);
			return Ok(result);
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error occurred while retrieving navigation stage for UserId: {UserId}", userId);
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpGet("get-all-paytypes")]
	public IActionResult GetAllPayTypes()
	{
		try
		{
			IEnumerable<PayTypeModel> data = _userService.GetAllPayTypes();
			return Ok(new
			{
				status = "Ok",
				data = data
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while fetching pay type list");
			return StatusCode(500, new
			{
				status = "Error",
				message = ex.Message
			});
		}
	}

	[HttpGet("get-all-job-categories")]
	public IActionResult GetAllJobCategories()
	{
		try
		{
			IEnumerable<JobCategory> data = _userService.GetAllJobCategories();
			return Ok(new
			{
				status = "Ok",
				data = data
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while fetching job categories");
			return StatusCode(500, new
			{
				status = "Error",
				message = ex.Message
			});
		}
	}

	[HttpPost("ChangePassword")]
	public IActionResult ChangePassword([FromBody] ChangePasswordRequestModel request)
	{
		if (!base.ModelState.IsValid)
		{
			return BadRequest(base.ModelState);
		}
		if (_userService.ChangeUserPassword(request))
		{
			return Ok(new
			{
				StatusCode = 1,
				Message = "Password changed successfully."
			});
		}
		return BadRequest(new
		{
			StatusCode = 0,
			Message = "Old password incorrect or update failed."
		});
	}

	[HttpGet("GetActiveNotifications")]
	public IActionResult GetActiveNotifications(int userId, string userRole)
	{
		try
		{
			_logger.LogInformation("Fetching active notifications for UserId: {UserId} and Role: {UserRole}", userId, userRole);
			IEnumerable<NotificationMessageModel> data = _userService.GetActiveNotificationsByUser(userId, userRole);
			return Ok(new
			{
				Status = "Ok",
				Message = "Active notifications retrieved successfully.",
				Data = data
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error fetching active notifications for UserId: {UserId}", userId);
			return StatusCode(500, new
			{
				Status = "Error",
				Message = "An error occurred while retrieving notifications."
			});
		}
	}

	[HttpPost("ForgotPassword")]
	public IActionResult ForgotPassword([FromBody] ForgotPasswordRequestModel request)
	{
		if (!base.ModelState.IsValid)
		{
			return BadRequest(base.ModelState);
		}
		if (_userService.ForgotPassword(request, out string _))
		{
			return Ok(new
			{
				StatusCode = 1,
				Message = "A new password has been sent to your email."
			});
		}
		return Ok(new
		{
			StatusCode = 0,
			Message = "No account found for this email address. Please check the email and try again."
		});
	}

	[HttpGet("get-all-representation-headings")]
	public IActionResult GetAllRepresentationHeadings()
	{
		try
		{
			IEnumerable<RepresentationHeadingModel> data = _userService.GetRepresentationHeadings();
			return Ok(new
			{
				status = "Ok",
				data = data
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while fetching representation headings");
			return StatusCode(500, new
			{
				status = "Error",
				message = ex.Message
			});
		}
	}

	[HttpGet("get-custom-affiliations")]
	public IActionResult GetCustomAffiliations()
	{
		try
		{
			IEnumerable<Affiliation2Model> data = _userService.GetCustomOrderedAffiliations();
			return Ok(new
			{
				status = "Ok",
				data = data
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while fetching affiliations");
			return StatusCode(500, new
			{
				status = "Error",
				message = ex.Message
			});
		}
	}

	[HttpPost("ContactUsDetails")]
	public PaginationResponse SendContactUsEmail(string name, string email, string department, string message)
	{
		try
		{
			_logger.LogInformation("Contact Us method called");
			bool results = _userService.SendContactUsEmail(name, email, department, message);
			_logger.LogInformation("Contact Us Email sent successfully");
			return new PaginationResponse
			{
				Status = "Ok",
				Data = results
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while sending Contactus Email details");
			return new PaginationResponse
			{
				Status = "Error",
				Message = ex.Message
			};
		}
	}

	[HttpGet("GetUserName")]
	public PaginationResponse GetUserName(int userId)
	{
		try
		{
			_logger.LogInformation("GetUserName method called");
			string results = _userService.GetUserName(userId);
			return new PaginationResponse
			{
				Status = "Ok",
				UserName = results
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while calling GetuserName ");
			return new PaginationResponse
			{
				Status = "Error",
				Message = ex.Message
			};
		}
	}

	[HttpGet("GetUserId")]
	public PaginationResponse GetUserId(string userName)
	{
		try
		{
			_logger.LogInformation("GetUserId method called");
			int results = _userService.GetUserId(userName);
			return new PaginationResponse
			{
				Status = "Ok",
				UserId = results
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while calling GetuserId ");
			return new PaginationResponse
			{
				Status = "Error",
				Message = ex.Message
			};
		}
	}

	[HttpPost("UpdateResumeName")]
	public IActionResult UpdateResumeName(int userId, string name)
	{
		try
		{
			_logger.LogInformation("UpdateResumeName method called");
			if (_userService.UpdateResumeName(userId, name))
			{
				return Ok(new
				{
					Status = "Ok",
					Message = "Resume name updated successfully"
				});
			}
			return BadRequest(new
			{
				Status = "Error",
				Message = "Failed to update resume name"
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while updating resume name");
			return StatusCode(500, new
			{
				Status = "Error",
				Message = ex.Message
			});
		}
	}

	[HttpPost("SaveUserPrivacySettings")]
	public IActionResult SaveUserPrivacySettings([FromBody] SaveUserPrivacySettingsRequestModel model)
	{
		try
		{
			_logger.LogInformation("Saving user privacy settings: {@Model}", model);
			if (model.UserId <= 0)
			{
				return BadRequest("Invalid UserId.");
			}
			bool result = _userService.SaveUserPrivacySettings(model);
			return Ok(new
			{
				Status = "Success",
				Message = "User privacy settings saved successfully.",
				Result = result
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error saving user privacy settings");
			return StatusCode(500, "Internal server error");
		}
	}

	[HttpGet("GetUserPrivacySettings")]
	public IActionResult GetUserPrivacySettings(int userId)
	{
		try
		{
			_logger.LogInformation("Fetching privacy settings for UserId: {UserId}", userId);
			if (userId <= 0)
			{
				return BadRequest("Invalid UserId.");
			}
			UserPrivacySettingsResponseModel result = _userService.GetUserPrivacySettings(userId);
			if (result == null)
			{
				return NotFound("Privacy settings not found for this user.");
			}
			return Ok(new
			{
				Status = "Success",
				Data = result
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error fetching user privacy settings");
			return StatusCode(500, "Internal server error");
		}
	}

	[HttpPost("ForgotUsername")]
	public IActionResult ForgotUsername([FromBody] ForgotUsernameRequestModel request)
	{
		if (!base.ModelState.IsValid)
		{
			return BadRequest(base.ModelState);
		}
		if (_userService.ForgotUsername(request))
		{
			return Ok(new
			{
				StatusCode = 1,
				Message = "Your username has been sent to your email."
			});
		}
		return Ok(new
		{
			StatusCode = 0,
			Message = "No account found for this Email address. Please check the Email and try again."
		});
	}

	[HttpGet("IsVerifiedDirector")]
	public PaginationResponse IsVerifiedDirector(int userId)
	{
		try
		{
			_logger.LogInformation("IsVerifiedDirector method called for UserId: {UserId}", userId);
			bool result = _userService.IsVerifiedDirector(userId);
			_logger.LogInformation("Verified director check completed successfully");
			return new PaginationResponse
			{
				Status = "Ok",
				Data = result
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while checking verified director");
			return new PaginationResponse
			{
				Status = "Error",
				Message = ex.Message
			};
		}
	}

	[HttpGet("GetCountryCodes")]
	public IActionResult GetCountryCodes()
	{
		try
		{
			IEnumerable<CountryCodeModel> countries = _userService.GetCountryCodes();
			return Ok(new
			{
				status = "Success",
				data = countries
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Failed to load country codes.");
			return StatusCode(500, new
			{
				status = "Error",
				message = "Unable to load country codes."
			});
		}
	}

	[HttpGet("UnsubscribeUser")]
	public IActionResult Unsubscribe([FromQuery] int userId, [FromQuery] string email)
	{
		if (userId <= 0)
		{
			return BadRequest("Invalid unsubscribe request.");
		}
		bool isSuccess = _userService.UnsubscribeUser(userId, email);
		string message = (isSuccess ? "You have been unsubscribed and will no longer receive Role Alert emails." : "We couldn't process your unsubscribe request. Please try again later.");
		return Content($"<html><body style='font-family:Arial;text-align:center;padding:40px;'><h2>{(isSuccess ? "Unsubscribed" : "Something went wrong")}</h2><p>{message}</p></body></html>", "text/html");
	}

	[HttpGet("UnsubscribeEventMail")]
	public IActionResult UnsubscribeEventMail([FromQuery] int userId, [FromQuery] string email)
	{
		if (userId <= 0)
		{
			return BadRequest("Invalid unsubscribe request.");
		}
		bool isSuccess = _userService.UnsubscribeEventMail(userId, email);
		string message = (isSuccess ? "You have been unsubscribed and will no longer receive event emails." : "We couldn't process your unsubscribe request. Please try again later.");
		return Content($"<html><body style='font-family:Arial;text-align:center;padding:40px;'><h2>{(isSuccess ? "Unsubscribed" : "Something went wrong")}</h2><p>{message}</p></body></html>", "text/html");
	}
}

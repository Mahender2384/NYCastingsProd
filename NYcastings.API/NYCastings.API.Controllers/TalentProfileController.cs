using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NYCastings.API.Core.Contracts.TalentProfileInterface;
using NYCastings.API.Core.Models.NotesModel;
using NYCastings.API.Core.Models.RepresentationInfoModel;
using NYCastings.API.Core.Models.TalentFullProfileModel;
using NYCastings.API.Core.Models.UserAffiliationModel;
using NYCastings.API.Core.Models.UserMediaModel;
using NYCastings.API.Core.Models.UserResumeTextDataModel;
using NYCastings.API.Core.Models.UserVocalTypeModel;

namespace NYCastings.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TalentProfileController : ControllerBase
{
	private readonly ILogger<UserController> _logger;

	private readonly ITalentProfileService _talentProfileService;

	public TalentProfileController(ILogger<UserController> logger, ITalentProfileService talentProfileService)
	{
		_logger = logger;
		_talentProfileService = talentProfileService;
	}

	[HttpGet("get-talent-profile/{userId}")]
	public IActionResult GetTalentProfile(int userId)
	{
		try
		{
			_logger.LogInformation("Fetching full talent profile for user: {UserId}", userId);
			TalentFullProfileModel result = _talentProfileService.GetTalentFullProfile(userId);
			return Ok(new
			{
				status = "Ok",
				data = result
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while fetching full talent profile for user: {UserId}", userId);
			return StatusCode(500, new
			{
				status = "Error",
				message = ex.Message
			});
		}
	}

	[HttpPost("update-talent-profile")]
	[HttpPost("update-profile")]
	public async Task<IActionResult> UpdateTalentProfile([FromBody] TalentProfileUpdateModel model)
	{
		try
		{
			if (model == null)
			{
				return BadRequest(new
				{
					status = "Error",
					message = "Invalid request data."
				});
			}
			string modelJson = JsonSerializer.Serialize(model);
			_logger.LogInformation("Updating talent profile. Request Model: {Model}", modelJson);
			bool result = await _talentProfileService.UpdateTalentProfile(model);
			return Ok(new
			{
				status = (result ? "Ok" : "Error"),
				message = (result ? "Profile updated successfully." : "Profile update failed.")
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error updating talent profile for user: {UserId}", model?.UserId);
			return StatusCode(500, new
			{
				status = "Error",
				message = "Something went wrong while updating profile."
			});
		}
	}

	[HttpPost("AddorUpdateTypeofTalent")]
	public IActionResult AddOrSyncUserTalents([FromBody] UserTalentUpdateModel model)
	{
		if (model == null || model.TalentIds == null)
		{
			_logger.LogWarning("Invalid input received in AddOrSyncUserTalents: {Model}", model);
			return BadRequest("Invalid input");
		}
		try
		{
			_logger.LogInformation("Processing AddOrSyncUserTalents for UserId: {UserId} by UserBy: {UserBy} with TalentIds: {TalentIds}", model.UserId, model.UserBy, string.Join(",", model.TalentIds));
			if (_talentProfileService.AddOrSyncUserTalents(model))
			{
				_logger.LogInformation("Successfully updated talents for UserId: {UserId}", model.UserId);
				return Ok("User talents updated successfully.");
			}
			_logger.LogError("Failed to update talents for UserId: {UserId}", model.UserId);
			return StatusCode(500, "Failed to update talents.");
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Exception occurred while updating talents for UserId: {UserId}", model.UserId);
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpGet("GetUserTalents/{userId}")]
	public IActionResult GetUserTalents(int userId)
	{
		try
		{
			IEnumerable<UserTalentModel> result = _talentProfileService.GetUserTalents(userId);
			return Ok(result);
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error occurred while retrieving user talents for userId: {UserId}", userId);
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpGet("GetUserAffiliations/{userId}")]
	public IActionResult GetUserAffiliations(int userId)
	{
		try
		{
			IEnumerable<UserAffiliationModel> result = _talentProfileService.GetUserAffiliations(userId);
			return Ok(result);
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error fetching affiliations for UserId: {UserId}", userId);
			return StatusCode(500, "Internal server error");
		}
	}

	[HttpPost("AddorUpdateAffiliations")]
	public IActionResult AddOrSyncUserAffiliations([FromBody] UserAffiliationUpdateModel model)
	{
		try
		{
			if (_talentProfileService.AddOrSyncUserAffiliations(model))
			{
				return Ok(new
				{
					Success = true,
					Message = "Affiliations synced successfully."
				});
			}
			return BadRequest(new
			{
				Success = false,
				Message = "Failed to sync affiliations."
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error while syncing user affiliations");
			return StatusCode(500, new
			{
				Success = false,
				Message = "Internal Server Error."
			});
		}
	}

	[HttpGet("GetUserVocalTypes/{userId}")]
	public IActionResult GetUserVocalTypes(int userId)
	{
		try
		{
			IEnumerable<UserVocalTypeModel> result = _talentProfileService.GetUserVocalTypes(userId);
			return Ok(result);
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error fetching user vocal types");
			return StatusCode(500, "Internal server error");
		}
	}

	[HttpPost("AddorUpdateUserVocalTypes")]
	public IActionResult AddOrSyncUserVocalTypes([FromBody] UpdateUserVocalTypesRequest model)
	{
		try
		{
			_talentProfileService.AddOrSyncUserVocalTypes(model);
			return Ok(new
			{
				Message = "Vocal types updated successfully"
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error updating vocal types");
			return StatusCode(500, "Internal server error");
		}
	}

	[HttpGet("GetUserMedia/{userId}")]
	public IActionResult GetUserMedia(int userId)
	{
		try
		{
			_logger.LogInformation("Fetching media for userId: {UserId}", userId);
			IEnumerable<UserMediaModel> result = _talentProfileService.GetUserMedia(userId);
			return Ok(result);
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error fetching media for userId: {UserId}", userId);
			return StatusCode(500, "An error occurred while fetching user media.");
		}
	}

	[HttpGet("GetUserResumeTextData/{userId}")]
	public IActionResult GetUserResumeTextData(int userId)
	{
		_logger.LogInformation("Fetching resume text data for user ID {UserId}", userId);
		try
		{
			IEnumerable<UserResumeTextDataModel> result = _talentProfileService.GetUserResumeTextData(userId);
			return Ok(result);
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error fetching resume text data for user ID {UserId}", userId);
			return StatusCode(500, "An error occurred while fetching data.");
		}
	}

	[HttpGet("GetResumeSectionsWithDetails/{userId}")]
	public IActionResult GetResumeSectionsWithDetails(int userId)
	{
		try
		{
			_logger.LogInformation($"Fetching resume sections with details for UserId: {userId}");
			IEnumerable<ResumeSectionWithDetailsModel> result = _talentProfileService.GetResumeSectionsWithDetails(userId);
			return Ok(result);
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, $"Error occurred while fetching resume sections for UserId: {userId}");
			return StatusCode(500, "An error occurred while fetching data.");
		}
	}

	[HttpPost("AddOrUpdateResumeText")]
	public IActionResult AddOrUpdateResumeText([FromBody] ResumeTextDataModel model)
	{
		try
		{
			_logger.LogInformation("AddOrUpdateResumeTextData called for UserId: {UserId}", model.UserId);
			if (_talentProfileService.AddOrUpdateResumeTextData(model))
			{
				return Ok(new
				{
					Success = true,
					Message = "Resume text data saved successfully."
				});
			}
			return BadRequest(new
			{
				Success = false,
				Message = "Failed to save resume text data."
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error in AddOrUpdateResumeTextData for UserId: {UserId}", model.UserId);
			return StatusCode(500, new
			{
				Success = false,
				Message = "An error occurred while saving data."
			});
		}
	}

	[HttpPost("AddOrUpdateResumeSection")]
	public IActionResult AddOrUpdateResumeSection([FromBody] ResumeSectionRequest model)
	{
		_logger.LogInformation("AddOrUpdateResumeSection called for UserId: {UserId}, ResumeSectionId: {ResumeSectionId}", model.UserId, model.ResumeSectionId);
		try
		{
			if (_talentProfileService.AddOrUpdateResumeSection(model))
			{
				_logger.LogInformation("Resume section saved successfully for UserId: {UserId}", model.UserId);
				return Ok(new
				{
					success = true,
					message = "Resume section saved successfully."
				});
			}
			_logger.LogWarning("Failed to save resume section for UserId: {UserId}", model.UserId);
			return BadRequest(new
			{
				success = false,
				message = "Failed to save resume section."
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "An error occurred while saving resume section for UserId: {UserId}", model.UserId);
			return StatusCode(500, new
			{
				success = false,
				message = "An error occurred while saving resume section."
			});
		}
	}

	[HttpPost("AddOrUpdateResumeSectionDetails")]
	public IActionResult AddOrUpdateResumeSectionDetail([FromBody] ResumeSectionDetailRequest model)
	{
		_logger.LogInformation("AddOrUpdateResumeSectionDetail called. ResumeSectionDetailId: {ResumeSectionDetailId}, UserId: {UserId}", model.ResumeSectionDetailId, model.UserId);
		try
		{
			if (_talentProfileService.AddOrUpdateResumeSectionDetail(model))
			{
				_logger.LogInformation("Resume section detail saved successfully for ResumeSectionId: {ResumeSectionId}", model.ResumeSectionId);
				return Ok(new
				{
					success = true,
					message = "Resume section detail saved successfully."
				});
			}
			_logger.LogWarning("Failed to save resume section detail for ResumeSectionId: {ResumeSectionId}", model.ResumeSectionId);
			return BadRequest(new
			{
				success = false,
				message = "Failed to save resume section detail."
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "An error occurred while saving resume section detail.");
			return StatusCode(500, new
			{
				success = false,
				message = "An error occurred while saving resume section detail."
			});
		}
	}

	[DisableRequestSizeLimit]
	[RequestFormLimits(MultipartBodyLengthLimit = 524288000L)]
	[HttpPost("AddOrUpdateUserMedia")]
	public async Task<IActionResult> AddOrUpdateUserMedia([FromForm] UserMediaRequest model, CancellationToken cancellationToken)
	{
		if (!base.ModelState.IsValid)
		{
			return BadRequest(new
			{
				success = false,
				message = "Invalid request data."
			});
		}
		try
		{
			_logger.LogInformation("AddOrUpdateUserMedia called for UserId: {UserId}", model.UserId);
			if (await _talentProfileService.AddOrUpdateUserMediaAsync(model, cancellationToken))
			{
				_logger.LogInformation("User media successfully saved for UserId: {UserId}", model.UserId);
				return Ok(new
				{
					success = true,
					message = "Media saved successfully."
				});
			}
			_logger.LogWarning("Failed to save user media for UserId: {UserId}", model.UserId);
			return BadRequest(new
			{
				success = false,
				message = "Failed to save media."
			});
		}
		catch (ArgumentException ex)
		{
			_logger.LogWarning(ex, "Invalid argument in AddOrUpdateUserMedia for UserId: {UserId}", model.UserId);
			return BadRequest(new
			{
				success = false,
				message = ex.Message
			});
		}
		catch (InvalidOperationException ex2)
		{
			_logger.LogWarning(ex2, "Invalid operation in AddOrUpdateUserMedia for UserId: {UserId}", model.UserId);
			return BadRequest(new
			{
				success = false,
				message = ex2.Message
			});
		}
		catch (OperationCanceledException)
		{
			_logger.LogWarning("Upload cancelled for UserId: {UserId}", model.UserId);
			return StatusCode(499, new
			{
				success = false,
				message = "Upload was cancelled."
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Exception in AddOrUpdateUserMedia for UserId: {UserId}", model.UserId);
			return StatusCode(500, new
			{
				success = false,
				message = "An error occurred while saving media."
			});
		}
	}

	[HttpGet("GetRepresentationInformation/{userId}/{headingName}")]
	public IActionResult GetRepresentationInformation(int userId, string headingName)
	{
		try
		{
			_logger.LogInformation("Starting retrieval of representation information for userId: {UserId} with heading: {HeadingName}", userId, headingName);
			IEnumerable<RepresentationInfoModel> result = _talentProfileService.GetRepresentationInformation(userId, headingName);
			_logger.LogInformation("Successfully retrieved representation information for userId: {UserId} with heading: {HeadingName}", userId, headingName);
			return Ok(result);
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error occurred while retrieving representation information for userId: {UserId} with heading: {HeadingName}", userId, headingName);
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpPost("AddOrUpdateRepresentationInformation")]
	public IActionResult AddOrUpdateRepresentationInformation([FromBody] RepresentationInfoRequestModel model)
	{
		try
		{
			_logger.LogInformation("AddOrUpdateRepresentationInformation called for UserId: {UserId}, HeadingName: {HeadingName}", model.UserId, model.HeadingName);
			_talentProfileService.AddOrUpdateRepresentationInformation(model);
			_logger.LogInformation("Successfully added or updated representation information for UserId: {UserId}, HeadingName: {HeadingName}", model.UserId, model.HeadingName);
			return Ok(new
			{
				Message = "Representation information saved successfully."
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error occurred while adding or updating representation information for UserId: {UserId}", model.UserId);
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpPost("SaveTalentFavoriteNotice")]
	public IActionResult SaveTalentFavoriteNotice([FromBody] SaveTalentFavoriteNoticeRequestModel model)
	{
		try
		{
			_logger.LogInformation("Saving talent favorite notice: {@Model}", model);
			if (model.TalentId <= 0)
			{
				return BadRequest("Invalid TalentId.");
			}
			if (model.NoticeId <= 0)
			{
				return BadRequest("Invalid NoticeId.");
			}
			bool result = _talentProfileService.SaveTalentFavoriteNotice(model);
			return Ok(new
			{
				Status = "Success",
				Message = "Talent favorite notice saved successfully.",
				Result = result
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error saving talent favorite notice");
			return StatusCode(500, "Internal server error");
		}
	}
}

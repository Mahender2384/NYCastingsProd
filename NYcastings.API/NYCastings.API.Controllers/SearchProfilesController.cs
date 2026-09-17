using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NYCastings.API.Core.Contracts.ProfileSearchInterface;
using NYCastings.API.Core.Models.HideUnhideTalentModel;
using NYCastings.API.Core.Models.ProfileSearchModel;
using NYCastings.API.Core.Models.Web;

namespace NYCastings.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SearchProfilesController : ControllerBase
{
	private readonly ILogger<UserController> _logger;

	private readonly IProfileSearchInterface _profileSearchService;

	public SearchProfilesController(ILogger<UserController> logger, IProfileSearchInterface profileSearchService)
	{
		_logger = logger;
		_profileSearchService = profileSearchService;
	}

	[HttpGet("SearchTalentProfiles")]
	public IActionResult SearchTalentProfiles([FromQuery] SearchProfileRequest request, int page = 1, int pageSize = 50)
	{
		try
		{
			_logger.LogInformation("SearchTalentProfiles called at {Timestamp}. Request Parameters: {Request}, Page: {Page}, PageSize: {PageSize}", DateTime.UtcNow, JsonSerializer.Serialize(request), page, pageSize);
			IEnumerable<SearchProfileModel> allResults = _profileSearchService.SearchProfiles(request);
			List<SearchProfileModel> paginatedResults = allResults.Skip((page - 1) * pageSize).Take(pageSize).ToList();
			return Ok(new PaginationResponse
			{
				Status = "Ok",
				Data = paginatedResults,
				Count = allResults.Count()
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while searching talent profiles at {Timestamp}", DateTime.UtcNow);
			return StatusCode(500, new PaginationResponse
			{
				Status = "Error",
				Message = ex.Message
			});
		}
	}

	[HttpPost("hide-unhide-talent")]
	public IActionResult HideOrUnhideTalent([FromBody] HideUnhideTalentRequest request)
	{
		try
		{
			_logger.LogInformation("Hide/Unhide request for TalentId: {TalentId}, Hide: {Hide}", request.TalentId, request.Hide);
			bool result = _profileSearchService.HideOrUnhideTalent(request);
			return Ok(new
			{
				status = "Ok",
				success = result
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error occurred in HideOrUnhideTalent");
			return StatusCode(500, new
			{
				status = "Error",
				message = "Internal server error"
			});
		}
	}

	[HttpGet("GetHidedTalnetDetails")]
	public IActionResult GetHidedTalnetDetails([FromQuery] int clientId, int page = 1, int pageSize = 50)
	{
		try
		{
			_logger.LogInformation("GetHidedTalnetDetails method called at {Timestamp}", DateTime.UtcNow);
			IEnumerable<SearchProfileModel> allResults = _profileSearchService.GetHidedTalentDetails(clientId);
			List<SearchProfileModel> paginatedResults = allResults.Skip((page - 1) * pageSize).Take(pageSize).ToList();
			return Ok(new PaginationResponse
			{
				Status = "Ok",
				Data = paginatedResults,
				Count = allResults.Count()
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while GetHidedTalnetDetails at {Timestamp}", DateTime.UtcNow);
			return StatusCode(500, new PaginationResponse
			{
				Status = "Error",
				Message = ex.Message
			});
		}
	}
}

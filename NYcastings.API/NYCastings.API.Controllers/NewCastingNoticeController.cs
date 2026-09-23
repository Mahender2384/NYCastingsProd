using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NYCastings.API.Core.Contracts.NewCastingNoticeInterface;
using NYCastings.API.Core.Models.ArchiveMessageDetailsModel;
using NYCastings.API.Core.Models.ClientDetailsModel;
using NYCastings.API.Core.Models.DeleteMessagesModel;
using NYCastings.API.Core.Models.FavoriteModel;
using NYCastings.API.Core.Models.FileUploadModel;
using NYCastings.API.Core.Models.InboxMessageDetailsModel;
using NYCastings.API.Core.Models.MessageModel;
using NYCastings.API.Core.Models.NewCastingNoticeModel;
using NYCastings.API.Core.Models.NotesModel;
using NYCastings.API.Core.Models.ProjectSubmissionModel;
using NYCastings.API.Core.Models.RoleModal;
using NYCastings.API.Core.Models.SentMessageDetailsModel;
using NYCastings.API.Core.Models.TalentDetailsfromListModel;
using NYCastings.API.Core.Models.UpdateUserinListModel;
using NYCastings.API.Core.Models.Web;

namespace NYCastings.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NewCastingNoticeController : ControllerBase
{
	private readonly ILogger<UserController> _logger;

	private readonly INewCastingNoticeService _castingNoticeService;

	public NewCastingNoticeController(ILogger<UserController> logger, INewCastingNoticeService castingNoticeService)
	{
		_logger = logger;
		_castingNoticeService = castingNoticeService;
	}

	[HttpPost("AddNoticeDetails")]
	[Consumes("multipart/form-data", new string[] { })]
	public async Task<IActionResult> AddNoticeDetails([FromForm] AddNewCastingNoticeDetails addCastingNoticeDetails)
	{
		try
		{
			_logger.LogInformation("AddNoticeDetails method called at {Timestamp} with details: {@AddCastingNoticeDetails}", DateTime.UtcNow, addCastingNoticeDetails);
			int noticeId = await _castingNoticeService.AddNoticeDetailsAsync(addCastingNoticeDetails);
			_logger.LogInformation("NoticeDetails added successfully.");
			return Ok(new PaginationResponse
			{
				Status = "Ok",
				Data = new
				{
					NoticeId = noticeId
				}
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while adding notice details at {Timestamp}", DateTime.UtcNow);
			return BadRequest(new PaginationResponse
			{
				Status = "Error",
				Message = ex.Message
			});
		}
	}

	[HttpGet("GetNewCastingNoticeDetails")]
	public IActionResult GetNewCastingNoticeDetails(int noticeId, string email = null, int page = 1, int pageSize = 50)
	{
		try
		{
			_logger.LogInformation("GetNewCastingNoticeDetails method called at {Timestamp}", DateTime.UtcNow);
			IEnumerable<GetNewCastingNoticeDetails> allResults = _castingNoticeService.GetNewCastingNoticeDetails(noticeId, email);
			_logger.LogInformation("NewCastingNoticeDetails fetched successfully.");
			List<GetNewCastingNoticeDetails> paginatedResults = allResults.Skip((page - 1) * pageSize).Take(pageSize).ToList();
			return Ok(new PaginationResponse
			{
				Status = "Ok",
				Data = paginatedResults,
				Count = allResults.Count()
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while Getting NewCastingNoticeDetails at {Timestamp}", DateTime.UtcNow);
			return StatusCode(500, new PaginationResponse
			{
				Status = "Error",
				Message = ex.Message
			});
		}
	}

	[HttpDelete("DeleteCastingNoticeDetails")]
	public PaginationResponse DeleteCastingNoticeDetails([FromQuery] int noticeId)
	{
		try
		{
			_logger.LogInformation("DeleteCastingNoticeDetails method called at {Timestamp}", DateTime.UtcNow);
			bool results = _castingNoticeService.DeleteCastingNoticeData(noticeId);
			_logger.LogInformation("CastingNoticeDetails deleted successfully.");
			return new PaginationResponse
			{
				Data = results,
				Status = "OK"
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while deleting CastingNoticeDetails at {Timestamp}", DateTime.UtcNow);
			return new PaginationResponse
			{
				Message = ex.Message
			};
		}
	}

	[HttpPost("UploadFiles")]
	[Consumes("multipart/form-data")]
	public async Task<IActionResult> UploadFiles(List<IFormFile> files)
	{
		try
		{
			_logger.LogInformation("UploadFiles method called at {Timestamp}", DateTime.UtcNow);
			List<UploadedFileResult> result = await _castingNoticeService.UploadFilesAsync(files);
			return Ok(new PaginationResponse
			{
				Status = "Ok",
				Data = result
			});
		}
		catch (ArgumentException ex)
		{
			return BadRequest(new PaginationResponse
			{
				Status = "Error",
				Message = ex.Message
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while uploading files at {Timestamp}", DateTime.UtcNow);
			return StatusCode(500, new PaginationResponse
			{
				Status = "Error",
				Message = ex.Message
			});
		}
	}

	[HttpPost("AddRoleDetails")]
	[Consumes("multipart/form-data", new string[] { })]
	public async Task<IActionResult> AddorUpdateDetails([FromForm] AddRoleDetails roleDetails)
	{
		try
		{
			_logger.LogInformation("AddRoleDetails method called at {Timestamp}", DateTime.UtcNow);
			bool result = await _castingNoticeService.AddOrUpdateRoleWithFile(roleDetails);
			_logger.LogInformation("Role deatils added successfully.");
			return Ok(new PaginationResponse
			{
				Status = "Ok",
				Data = result
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while adding Role details at {Timestamp}", DateTime.UtcNow);
			return BadRequest(new PaginationResponse
			{
				Status = "Error",
				Message = ex.Message
			});
		}
	}

	[HttpGet("GetRoleDetails")]
	public PaginationResponse GetRoleDetails(int? roleId, int? noticeId, string? roleName, string? sex, int page = 1, int pageSize = 10)
	{
		try
		{
			_logger.LogInformation("GetRoleDetails method called at {Timestamp}", DateTime.UtcNow);
			IEnumerable<GetRoleDetails> results = _castingNoticeService.GetRoleDetails(roleId, noticeId, roleName, sex);
			_logger.LogInformation("Role deatils fectched successfully.");
			List<GetRoleDetails> paginatedData = results.Skip((page - 1) * pageSize).Take(pageSize).ToList();
			return new PaginationResponse
			{
				Status = "Ok",
				Data = new
				{
					TotalItems = results.Count(),
					TotalPages = (int)Math.Ceiling((double)results.Count() / (double)pageSize),
					CurrentPage = page,
					PageSize = pageSize,
					Items = paginatedData
				}
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while fectching Role details at {Timestamp}", DateTime.UtcNow);
			return new PaginationResponse
			{
				Status = "Error",
				Message = ex.Message
			};
		}
	}

	[HttpDelete("DeleteRoleDetails")]
	public PaginationResponse DeleteRoleDetails([FromQuery] int roleId)
	{
		try
		{
			_logger.LogInformation("DeleteRoleDetails method called at {Timestamp} for RoleId: {RoleId}", DateTime.UtcNow, roleId);
			bool results = _castingNoticeService.DeleteRoleData(roleId);
			_logger.LogInformation("Role Details deleted successfully.");
			return new PaginationResponse
			{
				Data = results,
				Status = "OK"
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while Deleting Role details at {Timestamp}", DateTime.UtcNow);
			return new PaginationResponse
			{
				Message = ex.Message
			};
		}
	}

	[HttpPost("ApproveNotice")]
	public PaginationResponse ApproveNotice(int noticeId, bool approve, int userId)
	{
		try
		{
			_logger.LogInformation("ApproveNotice method called at {Timestamp}", DateTime.UtcNow);
			bool results = _castingNoticeService.ApproveNotice(noticeId, approve, userId);
			_logger.LogInformation("Notice Details approved successfully.");
			return new PaginationResponse
			{
				Data = results,
				Status = "OK"
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while Approving Notice  details at {Timestamp}", DateTime.UtcNow);
			return new PaginationResponse
			{
				Message = ex.Message
			};
		}
	}

	[HttpPost("NotifyUserOnNoticeSubmission")]
	public PaginationResponse NotifyUserOnNoticeSubmission(string userName, string email)
	{
		try
		{
			_logger.LogInformation("NotifyUserOnNoticeSubmission method called at {Timestamp}", DateTime.UtcNow);
			bool results = _castingNoticeService.NotifyUserOnNoticeSubmission(userName, email);
			_logger.LogInformation("NotifyUserOnNoticeSubmission is successfull.");
			return new PaginationResponse
			{
				Data = results,
				Status = "OK"
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while NotifyUserOnNoticeSubmission at {Timestamp}", DateTime.UtcNow);
			return new PaginationResponse
			{
				Message = ex.Message
			};
		}
	}

	[HttpGet("GetProjectSubmissions")]
	public PaginationResponse GetProjectSubmissions(int noticeId)
	{
		try
		{
			_logger.LogInformation("GetProjectSubmissions method called at {Timestamp}", DateTime.UtcNow);
			IEnumerable<ProjectSubmissionModel> results = _castingNoticeService.GetProjectSubmissions(noticeId);
			_logger.LogInformation("Project submissions retrieved successfully.");
			return new PaginationResponse
			{
				Data = results,
				Status = "OK"
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while fetching project submissions at {Timestamp}", DateTime.UtcNow);
			return new PaginationResponse
			{
				Message = ex.Message
			};
		}
	}

	[HttpGet("GetTalentDetails")]
	public PaginationResponse GetTalentDetails(string userIds, int roleId, int clientId, string sortBy)
	{
		try
		{
			_logger.LogInformation("GetTalentDetails method called at {Timestamp}", DateTime.UtcNow);
			IEnumerable<TalentDetailsModel> results = _castingNoticeService.GetTalentDetails(userIds, roleId, clientId, sortBy);
			_logger.LogInformation("Talent details retrieved successfully.");
			return new PaginationResponse
			{
				Data = results,
				Status = "OK"
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while fetching talent details at {Timestamp}", DateTime.UtcNow);
			return new PaginationResponse
			{
				Message = ex.Message
			};
		}
	}

	[HttpPost("UpdateUserList")]
	public PaginationResponse UpdateUserList([FromBody] UpdateUserListModel updateModel)
	{
		try
		{
			_logger.LogInformation("UpdateUserList method called at {Timestamp}", DateTime.UtcNow);
			if (_castingNoticeService.UpdateUserList(updateModel))
			{
				_logger.LogInformation("User list updated successfully.");
				return new PaginationResponse
				{
					Status = "OK",
					Message = "User moved successfully."
				};
			}
			_logger.LogWarning("Failed to update user list.");
			return new PaginationResponse
			{
				Status = "Error",
				Message = "Failed to update user list."
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while updating user list at {Timestamp}", DateTime.UtcNow);
			return new PaginationResponse
			{
				Status = "Error",
				Message = ex.Message
			};
		}
	}

	[HttpPost("AddOrUpdateTalentNote")]
	public PaginationResponse AddOrUpdateTalentNote([FromBody] TalentNotes note)
	{
		try
		{
			_logger.LogInformation("AddOrUpdateTalentNote method called");
			bool noteId = _castingNoticeService.AddOrUpdateTalentNote(note);
			_logger.LogInformation("Talent note saved successfully");
			return new PaginationResponse
			{
				Status = "Ok",
				Data = noteId
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while saving the talent note");
			return new PaginationResponse
			{
				Status = "Error",
				Message = ex.Message
			};
		}
	}

	[HttpGet("GetTalentNotes")]
	public PaginationResponse GetTalentNotes([FromQuery] int? talentId = null, [FromQuery] int? directorId = null)
	{
		try
		{
			_logger.LogInformation("GetTalentNotes method called");
			List<TalentNotes> notes = _castingNoticeService.GetTalentNotes(talentId, directorId);
			if (!notes.Any())
			{
				return new PaginationResponse
				{
					Status = "No Data",
					Message = "No talent notes found."
				};
			}
			_logger.LogInformation("Talent notes retrieved successfully");
			return new PaginationResponse
			{
				Status = "Ok",
				Data = notes
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while retrieving talent notes");
			return new PaginationResponse
			{
				Status = "Error",
				Message = ex.Message
			};
		}
	}

	[HttpPost("AddOrUpdateFave")]
	public IActionResult AddOrUpdateClientFave([FromForm] ClientFaveModel clientFaveModel)
	{
		try
		{
			_logger.LogInformation("AddOrUpdateClientFave method called at {Timestamp} with Data: ClientId={ClientId}, TalentId={TalentId}, Fave={Fave}", DateTime.UtcNow, clientFaveModel.ClientId, clientFaveModel.TalentId, clientFaveModel.Fave);
			bool result = _castingNoticeService.AddOrUpdateClientFave(clientFaveModel);
			_logger.LogInformation("Client favorite updated successfully for ClientId={ClientId}, TalentId={TalentId}, Fave={Fave} at {Timestamp}", clientFaveModel.ClientId, clientFaveModel.TalentId, clientFaveModel.Fave, DateTime.UtcNow);
			return Ok(new PaginationResponse
			{
				Status = "OK",
				Data = result
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while updating client favorite at {Timestamp}", DateTime.UtcNow);
			return BadRequest(new PaginationResponse
			{
				Status = "Error",
				Message = ex.Message
			});
		}
	}

	[HttpGet("GetClientFavoriteTalentDetails")]
	public PaginationResponse GetClientFavTalentDetails(int clientId, int page = 1, int pageSize = 10)
	{
		try
		{
			_logger.LogInformation("GetClientFavTalentDetails method called at {Timestamp}", DateTime.UtcNow);
			IEnumerable<ClientFaveTalentModel> results = _castingNoticeService.GetClientFavTalentDetails(clientId);
			_logger.LogInformation("GetClientFavTalentDetails details retrieved successfully.");
			List<ClientFaveTalentModel> paginatedData = results.Skip((page - 1) * pageSize).Take(pageSize).ToList();
			return new PaginationResponse
			{
				Status = "OK",
				Data = new
				{
					TotalItems = results.Count(),
					TotalPages = (int)Math.Ceiling((double)results.Count() / (double)pageSize),
					CurrentPage = page,
					PageSize = pageSize,
					Items = paginatedData
				}
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while fetching GetClientFavTalentDetails details at {Timestamp}", DateTime.UtcNow);
			return new PaginationResponse
			{
				Status = "Error",
				Message = ex.Message
			};
		}
	}

	[HttpGet("GetInboxMessages")]
	public PaginationResponse GetInboxMessages(string email, int page = 1, int pageSize = 10, string sortOrder = "Latest")
	{
		try
		{
			_logger.LogInformation("GetInboxMessages method called at {Timestamp}", DateTime.UtcNow);
			IEnumerable<InboxMessageDetails> results = _castingNoticeService.GetInboxMessagesDetails(email, sortOrder);
			_logger.LogInformation("Inbox messages retrieved successfully.");
			List<InboxMessageDetails> paginatedData = results.Skip((page - 1) * pageSize).Take(pageSize).ToList();
			return new PaginationResponse
			{
				Status = "Ok",
				Data = new
				{
					TotalItems = results.Count(),
					TotalPages = (int)Math.Ceiling((double)results.Count() / (double)pageSize),
					CurrentPage = page,
					PageSize = pageSize,
					Items = paginatedData
				}
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while fetching inbox messages at {Timestamp}", DateTime.UtcNow);
			return new PaginationResponse
			{
				Message = ex.Message
			};
		}
	}

	[HttpGet("GetSentMessages")]
	public PaginationResponse GetSentMessages(string email, int page = 1, int pageSize = 10)
	{
		try
		{
			_logger.LogInformation("GetSentMessages method called at {Timestamp}", DateTime.UtcNow);
			IEnumerable<SentMessageDetails> results = _castingNoticeService.GetSentMessagesDetails(email);
			_logger.LogInformation("Sent messages retrieved successfully.");
			List<SentMessageDetails> paginatedData = results.Skip((page - 1) * pageSize).Take(pageSize).ToList();
			return new PaginationResponse
			{
				Status = "Ok",
				Data = new
				{
					TotalItems = results.Count(),
					TotalPages = (int)Math.Ceiling((double)results.Count() / (double)pageSize),
					CurrentPage = page,
					PageSize = pageSize,
					Items = paginatedData
				}
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while fetching Sent messages at {Timestamp}", DateTime.UtcNow);
			return new PaginationResponse
			{
				Message = ex.Message
			};
		}
	}

	[HttpGet("GetArchiveMessages")]
	public PaginationResponse GetArchiveMessages(string email, string user, int page = 1, int pageSize = 10, string sortOrder = "Latest")
	{
		try
		{
			_logger.LogInformation("GetArchiveMessages method called at {Timestamp}", DateTime.UtcNow);
			IEnumerable<ArchiveMessageDetails> results = _castingNoticeService.GetArchiveMessagesDetails(email, user, sortOrder);
			_logger.LogInformation("Archive messages retrieved successfully.");
			List<ArchiveMessageDetails> paginatedData = results.Skip((page - 1) * pageSize).Take(pageSize).ToList();
			return new PaginationResponse
			{
				Status = "Ok",
				Data = new
				{
					TotalItems = results.Count(),
					TotalPages = (int)Math.Ceiling((double)results.Count() / (double)pageSize),
					CurrentPage = page,
					PageSize = pageSize,
					Items = paginatedData
				}
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while fetching Archive messages at {Timestamp}", DateTime.UtcNow);
			return new PaginationResponse
			{
				Message = ex.Message
			};
		}
	}

	[HttpGet("GetArchiveTalentMessages")]
	public PaginationResponse GetArchiveTalentMessages(string email, string user, int page = 1, int pageSize = 10, string sortOrder = "Latest")
	{
		try
		{
			_logger.LogInformation("GetArchiveTalentMessages method called at {Timestamp}", DateTime.UtcNow);
			IEnumerable<ArchiveMessageDetails> results = _castingNoticeService.GetArchiveMessagesDetails(email, user, sortOrder);
			_logger.LogInformation("TalentArchive messages retrieved successfully.");
			List<ArchiveMessageDetails> paginatedData = results.Skip((page - 1) * pageSize).Take(pageSize).ToList();
			return new PaginationResponse
			{
				Status = "Ok",
				Data = new
				{
					TotalItems = results.Count(),
					TotalPages = (int)Math.Ceiling((double)results.Count() / (double)pageSize),
					CurrentPage = page,
					PageSize = pageSize,
					Items = paginatedData
				}
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while fetching Archive messages at {Timestamp}", DateTime.UtcNow);
			return new PaginationResponse
			{
				Message = ex.Message
			};
		}
	}

	[HttpGet("ShowInboxMessages")]
	public IActionResult ShowInboxMessages(int originalMessageId, string status, string user)
	{
		try
		{
			_logger.LogInformation("ShowMessages method called at {Timestamp}", DateTime.UtcNow);
			IEnumerable<MessageModel> results = _castingNoticeService.ShowMessages(originalMessageId, status, user);
			_logger.LogInformation("Messages retrieved successfully.");
			return Ok(new
			{
				Status = "OK",
				Data = results
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while fetching messages at {Timestamp}", DateTime.UtcNow);
			return BadRequest(new { ex.Message });
		}
	}

	[HttpPost("SendMessage")]
	public async Task<IActionResult> SendMessage([FromForm] SendMessageModel message)
	{
		try
		{
			string messageData = JsonConvert.SerializeObject(message);
			_logger.LogInformation("SendMessage method called at {Timestamp}. Input Data: {MessageData}", DateTime.UtcNow, messageData);
			int originalMessageId = await _castingNoticeService.SendMessage(message);
			if (originalMessageId > 0)
			{
				_logger.LogInformation("Message inserted successfully at {Timestamp}. OriginalMessageId: {OriginalMessageId}", DateTime.UtcNow, originalMessageId);
				return Ok(new PaginationResponse
				{
					Status = "Ok",
					Message = "Message inserted successfully",
					Data = new
					{
						OriginalMessageId = originalMessageId
					}
				});
			}
			_logger.LogWarning("Message insertion failed at {Timestamp}", DateTime.UtcNow);
			return BadRequest(new PaginationResponse
			{
				Status = "Error",
				Message = "Failed to insert message"
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while inserting the message at {Timestamp}", DateTime.UtcNow);
			return BadRequest(new PaginationResponse
			{
				Status = "Error",
				Message = ex.Message
			});
		}
	}

	[HttpPost("archive")]
	public IActionResult ToggleArchiveMessages([FromBody] ArchiveMessageRequest request)
	{
		try
		{
			_logger.LogInformation("ToggleArchiveMessages API called with MessageIds: {MessageIds}, ActiveFlag: {ActiveFlag}", string.Join(",", request.OriginalMsgIds), request.ActiveFlag);
			string messageIdsCsv = string.Join(",", request.OriginalMsgIds);
			if (_castingNoticeService.ToggleArchiveMessages(messageIdsCsv, request.ActiveFlag, request.ArchiveFrom))
			{
				string action = (request.ActiveFlag ? "archived" : "unarchived");
				return Ok(new
				{
					Status = "Ok",
					Message = "Messages " + action + " successfully."
				});
			}
			return BadRequest(new
			{
				Status = "Error",
				Message = "Failed to change archive status of messages."
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error while toggling archive status of messages.");
			return StatusCode(500, new
			{
				Status = "Error",
				Message = ex.Message
			});
		}
	}

	[HttpPost("delete")]
	public IActionResult DeleteMessages([FromBody] DeleteMessages request)
	{
		try
		{
			_logger.LogInformation("DeleteMessages API called with MessageIds: {MessageIds}, ActiveFlag: {ActiveFlag},PermentArchive: {PermentArchive}", string.Join(",", request.OriginalMsgIds), request.ActiveFlag, request.PermanentArchive);
			string messageIdsCsv = string.Join(",", request.OriginalMsgIds);
			if (_castingNoticeService.DeleteMessages(messageIdsCsv, request.ActiveFlag, request.PermanentArchive, request.UserId))
			{
				string action = (request.PermanentArchive ? "is Deleted" : " is not Deleted");
				return Ok(new
				{
					Status = "Ok",
					Message = "Messages " + action + " successfully."
				});
			}
			return BadRequest(new
			{
				Status = "Error",
				Message = "Failed to change delete status of messages."
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error while delete status of messages.");
			return StatusCode(500, new
			{
				Status = "Error",
				Message = ex.Message
			});
		}
	}

	[HttpGet("GetClientDetails")]
	public IActionResult GetClientDetails([FromQuery] string email)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(email))
			{
				return BadRequest(new
				{
					Status = "Error",
					Message = "Client email cannot be empty."
				});
			}
			_logger.LogInformation("Fetching client details for: {Email}", email);
			List<ClientDetailsModel> clientDetails = _castingNoticeService.GetClientDetails(email).ToList();
			if (clientDetails.Count == 0)
			{
				return NotFound(new
				{
					Status = "Error",
					Message = "Client not found"
				});
			}
			return Ok(new
			{
				Status = "Ok",
				Data = clientDetails
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error fetching client details for {Email}", email);
			return StatusCode(500, new
			{
				Status = "Error",
				Message = "Internal Server Error"
			});
		}
	}

	[HttpPost("updateClientDetails")]
	public async Task<IActionResult> UpdateClient([FromForm] UpdateClientDetailsModel client)
	{
		try
		{
			_logger.LogInformation("Received UpdateClientDetails payload: UserId: {UserId}, Password: {Password}, ContactEmail: {ContactEmail}, YourName: {YourName}, Name2: {Name2}, Name3: {Name3}, PhonNumber: {PhonNumber}, CompanyName: {CompanyName}, Address1: {Address1}, Address2: {Address2}, City: {City}, State1: {State1}, Zip: {Zip}, FileURL: {FileURL}, Logo FileName: {LogoFileName}", client.UserId, client.Password, client.ContactEmail, client.FirstName, client.LastName, client.PhonNumber, client.CompanyName, client.Address1, client.Address2, client.City, client.State1, client.Zip, client.FileURL, (client.Logo != null) ? client.Logo.FileName : "No File Uploaded");
			_logger.LogInformation("Attempting to update client details for UserId: {UserId}", client.UserId);
			if (await _castingNoticeService.UpdateClientDetails(client))
			{
				_logger.LogInformation("Successfully updated client details for UserId: {UserId}", client.UserId);
				return Ok(new
				{
					Status = "Ok",
					Message = "Client details updated successfully."
				});
			}
			_logger.LogWarning("Failed to update client details for UserId: {UserId}", client.UserId);
			return BadRequest(new
			{
				Status = "Error",
				Message = "Failed to update client details."
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "An error occurred while updating client details for UserId: {UserId}", client?.UserId);
			return StatusCode(500, new
			{
				Status = "Error",
				Message = "Internal Server Error"
			});
		}
	}

	[HttpGet("check-email")]
	public IActionResult CheckEmailExists([FromQuery] string email)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(email))
			{
				return BadRequest(new
				{
					Status = "Error",
					Message = "Email cannot be empty."
				});
			}
			_logger.LogInformation("Checking if email exists: {Email}", email);
			bool exists = _castingNoticeService.CheckEmailExists(email);
			return Ok(new
			{
				Status = "Ok",
				EmailExists = exists
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error checking email existence: {Email}", email);
			return StatusCode(500, new
			{
				Status = "Error",
				Message = "Internal Server Error"
			});
		}
	}

	[HttpGet("GetInboxTalentMessages")]
	public PaginationResponse GetInboxTalentMessages([FromQuery] string email, int page = 1, int pageSize = 10, string sortOrder = "Latest")
	{
		try
		{
			_logger.LogInformation("Getting inbox talent messages for email: {Email}", email);
			IEnumerable<InboxTalentMessageModel> results = _castingNoticeService.GetInboxTalentMessages(email, sortOrder);
			List<InboxTalentMessageModel> paginatedData = results.Skip((page - 1) * pageSize).Take(pageSize).ToList();
			return new PaginationResponse
			{
				Status = "Ok",
				Data = new
				{
					TotalItems = results.Count(),
					TotalPages = (int)Math.Ceiling((double)results.Count() / (double)pageSize),
					CurrentPage = page,
					PageSize = pageSize,
					Items = paginatedData
				}
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error retrieving inbox talent messages for email: {Email}", email);
			return new PaginationResponse
			{
				Message = ex.Message
			};
		}
	}

	[HttpGet("GetUnreadMessageCount")]
	public PaginationResponse GetUnreadMessageCount(string email)
	{
		try
		{
			_logger.LogInformation("GetUnreadMessageCount method called at {Timestamp}", DateTime.UtcNow);
			int count = _castingNoticeService.GetUnreadMessageCount(email);
			_logger.LogInformation("Unread message count retrieved successfully.");
			return new PaginationResponse
			{
				Status = "Ok",
				Data = new
				{
					UnreadMessageCount = count
				}
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while fetching unread message count at {Timestamp}", DateTime.UtcNow);
			return new PaginationResponse
			{
				Message = ex.Message
			};
		}
	}

	[HttpGet("check-userName")]
	public IActionResult CheckUserNameExists([FromQuery] string userName)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(userName))
			{
				return BadRequest(new
				{
					Status = "Error",
					Message = "Username cannot be empty."
				});
			}
			_logger.LogInformation("Checking if username exists: {UserName}", userName);
			bool exists = _castingNoticeService.CheckUserNameExists(userName);
			return Ok(new
			{
				Status = "Ok",
				UserNameExists = exists
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error checking username existence: {UserName}", userName);
			return StatusCode(500, new
			{
				Status = "Error",
				Message = "Internal Server Error"
			});
		}
	}

	[HttpGet("user-active-status")]
	public IActionResult GetUserActiveStatus([FromQuery] int userId)
	{
		try
		{
			if (userId <= 0)
			{
				return BadRequest(new
				{
					Status = "Error",
					Message = "Invalid UserId."
				});
			}
			_logger.LogInformation("Checking active status for UserId: {UserId}", userId);
			int isActive = _castingNoticeService.GetUserActiveStatus(userId);
			return Ok(new
			{
				Status = "Ok",
				IsActive = isActive
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error checking active status for UserId: {UserId}", userId);
			return StatusCode(500, new
			{
				Status = "Error",
				Message = "Internal Server Error"
			});
		}
	}
}

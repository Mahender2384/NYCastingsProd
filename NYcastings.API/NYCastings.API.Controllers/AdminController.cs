using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NYCastings.API.Core.Contracts.AdminInterface;
using NYCastings.API.Core.Models.Alleventmodels;
using NYCastings.API.Core.Models.BulkEmailModels;
using NYCastings.API.Core.Models.ChargebeeModel;
using NYCastings.API.Core.Models.CreditMaster;
using NYCastings.API.Core.Models.Credits;
using NYCastings.API.Core.Models.DeleteUserRequestModel;
using NYCastings.API.Core.Models.NoticeApprovalRequestModel;
using NYCastings.API.Core.Models.NotificationMessageModel;
using NYCastings.API.Core.Models.PaginatedResponse;
using NYCastings.API.Core.Models.PlanDetailModel;
using NYCastings.API.Core.Models.Question_Answers;
using NYCastings.API.Core.Models.RoleModel;
using NYCastings.API.Core.Models.UserBillingModel;
using NYCastings.API.Core.Models.UserDetailsByRoleModel;
using NYCastings.API.Core.Models.UserResume;
using NYCastings.API.Core.Models.UserVisibility;
using NYCastings.API.Core.Models.Web;

namespace NYCastings.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AdminController : ControllerBase
{
	private readonly ILogger<UserController> _logger;

	private readonly IAdminService _adminService;

	public AdminController(ILogger<UserController> logger, IAdminService adminService)
	{
		_logger = logger;
		_adminService = adminService;
	}

	[HttpPost("DismissNotification")]
	public PaginationResponse DismissNotification([FromBody] DismissNotificationRequestModel request)
	{
		try
		{
			_logger.LogInformation("DismissNotification method called");
			bool result = _adminService.DismissNotification(request);
			_logger.LogInformation("Notification dismissed successfully");
			return new PaginationResponse
			{
				Status = "Ok",
				Data = result
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while dismissing the notification");
			return new PaginationResponse
			{
				Status = "Error",
				Message = ex.Message
			};
		}
	}

	[HttpPost("AddOrUpdateNotificationMessage")]
	public PaginationResponse AddOrUpdateNotificationMessage([FromBody] NotificationMessageRequest request)
	{
		try
		{
			_logger.LogInformation("AddOrUpdateNotificationMessage called");
			int notificationId = _adminService.AddOrUpdateNotificationMessage(request);
			if (notificationId > 0)
			{
				return new PaginationResponse
				{
					Status = "Ok",
					Data = new
					{
						NotificationId = notificationId
					}
				};
			}
			return new PaginationResponse
			{
				Status = "Error",
				Message = "Failed to add or update notification."
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error in AddOrUpdateNotificationMessage");
			return new PaginationResponse
			{
				Status = "Error",
				Message = ex.Message
			};
		}
	}

	[HttpGet("GetUserDetailsBasedOnRole")]
	public PaginationResponse GetUserDetailsBasedOnRole(int roleId, string searchText = null, int page = 1, int pageSize = 10)
	{
		try
		{
			_logger.LogInformation("Fetching user details by role ID: {RoleId}", roleId);
			(IEnumerable<UserDetailsByRoleModel> Users, int Count) userDetailsBasedOnRole = _adminService.GetUserDetailsBasedOnRole(roleId, searchText);
			IEnumerable<UserDetailsByRoleModel> users = userDetailsBasedOnRole.Users;
			int count = userDetailsBasedOnRole.Count;
			List<UserDetailsByRoleModel> paginatedData = users.Skip((page - 1) * pageSize).Take(pageSize).ToList();
			return new PaginationResponse
			{
				Status = "Ok",
				Data = new
				{
					TotalItems = count,
					TotalPages = (int)Math.Ceiling((double)count / (double)pageSize),
					CurrentPage = page,
					PageSize = pageSize,
					Items = paginatedData
				}
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error retrieving user details by role ID");
			return new PaginationResponse
			{
				Status = "Error",
				Message = ex.Message
			};
		}
	}

	[HttpPost("DeleteUser")]
	public PaginationResponse DeleteUser([FromBody] DeleteUserRequestModel request)
	{
		try
		{
			_logger.LogInformation("DeleteUser method called");
			bool result = _adminService.DeleteUser(request);
			_logger.LogInformation("User deleted successfully");
			return new PaginationResponse
			{
				Status = "Ok",
				Data = result
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while deleting the user");
			return new PaginationResponse
			{
				Status = "Error",
				Message = ex.Message
			};
		}
	}

	[HttpGet("GetNotificationMessages")]
	public PaginationResponse GetNotificationMessages([FromQuery] int notificationId = 0)
	{
		try
		{
			_logger.LogInformation("GetNotificationMessages called");
			IEnumerable<NotificationMessageResponse> result = _adminService.GetNotificationMessages(notificationId);
			return new PaginationResponse
			{
				Status = "Ok",
				Data = result
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error in GetNotificationMessages");
			return new PaginationResponse
			{
				Status = "Error",
				Message = ex.Message
			};
		}
	}

	[HttpGet("GetAllRoles")]
	public PaginationResponse GetAllRoles()
	{
		try
		{
			_logger.LogInformation("GetAllRoles called");
			IEnumerable<ActiveRolesModel> result = _adminService.GetAllRoles();
			return new PaginationResponse
			{
				Status = "Ok",
				Data = result
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error in GetAllRoles");
			return new PaginationResponse
			{
				Status = "Error",
				Message = ex.Message
			};
		}
	}

	[HttpGet("GetApprovedorunApprovedNotices")]
	public IActionResult GetUnapprovedNotices(string status = null, string email = null, int Page = 1, int PageSize = 10, string sortOrder = "Desc")
	{
		try
		{
			PaginatedUnapprovedNoticeResponse result = _adminService.GetUnapprovedNotices(status, email, Page, PageSize, sortOrder);
			return Ok(new PaginationResponse
			{
				Status = "Ok",
				Data = result.Notices,
				Count = result.TotalCount
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error retrieving notices");
			return StatusCode(500, "Internal server error.");
		}
	}

	[HttpPost("ApproveOrRejectNotice")]
	public IActionResult ApproveOrRejectNotice([FromBody] NoticeApprovalRequest request)
	{
		try
		{
			_logger.LogInformation("ApproveOrRejectNotice called with: {@request}", request);
			bool result = _adminService.ApproveOrRejectNotice(request);
			return Ok(new
			{
				Status = (result ? "Success" : "Failed"),
				Message = (result ? "Notice updated and user notified." : "Failed to update notice or notify user.")
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error in ApproveOrRejectNotice");
			return StatusCode(500, "Internal server error");
		}
	}

	[HttpGet("GetUserBillingDetails")]
	public IActionResult GetUserBillingDetails([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
	{
		try
		{
			_logger.LogInformation("Fetching paginated user billing details: Page {Page}, PageSize {PageSize}", page, pageSize);
			IEnumerable<UserBillingModel> allRecords = _adminService.GetUserBillingDetails();
			int totalCount = allRecords.Count();
			List<UserBillingModel> paginated = allRecords.Skip((page - 1) * pageSize).Take(pageSize).ToList();
			return Ok(new PaginationResponse<UserBillingModel>
			{
				Status = "Ok",
				Data = paginated,
				Count = totalCount
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error retrieving user billing details");
			return StatusCode(500, "Internal server error.");
		}
	}

	[HttpPost("AddOrUpdateUserBilling")]
	public IActionResult AddOrUpdateUserBilling([FromBody] UserBillingRequestModel model)
	{
		try
		{
			_logger.LogInformation("Adding or updating user billing: {@Model}", model);
			bool result = _adminService.AddOrUpdateUserBilling(model);
			return Ok(new
			{
				Status = "Success",
				Message = "User billing updated successfully",
				Result = result
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error updating user billing");
			return StatusCode(500, "Internal server error");
		}
	}

	[HttpGet("GetPlanDetails")]
	public IActionResult GetPlanDetails()
	{
		try
		{
			_logger.LogInformation("Retrieving plan details...");
			IEnumerable<PlanDetail> result = _adminService.GetPlanDetails();
			return Ok(new
			{
				Status = "Ok",
				Data = result
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error while retrieving plan details");
			return StatusCode(500, "Internal server error.");
		}
	}

	[HttpPost("AddOrUpdatePlanDetails")]
	public IActionResult AddOrUpdatePlan([FromBody] PlanDetailsRequestModel model)
	{
		try
		{
			bool result = _adminService.AddOrUpdatePlanDetails(model);
			return Ok(new
			{
				Status = (result ? "Plan Details Saved Successfully" : "Failed")
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error while adding/updating plan details");
			return StatusCode(500, "Internal server error");
		}
	}

	[HttpPost("AddOrUpdateUserVisibility")]
	public IActionResult AddOrUpdateUserVisibility([FromBody] UserVisibilityRequestModel model)
	{
		try
		{
			_logger.LogInformation("Adding or updating user visibility: {@Model}", model);
			bool result = _adminService.AddOrUpdateUserVisibility(model);
			return Ok(new
			{
				Status = "Success",
				Message = "User visibility updated successfully",
				Result = result
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error updating user visibility");
			return StatusCode(500, "Internal server error");
		}
	}

	[HttpPost("UpdateUserDetailsById")]
	public IActionResult UpdateUserDetailsById([FromBody] UpdateUserDetailsRequestModel model)
	{
		try
		{
			_logger.LogInformation("Updating user details: {@Model}", model);
			bool result = _adminService.UpdateUserDetailsById(model);
			return Ok(new
			{
				Status = "Success",
				Message = "User details updated successfully",
				Result = result
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error updating user details");
			return StatusCode(500, "Internal server error");
		}
	}

	[HttpGet("GetNoticeStatusCounts")]
	public IActionResult GetNoticeStatusCounts()
	{
		try
		{
			_logger.LogInformation("Fetching notice status counts...");
			NoticeStatusCountResponseModel result = _adminService.GetNoticeStatusCounts();
			return Ok(new
			{
				Status = "Success",
				Message = "Notice status counts retrieved successfully",
				Data = result
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error retrieving notice status counts");
			return StatusCode(500, "Internal server error.");
		}
	}

	[HttpPost("SaveOrUpdateCreditMaster")]
	public IActionResult SaveOrUpdateCreditMaster([FromBody] CreditMasterRequestModel model)
	{
		try
		{
			_logger.LogInformation("Saving credit master: {@Model}", model);
			bool result = _adminService.SaveOrUpdateCreditMaster(model);
			return Ok(new
			{
				Status = "Success",
				Message = "Credit rules saved successfully",
				Result = result
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error saving credit master");
			return StatusCode(500, "Internal server error");
		}
	}

	[HttpPost("GrantCreditsToInactiveTalents")]
	public IActionResult GrantCreditsToInactiveTalents([FromBody] AdminGrantCreditsRequestModel model)
	{
		try
		{
			_logger.LogInformation("Granting credits to inactive talents: {@Model}", model);
			if (model.CreditPoints <= 0)
			{
				return BadRequest("CreditPoints must be greater than zero.");
			}
			bool result = _adminService.GrantCreditsToInactiveTalents(model);
			return Ok(new
			{
				Status = "Success",
				Message = "Credits successfully granted to inactive talents.",
				Result = result
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error granting credits to inactive talents");
			return StatusCode(500, "Internal server error");
		}
	}

	[HttpGet("GetPlans")]
	public PaginationResponse GetPlans()
	{
		try
		{
			_logger.LogInformation("Fetching Chargebee plans...");
			List<ChargebeePlanModel> result = _adminService.GetPlans();
			return new PaginationResponse
			{
				Status = "Ok",
				Data = result
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error fetching plans");
			return new PaginationResponse
			{
				Status = "Error",
				Message = ex.Message
			};
		}
	}

	[HttpPost("create-checkout")]
	public async Task<IActionResult> CreateCheckout([FromBody] CheckoutRequest req)
	{
		if (req == null || string.IsNullOrEmpty(req.Email) || string.IsNullOrEmpty(req.ItemPriceId))
		{
			return BadRequest("Email and ItemPriceId are required");
		}
		return Ok(new
		{
			message = "Checkout URL generated",
			url = await _adminService.CreateCheckoutAsync(req.Email, req.ItemPriceId)
		});
	}

	[HttpPost("webhook")]
	public async Task<IActionResult> Webhook()
	{
		using StreamReader reader = new StreamReader(base.Request.Body);
		dynamic data = JsonConvert.DeserializeObject(await reader.ReadToEndAsync());
		string eventType = data.event_type;
		if (eventType == "invoice_paid")
		{
			SaveInvoicePaid(data);
		}
		return Ok();
	}

	private void SaveInvoicePaid(dynamic data)
	{
		dynamic invoice = data.content.invoice;
		dynamic subscription = data.content.subscription;
		dynamic customer = data.content.customer;
		PaymentSaveRequest paymentSaveRequest = new PaymentSaveRequest();
		paymentSaveRequest.InvoiceId = invoice.id;
		paymentSaveRequest.SubscriptionId = subscription.id;
		paymentSaveRequest.CustomerId = customer.id;
		paymentSaveRequest.Email = customer.email;
		paymentSaveRequest.PlanId = subscription.plan_id;
		paymentSaveRequest.ItemPriceId = invoice.line_items[0].entity_id;
		paymentSaveRequest.Amount = (double)invoice.total / 100.0;
		paymentSaveRequest.TermStart = DateTimeOffset.FromUnixTimeSeconds((long)subscription.current_term_start).DateTime;
		paymentSaveRequest.TermEnd = DateTimeOffset.FromUnixTimeSeconds((long)subscription.current_term_end).DateTime;
		PaymentSaveRequest model = paymentSaveRequest;
		_adminService.SaveChargebeePayment(model);
	}

	[HttpPost("edit-billing")]
	public async Task<IActionResult> EditBilling(string email)
	{
		string portalUrl = await _adminService.RedirectToBillingPortalIfActiveAsync(email);
		if (portalUrl == null)
		{
			return Ok(new
			{
				message = "No active subscription found"
			});
		}
		return Ok(new
		{
			redirectUrl = portalUrl
		});
	}

	[HttpPost("subscription-status")]
	[AllowAnonymous]
	public async Task<IActionResult> SubscriptionStatus()
	{
		try
		{
			string authHeader = base.Request.Headers["Authorization"].ToString();
			(bool, bool, string, string, string, string) result = await _adminService.ProcessWebhookAsync(base.Request.Body, authHeader);
			if (!result.Item1)
			{
				_logger.LogWarning("Invalid Chargebee webhook credentials.");
				return Unauthorized();
			}
			if (!result.Item2)
			{
				_logger.LogInformation("Chargebee webhook ignored (no actionable data).");
				return Ok(new
				{
					Status = "Ignored"
				});
			}
			_logger.LogInformation("Chargebee Event: {EventType}, Email: {Email}, SubscriptionId: {SubscriptionId}, Status: {Status}", result.Item6, result.Item3, result.Item4, result.Item5);
			if (!_adminService.UpsertUserSubscriptionStatus(result.Item3, result.Item4, result.Item5, out var isActive))
			{
				_logger.LogWarning("DB update affected 0 rows for {Email}", result.Item3);
			}
			else
			{
				_logger.LogInformation("User {Email} active status recomputed to {IsActive}", result.Item3, isActive);
			}
			return Ok(new
			{
				Status = "Success"
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Chargebee webhook processing error");
			return StatusCode(500, "Internal Server Error");
		}
	}

	[HttpGet("GetActiveSubscribers")]
	public async Task<IActionResult> GetActiveSubscribers()
	{
		try
		{
			List<SubscriptionDetailsModel> result = await _adminService.GetActiveSubscribersAsync();
			return Ok(new
			{
				Status = "Success",
				Count = result.Count,
				Data = result
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error getting active subscribers");
			return StatusCode(500, new
			{
				Status = "Error",
				Message = "Error retrieving active subscribers."
			});
		}
	}

	[HttpGet("GetSubscriptionByEmail")]
	public async Task<IActionResult> GetSubscriptionByEmail(string email)
	{
		try
		{
			SubscriptionDetailsModel result = await _adminService.GetSubscriptionDetailsByEmail(email);
			if (result == null)
			{
				return Ok(new
				{
					Status = "NotFound",
					Message = "Subscription details not found."
				});
			}
			return Ok(new
			{
				Status = "Success",
				Data = result
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error getting subscription details");
			return StatusCode(500, new
			{
				Status = "Error",
				Message = "Error retrieving subscription details."
			});
		}
	}

	[HttpPost("UpdateUserResumeStatus")]
	public IActionResult UpdateUserResumeStatus([FromBody] UserResumeStatusModel model)
	{
		if (model == null || model.UserId <= 0)
		{
			_logger.LogWarning("Invalid input in UpdateUserResumeStatus: {Model}", model);
			return BadRequest("Invalid input");
		}
		try
		{
			_logger.LogInformation("Processing UpdateUserResumeStatus for UserId: {UserId}, IsDelete: {IsDelete}", model.UserId, model.IsDelete);
			if (_adminService.UpdateUserResumeStatus(model))
			{
				return Ok(model.IsDelete ? "User resume deleted successfully." : "User resume restored successfully.");
			}
			return StatusCode(500, "Failed to update user resume status.");
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Exception in UpdateUserResumeStatus for UserId: {UserId}", model.UserId);
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpGet("GetUserResumeStatus/{userId}")]
	public IActionResult GetUserResumeStatus(int userId)
	{
		if (userId <= 0)
		{
			_logger.LogWarning("Invalid UserId in GetUserResumeStatus: {UserId}", userId);
			return BadRequest("Invalid UserId");
		}
		try
		{
			bool isDeleted = _adminService.GetUserResumeStatus(userId);
			return Ok(new
			{
				UserId = userId,
				IsResumeDeleted = isDeleted
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Exception in GetUserResumeStatus for UserId: {UserId}", userId);
			return StatusCode(500, "An error occurred while processing your request.");
		}
	}

	[HttpPost("AddOrUpdateQuestion")]
	public IActionResult AddOrUpdateQuestion([FromBody] QuestionModel model)
	{
		try
		{
			if (model == null)
			{
				_logger.LogWarning("Invalid input received in AddOrUpdateQuestion");
				return BadRequest("Invalid input");
			}
			bool result = _adminService.AddOrUpdateQuestion(model);
			return Ok(new
			{
				Status = (result ? "Question saved successfully" : "Failed to save question")
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error while adding/updating question");
			return StatusCode(500, "Internal server error");
		}
	}

	[HttpDelete("DeleteQuestion")]
	public IActionResult DeleteQuestion(int questionId, int userId)
	{
		try
		{
			if (questionId <= 0)
			{
				_logger.LogWarning("Invalid input in DeleteQuestion: QuestionId={QuestionId}, UserId={UserId}", questionId, userId);
				return BadRequest("Invalid input");
			}
			bool result = _adminService.DeleteQuestion(questionId, userId);
			return Ok(new
			{
				Status = (result ? "Question deleted successfully" : "Failed to delete question")
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error while deleting question. QuestionId={QuestionId}", questionId);
			return StatusCode(500, "Internal server error");
		}
	}

	[HttpGet("GetQuestions")]
	public IActionResult GetQuestions()
	{
		try
		{
			IEnumerable<QuestionResponseModel> data = _adminService.GetQuestions();
			return Ok(new
			{
				Status = "Success",
				Data = data
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error while fetching questions");
			return StatusCode(500, "Internal server error");
		}
	}

	[HttpPost("AddOrUpdateQuestionMaster")]
	public IActionResult AddOrUpdateQuestionMaster([FromBody] QuestionMasterModel model)
	{
		try
		{
			if (model == null)
			{
				return BadRequest("Invalid input");
			}
			bool result = _adminService.AddOrUpdateQuestionMaster(model);
			return Ok(new
			{
				Status = (result ? "Question saved successfully" : "Failed")
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error in AddOrUpdateQuestion");
			return StatusCode(500, "Internal server error");
		}
	}

	[HttpGet("GetQuestionMaster")]
	public IActionResult GetQuestionMaster()
	{
		try
		{
			IEnumerable<QuestionMasterModel> data = _adminService.GetQuestionMaster();
			return Ok(data);
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error in GetQuestions");
			return StatusCode(500, "Internal server error");
		}
	}

	[HttpDelete("DeleteQuestionMaster")]
	public IActionResult DeleteQuestion(int questionId)
	{
		try
		{
			if (questionId <= 0)
			{
				return BadRequest("Invalid QuestionId");
			}
			bool result = _adminService.DeleteQuestionMaster(questionId);
			return Ok(new
			{
				Status = (result ? "Question deleted successfully" : "Failed")
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Error in DeleteQuestion");
			return StatusCode(500, "Internal server error");
		}
	}

	[HttpPost("SendNoticeApproveEmail")]
	public IActionResult SendNoticeApproveEmail([FromBody] SendApproveorRejectEmailRequestModel request)
	{
		if (request == null || request.NoticeId <= 0)
		{
			_logger.LogWarning("SendNoticeApproveEmail called with invalid request. NoticeId: {NoticeId}", request?.NoticeId);
			return BadRequest(new
			{
				message = "Invalid notice id."
			});
		}
		_logger.LogInformation("SendNoticeApproveEmail started for notice {NoticeId}. IsApproved: {IsApproved}", request.NoticeId, request.IsApproved);
		try
		{
			if (_adminService.SendNoticeStatusEmail(request.NoticeId, request.IsApproved, request.RejectionReason))
			{
				_logger.LogInformation("{EmailType} email sent for notice {NoticeId}.", request.IsApproved ? "Approval" : "Rejection", request.NoticeId);
				return Ok(new
				{
					message = (request.IsApproved ? "Approval email sent." : "Rejection email sent.")
				});
			}
			_logger.LogWarning("{EmailType} email NOT sent for notice {NoticeId} — notice missing or no contact email.", request.IsApproved ? "Approval" : "Rejection", request.NoticeId);
			return Ok(new
			{
				message = "Email could not be sent. Check logs for details."
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "SendNoticeApproveEmail failed for notice {NoticeId}.", request.NoticeId);
			return StatusCode(500, new
			{
				message = "Failed to send email."
			});
		}
	}

	[HttpGet("chargebeefromSite")]
	public async Task<IActionResult> GetChargebeePlans()
	{
		try
		{
			return Ok(await _adminService.GetChargebeePlansfromSite());
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Failed to fetch Chargebee plans.");
			return StatusCode(500, new
			{
				message = "Unable to load plans."
			});
		}
	}

	[HttpGet("GetAudienceTypes")]
	public IActionResult GetAudienceTypes()
	{
		try
		{
			return Ok(new
			{
				status = "Success",
				data = _adminService.GetAudienceTypes()
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "GetAudienceTypes failed");
			return StatusCode(500, new
			{
				status = "Error",
				message = "Unable to load audience types."
			});
		}
	}

	[HttpGet("GetAudienceCount/{audienceTypeId}")]
	public IActionResult GetAudienceCount(int audienceTypeId)
	{
		if (audienceTypeId <= 0)
		{
			return BadRequest(new
			{
				message = "Invalid audience type."
			});
		}
		try
		{
			return Ok(new
			{
				status = "Success",
				count = _adminService.GetAudienceCount(audienceTypeId)
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "GetAudienceCount failed for {Id}", audienceTypeId);
			return StatusCode(500, new
			{
				status = "Error"
			});
		}
	}

	[HttpGet("GetEvents")]
	public IActionResult GetEvents()
	{
		try
		{
			return Ok(new
			{
				status = "Success",
				data = _adminService.GetEvents()
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "GetEvents failed");
			return StatusCode(500, new
			{
				status = "Error"
			});
		}
	}

	[HttpPost("AddEvent")]
	public IActionResult AddEvent([FromBody] EventRequestModel model)
	{
		if (model == null || string.IsNullOrWhiteSpace(model.EventName))
		{
			return BadRequest(new
			{
				message = "EventName is required."
			});
		}
		try
		{
			int id = _adminService.AddEvent(model);
			return Ok(new
			{
				status = ((id > 0) ? "Success" : "Failed"),
				eventId = id
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "AddEvent failed");
			return StatusCode(500, new
			{
				status = "Error"
			});
		}
	}

	[HttpPut("UpdateEvent")]
	public IActionResult UpdateEvent([FromBody] EventRequestModel model)
	{
		if (model == null || model.EventId <= 0 || string.IsNullOrWhiteSpace(model.EventName))
		{
			return BadRequest(new
			{
				message = "EventId and EventName are required."
			});
		}
		try
		{
			return Ok(new
			{
				status = (_adminService.UpdateEvent(model) ? "Success" : "Failed")
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "UpdateEvent failed");
			return StatusCode(500, new
			{
				status = "Error"
			});
		}
	}

	[HttpDelete("DeleteEvent/{eventId}")]
	public IActionResult DeleteEvent(int eventId)
	{
		if (eventId <= 0)
		{
			return BadRequest(new
			{
				message = "Invalid event id."
			});
		}
		try
		{
			return Ok(new
			{
				status = (_adminService.DeleteEvent(eventId) ? "Success" : "Failed")
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "DeleteEvent failed");
			return StatusCode(500, new
			{
				status = "Error"
			});
		}
	}

	[HttpGet("GetEventMessages")]
	public IActionResult GetEventMessages([FromQuery] int? eventId = null, [FromQuery] int? audienceTypeId = null)
	{
		try
		{
			return Ok(new
			{
				status = "Success",
				data = _adminService.GetEventMessages(eventId, audienceTypeId)
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "GetEventMessages failed");
			return StatusCode(500, new
			{
				status = "Error"
			});
		}
	}

	[HttpPost("AddEventMessage")]
	public IActionResult AddEventMessage([FromBody] EventMessageRequestModel model)
	{
		if (model == null || model.EventId <= 0 || model.AudienceTypeId <= 0 || string.IsNullOrWhiteSpace(model.Subject) || string.IsNullOrWhiteSpace(model.MessageBody))
		{
			return BadRequest(new
			{
				message = "EventId, AudienceTypeId, Subject and MessageBody are required."
			});
		}
		try
		{
			int id = _adminService.AddEventMessage(model);
			return Ok(new
			{
				status = ((id > 0) ? "Success" : "Failed"),
				eventMessageId = id
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "AddEventMessage failed");
			return StatusCode(500, new
			{
				status = "Error"
			});
		}
	}

	[HttpPut("UpdateEventMessage")]
	public IActionResult UpdateEventMessage([FromBody] EventMessageRequestModel model)
	{
		if (model == null || model.EventMessageId <= 0)
		{
			return BadRequest(new
			{
				message = "EventMessageId is required."
			});
		}
		try
		{
			return Ok(new
			{
				status = (_adminService.UpdateEventMessage(model) ? "Success" : "Failed")
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "UpdateEventMessage failed");
			return StatusCode(500, new
			{
				status = "Error"
			});
		}
	}

	[HttpDelete("DeleteEventMessage/{eventMessageId}")]
	public IActionResult DeleteEventMessage(int eventMessageId)
	{
		if (eventMessageId <= 0)
		{
			return BadRequest(new
			{
				message = "Invalid id."
			});
		}
		try
		{
			return Ok(new
			{
				status = (_adminService.DeleteEventMessage(eventMessageId) ? "Success" : "Failed")
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "DeleteEventMessage failed");
			return StatusCode(500, new
			{
				status = "Error"
			});
		}
	}

	[HttpPost("SendEventMail")]
	public IActionResult SendEventMail([FromBody] SendEventMailRequestModel request)
	{
		if (request == null || request.AudienceTypeId <= 0 || string.IsNullOrWhiteSpace(request.Subject) || string.IsNullOrWhiteSpace(request.MessageBody))
		{
			return BadRequest(new
			{
				message = "AudienceTypeId, Subject and MessageBody are required."
			});
		}
		try
		{
			var (queued, message) = _adminService.SendEventMail(request);
			if (queued < 0)
			{
				return Ok(new
				{
					status = "Failed",
					message = message
				});
			}
			_logger.LogInformation("SendEventMail queued {Count} emails for audience {AudienceTypeId}.", queued, request.AudienceTypeId);
			return Ok(new
			{
				status = "Success",
				queued = queued,
				message = message
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "SendEventMail failed for audience {AudienceTypeId}", request.AudienceTypeId);
			return StatusCode(500, new
			{
				status = "Error",
				message = "Failed to send emails."
			});
		}
	}

	[HttpGet("GetBulkEmailBatches")]
	public IActionResult GetBulkEmailBatches([FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
	{
		try
		{
			IEnumerable<BulkEmailBatchModel> data = _adminService.GetBulkEmailBatches(fromDate, toDate);
			return Ok(new
			{
				status = "Success",
				data = data
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "GetBulkEmailBatches failed");
			return StatusCode(500, new
			{
				status = "Error",
				message = "Unable to load email batches."
			});
		}
	}

	[HttpGet("GetBulkEmailDetails")]
	public IActionResult GetBulkEmailDetails([FromQuery] int? noticeId = null, [FromQuery] bool eventOnly = false, [FromQuery] string subject = null, [FromQuery] string status = null, [FromQuery] string searchEmail = null, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
	{
		if (page < 1)
		{
			page = 1;
		}
		if (pageSize < 1 || pageSize > 500)
		{
			pageSize = 50;
		}
		try
		{
			BulkEmailDetailResponse data = _adminService.GetBulkEmailDetails(noticeId, eventOnly, subject, status, searchEmail, fromDate, toDate, page, pageSize);
			return Ok(new
			{
				status = "Success",
				data = data
			});
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "GetBulkEmailDetails failed");
			return StatusCode(500, new
			{
				status = "Error",
				message = "Unable to load email details."
			});
		}
	}
}

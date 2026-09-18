using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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

namespace NYCastings.API.Core.Contracts.AdminInterface;

public interface IAdminService
{
	bool DismissNotification(DismissNotificationRequestModel request);

	int AddOrUpdateNotificationMessage(NotificationMessageRequest request);

	(IEnumerable<UserDetailsByRoleModel> Users, int Count) GetUserDetailsBasedOnRole(int roleId, string searchText);

	bool DeleteUser(DeleteUserRequestModel request);

	IEnumerable<NotificationMessageResponse> GetNotificationMessages(int notificationId);

	IEnumerable<ActiveRolesModel> GetAllRoles();

	bool ApproveOrRejectNotice(NoticeApprovalRequest request);

	IEnumerable<UserBillingModel> GetUserBillingDetails();

	bool AddOrUpdateUserBilling(UserBillingRequestModel model);

	IEnumerable<PlanDetail> GetPlanDetails();

	bool AddOrUpdatePlanDetails(PlanDetailsRequestModel model);

	bool AddOrUpdateUserVisibility(UserVisibilityRequestModel model);

	bool UpdateUserDetailsById(UpdateUserDetailsRequestModel model);

	NoticeStatusCountResponseModel GetNoticeStatusCounts();

	PaginatedUnapprovedNoticeResponse GetUnapprovedNotices(string status, string email, int page, int pageSize, string sortOrder);

	bool SaveOrUpdateCreditMaster(CreditMasterRequestModel model);

	bool GrantCreditsToInactiveTalents(AdminGrantCreditsRequestModel model);

	List<ChargebeePlanModel> GetPlans();

	Task<string> CreateCheckoutAsync(string email, string itemPriceId);

	bool SaveChargebeePayment(PaymentSaveRequest request);

	Task<string> RedirectToBillingPortalIfActiveAsync(string email);

	Task<(bool Authenticated, bool Success, string Email, string SubscriptionId, string Status, string EventType)> ProcessWebhookAsync(Stream bodyStream, string authHeader);

	bool UpsertUserSubscriptionStatus(string email, string subscriptionId, string status, out bool isActive);

	bool UpdateUserActiveStatusByEmail(string email, bool isActive);

	Task<SubscriptionDetailsModel> GetSubscriptionDetailsByEmail(string email);

	Task<List<SubscriptionDetailsModel>> GetActiveSubscribersAsync();

	bool UpdateUserResumeStatus(UserResumeStatusModel model);

	bool GetUserResumeStatus(int userId);

	bool AddOrUpdateQuestion(QuestionModel model);

	bool DeleteQuestion(int questionId, int userId);

	IEnumerable<QuestionResponseModel> GetQuestions();

	bool AddOrUpdateQuestionMaster(QuestionMasterModel model);

	IEnumerable<QuestionMasterModel> GetQuestionMaster();

	bool DeleteQuestionMaster(int questionId);

	bool SendNoticeStatusEmail(int noticeId, bool isApproved, string rejectionReason = null);

	Task<List<ChargebeeNewPlanModel>> GetChargebeePlansfromSite();

	IEnumerable<AudienceTypeModel> GetAudienceTypes();

	int GetAudienceCount(int audienceTypeId);

	int AddEvent(EventRequestModel model);

	bool UpdateEvent(EventRequestModel model);

	IEnumerable<EventModel> GetEvents();

	bool DeleteEvent(int eventId);

	int AddEventMessage(EventMessageRequestModel model);

	bool UpdateEventMessage(EventMessageRequestModel model);

	IEnumerable<EventMessageModel> GetEventMessages(int? eventId, int? audienceTypeId);

	bool DeleteEventMessage(int eventMessageId);

	(int QueuedCount, string Message) SendEventMail(SendEventMailRequestModel request);

	IEnumerable<BulkEmailBatchModel> GetBulkEmailBatches(DateTime? fromDate, DateTime? toDate);

	BulkEmailDetailResponse GetBulkEmailDetails(int? noticeId, bool eventOnly, string subject, string status, string searchEmail, DateTime? fromDate, DateTime? toDate, int page, int pageSize);
}

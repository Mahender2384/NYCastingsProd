namespace NYCastings.API.Infrastructure.Helpers;

public static class StoredProcedures
{
	public const string AddUser = "AddUser";

	public const string USPVALIDATEUSER = "USP_NEW_VALIDATE_USER";

	public const string USP_ADD_NEWUSER = "ADD_NEWUSER";

	public const string USP_ADD_NEW_NOTICE12 = "USP_ADD_NEW_NOTICE12";

	public const string USP_DELETE_NEW_NOTICE_BY_ID = "[USP_DELETE_NEW_NOTICE_BY_ID]";

	public const string USP_ADD_NEW_ROLECREATOR = "USP_ADD_NEW_ROLECREATOR";

	public const string USP_GET_NEW_USER_ROLES = "USP_GET_NEW_USER_ROLES";

	public const string USP_DELETE_NEW_ROLE = "USP_DELETE_NEW_ROLE";

	public const string USP_APPROVE_NOTICE12 = "USP_APPROVE_NOTICE12";

	public const string USP_GET_LOCATIONS = "USP_GET_LOCATIONS";

	public const string USP_GET_PROJECT_SUBMISSIONS = "USP_GET_PROJECT_SUBMISSIONS";

	public const string USP_GET_TALENT_DETAILS_FROM_LIST = "USP_GET_TALENT_DETAILS_FROM_LIST";

	public const string USP_UPDATE_USER_LIST = "USP_UPDATE_USER_LIST";

	public const string USP_ADD_TALENT_NOTE = "USP_ADD_TALENT_NOTE";

	public const string USP_GET_TALENT_NOTES = "USP_GET_TALENT_NOTES";

	public const string USP_AddOrUpdateClientFave = "USP_AddOrUpdateClientFave";

	public const string USP_GetClientFaveList = "USP_GetClientFaveList";

	public const string USP_GETINBOXMESSAGESDETAILS = "USP_GETINBOXMESSAGESDETAILS";

	public const string USP_GETSENTMESSAGEDETAILS = "USP_GETSENTMESSAGEDETAILS";

	public const string USP_GETARCHIVEMESSAGEDETAILS = "USP_GETARCHIVEMESSAGEDETAILS";

	public const string USP_INSERT_MESSAGE = "USP_INSERT_MESSAGE";

	public const string USP_ARCHIVE_MESSAGES = "USP_ARCHIVE_MESSAGES";

	public const string USP_DELETE_MESSAGES = "USP_DELETE_MESSAGES";

	public const string USP_GETCLIENTDETAILS = "USP_GETCLIENTDETAILS";

	public const string USP_UPDATE_CLIENT_DETAILS = "USP_UPDATE_CLIENT_DETAILS";

	public const string USP_CHECK_EMAIL_EXISTS = "USP_CHECK_EMAIL_EXISTS";

	public const string USP_CHECK_USERNAME_EXISTS = "USP_CHECK_USERNAME_EXISTS";

	public const string USP_GET_USER_ACTIVE_STATUS = "USP_GET_USER_ACTIVE_STATUS";

	public const string USP_GET_TALENT_FULL_PROFILE = "USP_GET_TALENT_FULL_PROFILE";

	public const string USP_UPDATE_TALENT_FULL_PROFILE = "USP_UPDATE_TALENT_FULL_PROFILE";

	public const string USP_GET_ETHNICITIES = "USP_GET_ETHNICITIES";

	public const string USP_GET_VOCAL_RANGES = "USP_GET_VOCAL_RANGES";

	public const string USP_GET_EYE_COLOR = "USP_GET_EYE_COLOR";

	public const string USP_GET_HAIR_COLOR = "USP_GET_HAIR_COLOR";

	public const string USP_ADD_OR_SYNC_USER_TALENTS = "USP_ADD_OR_SYNC_USER_TALENTS";

	public const string USP_GET_ACTIVE_TALENTS = "USP_GET_ACTIVE_TALENTS";

	public const string USP_GET_USER_TALENTS = "USP_GET_USER_TALENTS";

	public const string USP_GET_USER_AFFILIATIONS = "USP_GET_USER_AFFILIATIONS";

	public const string USP_ADD_OR_SYNC_USER_AFFILIATIONS = "USP_ADD_OR_SYNC_USER_AFFILIATIONS";

	public const string USP_GET_ALL_AFFILIATIONS = "USP_GET_ALL_AFFILIATIONS";

	public const string USP_GET_USER_VOCAL_TYPES = "USP_GET_USER_VOCAL_TYPES";

	public const string USP_ADD_OR_SYNC_USER_VOCAL_TYPES = "USP_ADD_OR_SYNC_USER_VOCAL_TYPES";

	public const string USP_GET_VOCAL_TYPES = "USP_GET_VOCAL_TYPES";

	public const string USP_GET_USER_MEDIA = "USP_GET_USER_MEDIA";

	public const string USP_GET_USER_RESUME_TEXT_DATA_NEW = "USP_GET_USER_RESUME_TEXT_DATA_NEW";

	public const string USP_GET_RESUME_SECTIONS_WITH_DETAILS = "USP_GET_RESUME_SECTIONS_WITH_DETAILS";

	public const string USP_ADD_OR_UPDATE_USER_RESUME_TEXT_DATA = "USP_ADD_OR_UPDATE_USER_RESUME_TEXT_DATA";

	public const string USP_ADD_OR_UPDATE_RESUME_SECTION = "USP_ADD_OR_UPDATE_RESUME_SECTION";

	public const string USP_ADD_OR_UPDATE_RESUME_SECTION_DETAIL = "USP_ADD_OR_UPDATE_RESUME_SECTION_DETAIL";

	public const string USP_ADD_OR_UPDATE_USER_MEDIA = "USP_ADD_OR_UPDATE_USER_MEDIA";

	public const string USP_GET_USER_NAVIGATION_STAGE = "USP_GET_USER_NAVIGATION_STAGE";

	public const string USP_GET_REPRESENTATION_INFORMATION = "USP_GET_REPRESENTATION_INFORMATION";

	public const string USP_ADD_OR_UPDATE_REPRESENTATION_INFORMATION = "USP_ADD_OR_UPDATE_REPRESENTATION_INFORMATION";

	public const string USP_GET_CASTING_NOTICE_FILTERED = "USP_GET_CASTING_NOTICE_FILTERED";

	public const string USP_GET_LOCATION_BY_STATE_ABBR_LIST = "USP_GET_LOCATION_BY_STATE_ABBR_LIST";

	public const string USP_INSERT_DIRECT_SUBMIT_HISTORY = "USP_INSERT_DIRECT_SUBMIT_HISTORY";

	public const string USP_GET_USER_JOB_HISTORY = "USP_GET_USER_JOB_HISTORY";

	public const string USP_GET_PAY_TYPES = "USP_GET_PAY_TYPES";

	public const string USP_GET_ACTIVE_JOB_CATEGORIES = "USP_GET_ACTIVE_JOB_CATEGORIES";

	public const string USP_GetUserResumeViewsWithStatusCounts = "USP_GetUserResumeViewsWithStatusCounts";

	public const string USP_AddOrUpdateUserResumeViews = "USP_AddOrUpdateUserResumeViews";

	public const string USP_AddOrUpdate_UserNotifPrefs = "USP_AddOrUpdate_UserNotifPrefs";

	public const string USP_GetInboxTalentMessages = "USP_GetInboxTalentMessages";

	public const string USP_Get_UserNotifPrefs_ByUserId = "USP_Get_UserNotifPrefs_ByUserId";

	public const string USP_GetEmailsByCityChoices = "USP_GetEmailsByCityChoices";

	public const string USP_GetActiveNotificationsByUser = "USP_GetActiveNotificationsByUser";

	public const string USP_DismissNotification = "USP_DismissNotification";

	public const string USP_AddOrUpdateNotificationMessage = "USP_AddOrUpdateNotificationMessage";

	public const string USP_ChangeUserPassword = "USP_ChangeUserPassword";

	public const string USP_DeleteUser = "USP_DeleteUser";

	public const string USP_GetNotificationMessages = "USP_GetNotificationMessages";

	public const string USP_GetAllRoles = "USP_GetAllRoles";

	public const string USP_GetunApprovedNotices = "USP_GetunApprovedNotices";

	public const string USP_New_ForgotPassword = "USP_New_ForgotPassword";

	public const string USP_GetUserBillingDetails = "USP_GetUserBillingDetails";

	public const string USP_AddOrUpdateUserBilling = "USP_AddOrUpdateUserBilling";

	public const string USP_PlanDetails = "USP_PlanDetails";

	public const string USP_AddOrUpdatePlanDetails = "USP_AddOrUpdatePlanDetails";

	public const string USP_ApproveOrRejectNotice = "USP_ApproveOrRejectNotice";

	public const string USP_GET_NEW_NOTICE_DETAILS = "USP_GET_NEW_NOTICE_DETAILS";

	public const string USP_Get_RepresentationHeadings = "USP_Get_RepresentationHeadings";

	public const string USP_HideandunHideTalent = "USP_HideandunHideTalent";

	public const string USP_SEARCH_PROFILES_DYNAMIC = "USP_SEARCH_PROFILES_DYNAMIC";

	public const string USP_GetHidedTalents = "USP_GetHidedTalents";

	public const string USP_GetAffiliations_CustomOrder = "USP_GetAffiliations_CustomOrder";

	public const string USP_GET_USERID = "USP_GET_USERID";

	public const string USP_GET_USERNAME = "USP_GET_USERNAME";

	public const string USP_UPDATE_RESUMENAME = "USP_UPDATE_RESUMENAME";

	public const string USP_AddOrUpdateUserVisibility = "USP_AddOrUpdateUserVisibility";

	public const string USP_UpdateUserDetailsById = "USP_UpdateUserDetailsById";

	public const string USP_GetNoticeStatusCounts = "USP_GetNoticeStatusCounts";

	public const string USP_SAVE_CREDIT_MASTER = "USP_SAVE_CREDIT_MASTER";

	public const string USP_ADMIN_GRANT_CREDITS_TO_INACTIVE_TALENTS = "USP_ADMIN_GRANT_CREDITS_TO_INACTIVE_TALENTS";

	public const string USP_GetChargebeePlans = "USP_GetChargebeePlans";

	public const string USP_SaveChargebeePayment = "USP_SaveChargebeePayment";

	public const string USP_UPDATE_USER_ACTIVE_STATUS_BY_EMAIL = "USP_UPDATE_USER_ACTIVE_STATUS_BY_EMAIL";

	public const string USP_SaveUserPrivacySettings = "USP_SaveUserPrivacySettings";

	public const string USP_GetUserPrivacySettings = "USP_GetUserPrivacySettings";

	public const string USP_GetUnreadMessageCount = "USP_GetUnreadMessageCount";

	public const string USP_SaveUserCastingSearch = "USP_SaveUserCastingSearch";

	public const string USP_GetUserCastingSearch = "USP_GetUserCastingSearch";

	public const string USP_GET_EXPIRED_CASTING_NOTICE_FILTERED = "USP_GET_EXPIRED_CASTING_NOTICE_FILTERED";

	public const string USP_SaveTalentFavoriteNotice = "USP_SaveTalentFavoriteNotice";

	public const string USP_UPDATE_USER_RESUME_STATUS = "USP_UPDATE_USER_RESUME_STATUS";

	public const string USP_GET_USER_RESUME_STATUS = "USP_GET_USER_RESUME_STATUS";

	public const string USP_ADD_UPDATE_QUESTION = "USP_ADD_UPDATE_QUESTION";

	public const string USP_DELETE_QUESTION = "USP_DELETE_QUESTION";

	public const string USP_GET_QUESTIONS = "USP_GET_QUESTIONS";

	public const string USP_ADD_UPDATE_QUESTION_MASTER = "USP_ADD_UPDATE_QUESTION_MASTER";

	public const string USP_GET_QUESTION_MASTER = "USP_GET_QUESTION_MASTER";

	public const string USP_DELETE_QUESTIONMASTER = "USP_DELETE_QUESTIONMASTER";

	public const string USP_GET_USERNAME_BY_EMAIL = "USP_GET_USERNAME_BY_EMAIL";

	public const string USP_GET_CASTING_NOTICES_BY_DATE_RANGE = "USP_GET_CASTING_NOTICES_BY_DATE_RANGE";

	public const string USP_IS_VERIFIED_DIRECTOR = "USP_IS_VERIFIED_DIRECTOR";

	public const string USP_GET_COUNTRY_CODES = "USP_GET_COUNTRY_CODES";

	public const string USP_GET_EVENT_MAIL_AUDIENCE_TYPES = "USP_GET_EVENT_MAIL_AUDIENCE_TYPES";

	public const string USP_ADD_EVENT_MAIL_EVENT = "USP_ADD_EVENT_MAIL_EVENT";

	public const string USP_UPDATE_EVENT_MAIL_EVENT = "USP_UPDATE_EVENT_MAIL_EVENT";

	public const string USP_GET_EVENT_MAIL_EVENTS = "USP_GET_EVENT_MAIL_EVENTS";

	public const string USP_DELETE_EVENT_MAIL_EVENT = "USP_DELETE_EVENT_MAIL_EVENT";

	public const string USP_ADD_EVENT_MAIL_MESSAGE = "USP_ADD_EVENT_MAIL_MESSAGE";

	public const string USP_UPDATE_EVENT_MAIL_MESSAGE = "USP_UPDATE_EVENT_MAIL_MESSAGE";

	public const string USP_GET_EVENT_MAIL_MESSAGES = "USP_GET_EVENT_MAIL_MESSAGES";

	public const string USP_DELETE_EVENT_MAIL_MESSAGE = "USP_DELETE_EVENT_MAIL_MESSAGE";

	public const string USP_QUEUE_EVENT_MAIL_EMAILS = "USP_QUEUE_EVENT_MAIL_EMAILS";

	public const string USP_GET_EVENT_MAIL_AUDIENCE_COUNT = "USP_GET_EVENT_MAIL_AUDIENCE_COUNT";

	public const string USP_GET_BULK_EMAIL_BATCHES = "USP_GET_BULK_EMAIL_BATCHES";

	public const string USP_GET_BULK_EMAIL_DETAILS = "USP_GET_BULK_EMAIL_DETAILS";

	public const string USP_UPSERT_USER_SUBSCRIPTION_STATUS = "USP_UPSERT_USER_SUBSCRIPTION_STATUS";
}

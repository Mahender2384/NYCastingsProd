using System.Collections.Generic;
using System.Threading.Tasks;
using NYCastings.API.Core.Models.DirectSubmitHistoryRequest;
using NYCastings.API.Core.Models.Filtered_Casting_Notices;
using NYCastings.API.Core.Models.JobHistoryModel;
using NYCastings.API.Core.Models.LocationModel;
using NYCastings.API.Core.Models.ResumeViewsModel;
using NYCastings.API.Core.Models.SaveCastingSearchModel;
using NYCastings.API.Core.Models.UserEmail;
using NYCastings.API.Core.Models.UserNotifications;

namespace NYCastings.API.Core.Contracts.SearchNoticeInterface;

public interface ISearchNoticeInterface
{
	IEnumerable<CastingNoticeResponseModel> GetFilteredCastingNotices(CastingNoticeFilterRequest filter);

	IEnumerable<CastingNoticeResponseModel> GetExpiredFilteredCastingNotices(int userId);

	IEnumerable<LocationModel> GetLocationsByStateAbbreviations(string stateAbbrCsv);

	Task<string> SubmitDirectHistory(DirectSubmitHistoryRequest request);

	IEnumerable<JobHistoryModel> GetUserJobHistory(int userId);

	ResumeMessageResponse GetResumeMessagesWithStatusCounts(int userId);

	bool AddOrUpdateUserResumeView(UserResumeViewModel model);

	bool AddOrUpdateUserNotificationPrefs(UserNotificationPrefsRequest model);

	IEnumerable<UserNotifPrefsModel> GetUserNotifPrefsByUserId(int userId);

	Task<int> SendCityBasedRoleAlertEmails(RoleAlertEmailRequest request);

	bool SaveUserCastingSearch(SaveCastingSearchRequest model);

	SaveCastingSearchRequest GetUserCastingSearch(string email);
}

using System.Collections.Generic;
using NYCastings.API.Core.Contracts.JobCategoryModel;
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

namespace NYCastings.API.Core.Contracts.UserInterface;

public interface IUserService
{
	object Authenticate(string username, string password);

	bool AddUserDetails(AddUserDetails usersDetails);

	UserDetails GetUserDetails(string username, string password);

	IEnumerable<LocationModel> GetLocations();

	IEnumerable<EthnicityModel> GetAllEthnicities();

	IEnumerable<VocalRangeModel> GetAllVocalRanges();

	IEnumerable<EyeColorModel> GetAllEyeColors();

	IEnumerable<HairColorModel> GetAllHairColors();

	IEnumerable<TalentModel> GetActiveTalents();

	IEnumerable<AffiliationModel> GetAllAffiliations();

	IEnumerable<VocalTypeModel> GetVocalTypes();

	UserNavigationStageResponse GetUserNavigationStage(int userId);

	IEnumerable<PayTypeModel> GetAllPayTypes();

	IEnumerable<JobCategory> GetAllJobCategories();

	bool ChangeUserPassword(ChangePasswordRequestModel request);

	IEnumerable<NotificationMessageModel> GetActiveNotificationsByUser(int userId, string userRole);

	bool ForgotPassword(ForgotPasswordRequestModel request, out string newPassword);

	IEnumerable<RepresentationHeadingModel> GetRepresentationHeadings();

	IEnumerable<Affiliation2Model> GetCustomOrderedAffiliations();

	bool SendContactUsEmail(string name, string email, string department, string message);

	string GetUserName(int userId);

	int GetUserId(string userName);

	bool UpdateResumeName(int userId, string name);

	bool SaveUserPrivacySettings(SaveUserPrivacySettingsRequestModel model);

	UserPrivacySettingsResponseModel GetUserPrivacySettings(int userId);

	bool ForgotUsername(ForgotUsernameRequestModel request);

	bool IsVerifiedDirector(int userId);

	IEnumerable<CountryCodeModel> GetCountryCodes();

	bool UnsubscribeUser(int userId, string email);
}

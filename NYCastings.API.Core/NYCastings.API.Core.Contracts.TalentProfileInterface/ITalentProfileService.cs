using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NYCastings.API.Core.Models.NotesModel;
using NYCastings.API.Core.Models.RepresentationInfoModel;
using NYCastings.API.Core.Models.TalentFullProfileModel;
using NYCastings.API.Core.Models.UserAffiliationModel;
using NYCastings.API.Core.Models.UserMediaModel;
using NYCastings.API.Core.Models.UserResumeTextDataModel;
using NYCastings.API.Core.Models.UserVocalTypeModel;

namespace NYCastings.API.Core.Contracts.TalentProfileInterface;

public interface ITalentProfileService
{
	TalentFullProfileModel GetTalentFullProfile(int userId);

	Task<bool> UpdateTalentProfile(TalentProfileUpdateModel model);

	bool AddOrSyncUserTalents(UserTalentUpdateModel model);

	IEnumerable<UserTalentModel> GetUserTalents(int userId);

	IEnumerable<UserAffiliationModel> GetUserAffiliations(int userId);

	bool AddOrSyncUserAffiliations(UserAffiliationUpdateModel model);

	IEnumerable<UserVocalTypeModel> GetUserVocalTypes(int userId);

	bool AddOrSyncUserVocalTypes(UpdateUserVocalTypesRequest model);

	IEnumerable<UserMediaModel> GetUserMedia(int userId);

	IEnumerable<UserResumeTextDataModel> GetUserResumeTextData(int userId);

	IEnumerable<ResumeSectionWithDetailsModel> GetResumeSectionsWithDetails(int userId);

	bool AddOrUpdateResumeTextData(ResumeTextDataModel model);

	bool AddOrUpdateResumeSection(ResumeSectionRequest model);

	bool AddOrUpdateResumeSectionDetail(ResumeSectionDetailRequest model);

	Task<bool> AddOrUpdateUserMediaAsync(UserMediaRequest model, CancellationToken cancellationToken = default(CancellationToken));

	IEnumerable<RepresentationInfoModel> GetRepresentationInformation(int userId, string headingName);

	bool AddOrUpdateRepresentationInformation(RepresentationInfoRequestModel model);

	bool SaveTalentFavoriteNotice(SaveTalentFavoriteNoticeRequestModel model);
}

using System.Collections.Generic;
using System.Threading.Tasks;
using NYCastings.API.Core.Models.ArchiveMessageDetailsModel;
using NYCastings.API.Core.Models.ClientDetailsModel;
using NYCastings.API.Core.Models.FavoriteModel;
using NYCastings.API.Core.Models.InboxMessageDetailsModel;
using NYCastings.API.Core.Models.MessageModel;
using NYCastings.API.Core.Models.NewCastingNoticeModel;
using NYCastings.API.Core.Models.NotesModel;
using NYCastings.API.Core.Models.ProjectSubmissionModel;
using NYCastings.API.Core.Models.RoleModal;
using NYCastings.API.Core.Models.SentMessageDetailsModel;
using NYCastings.API.Core.Models.TalentDetailsfromListModel;
using NYCastings.API.Core.Models.UpdateUserinListModel;

namespace NYCastings.API.Core.Contracts.NewCastingNoticeInterface;

public interface INewCastingNoticeService
{
	Task<int> AddNoticeDetailsAsync(AddNewCastingNoticeDetails noticeDetails);

	IEnumerable<GetNewCastingNoticeDetails> GetNewCastingNoticeDetails(int noticeId, string email);

	bool DeleteCastingNoticeData(int noticeId);

	Task<bool> AddOrUpdateRoleWithFile(AddRoleDetails roleDetails);

	IEnumerable<GetRoleDetails> GetRoleDetails(int? roleId, int? noticeId, string? roleName, string? sex);

	bool DeleteRoleData(int roleId);

	bool ApproveNotice(int noticeId, bool approve, int userId);

	bool NotifyUserOnNoticeSubmission(string userName, string userEmail);

	IEnumerable<ProjectSubmissionModel> GetProjectSubmissions(int noticeId);

	IEnumerable<TalentDetailsModel> GetTalentDetails(string userIds, int roleId, int clientId, string sortBy);

	bool UpdateUserList(UpdateUserListModel updateModel);

	bool AddOrUpdateTalentNote(TalentNotes note);

	List<TalentNotes> GetTalentNotes(int? talentId = null, int? directorId = null);

	bool AddOrUpdateClientFave(ClientFaveModel clientFaveModel);

	IEnumerable<ClientFaveTalentModel> GetClientFavTalentDetails(int clientId);

	IEnumerable<InboxMessageDetails> GetInboxMessagesDetails(string email, string sortOrder);

	IEnumerable<SentMessageDetails> GetSentMessagesDetails(string email);

	IEnumerable<ArchiveMessageDetails> GetArchiveMessagesDetails(string email, string User, string sortOrder);

	IEnumerable<MessageModel> ShowMessages(int originalMessageId, string status, string user);

	Task<int> SendMessage(SendMessageModel message);

	bool ToggleArchiveMessages(string originalMsgIdsCsv, bool activeFlag, string userId);

	bool DeleteMessages(string originalMsgIdsCsv, bool activeFlag, bool permanentArchive, string userId);

	IEnumerable<ClientDetailsModel> GetClientDetails(string clientEmail);

	Task<bool> UpdateClientDetails(UpdateClientDetailsModel client);

	bool CheckEmailExists(string email);

	IEnumerable<InboxTalentMessageModel> GetInboxTalentMessages(string email, string sortOrder);

	int GetUnreadMessageCount(string email);

	bool CheckUserNameExists(string userName);

	int GetUserActiveStatus(int userId);
}

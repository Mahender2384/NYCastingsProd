using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NYCasting.Core.Models;
using NYCasting.Infrastructure.DataAccess;
using NYCastings.API.Core.Contracts.TalentProfileInterface;
using NYCastings.API.Core.Models.NotesModel;
using NYCastings.API.Core.Models.RepresentationInfoModel;
using NYCastings.API.Core.Models.TalentFullProfileModel;
using NYCastings.API.Core.Models.UserAffiliationModel;
using NYCastings.API.Core.Models.UserMediaModel;
using NYCastings.API.Core.Models.UserResumeTextDataModel;
using NYCastings.API.Core.Models.UserVocalTypeModel;

namespace NYCastings.API.Infrastructure.Services;

public class TalentProfileService : BaseApiService, ITalentProfileService
{
	private readonly DbManager _dbManager;

	private readonly IHttpContextAccessor _httpContextAccessor;

	private readonly IConfiguration _configuration;

	public TalentProfileService(IOptions<ConnectionString> dbConfig, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
		: base(dbConfig)
	{
		if (dbConfig == null || string.IsNullOrWhiteSpace(dbConfig.Value.NYCasting))
		{
			throw new ArgumentNullException("Connection string for NYCasting is missing.");
		}
		_dbManager = new DbManager(dbConfig.Value.NYCasting);
		_httpContextAccessor = httpContextAccessor;
		_configuration = configuration;
	}

	public TalentFullProfileModel GetTalentFullProfile(int userId)
	{
		string procedureName = "USP_GET_TALENT_FULL_PROFILE";
		Dictionary<string, object> parameters = new Dictionary<string, object> { { "@UserId", userId } };
		DataTable dataTable = _dbManager.ReadData(procedureName, CommandType.StoredProcedure, parameters);
		if (dataTable.Rows.Count == 0)
		{
			return new TalentFullProfileModel();
		}
		DataRow row = dataTable.Rows[0];
		return new TalentFullProfileModel
		{
			LastName = ((row["LastName"] != DBNull.Value) ? (row["LastName"] as string) : null),
			FirstName = ((row["FirstName"] != DBNull.Value) ? (row["FirstName"] as string) : null),
			AgeRangeFrom = ((row["AgeRangeFrom"] != DBNull.Value) ? new int?(Convert.ToInt32(row["AgeRangeFrom"])) : ((int?)null)),
			AgeRangeTo = ((row["AgeRangeTo"] != DBNull.Value) ? new int?(Convert.ToInt32(row["AgeRangeTo"])) : ((int?)null)),
			Sex = ((row["Sex"] != DBNull.Value) ? (row["Sex"] as string) : null),
			EthnicityCode = ((row["EthnicityCode"] != DBNull.Value) ? new int?(Convert.ToInt32(row["EthnicityCode"])) : ((int?)null)),
			Ethnicity = ((row["Ethnicity"] != DBNull.Value) ? (row["Ethnicity"] as string) : null),
			PrimaryContactNumber = ((row["PrimaryContactNumber"] != DBNull.Value) ? (row["PrimaryContactNumber"] as string) : null),
			SecondaryContactNumber = ((row["Secondarycontactnumber"] != DBNull.Value) ? (row["Secondarycontactnumber"] as string) : null),
			Location = ((row["Location"] != DBNull.Value) ? (row["Location"] as string) : null),
			StateCode = ((row["StateCode"] != DBNull.Value) ? (row["StateCode"] as string) : null),
			Email = ((row["Email"] != DBNull.Value) ? (row["Email"] as string) : null),
			EyeColorCode = ((row["EyeColorCode"] != DBNull.Value) ? new int?(Convert.ToInt32(row["EyeColorCode"])) : ((int?)null)),
			EyeColor = ((row["EyeColor"] != DBNull.Value) ? (row["EyeColor"] as string) : null),
			HairColorCode = ((row["HairColorCode"] != DBNull.Value) ? new int?(Convert.ToInt32(row["HairColorCode"])) : ((int?)null)),
			HairColor = ((row["HairColor"] != DBNull.Value) ? (row["HairColor"] as string) : null),
			Weight = ((row["Weight"] != DBNull.Value) ? new int?(Convert.ToInt32(row["Weight"])) : ((int?)null)),
			HeightStart = ((row["HeightStart"] != DBNull.Value) ? new int?(Convert.ToInt32(row["HeightStart"])) : ((int?)null)),
			HeightEnd = ((row["HeightEnd"] != DBNull.Value) ? new int?(Convert.ToInt32(row["HeightEnd"])) : ((int?)null)),
			VocalRangeID = ((row["VocalRangeID"] != DBNull.Value) ? new int?(Convert.ToInt32(row["VocalRangeID"])) : ((int?)null)),
			VocalRangeName = ((row["VocalRangeName"] != DBNull.Value) ? (row["VocalRangeName"] as string) : null),
			Dress = ((row["Dress"] != DBNull.Value) ? (row["Dress"] as string) : null),
			Bust = ((row["Bust"] != DBNull.Value) ? (row["Bust"] as string) : null),
			Waist = ((row["Waist"] != DBNull.Value) ? (row["Waist"] as string) : null),
			Hips = ((row["Hips"] != DBNull.Value) ? (row["Hips"] as string) : null),
			Shoe = ((row["Shoe"] != DBNull.Value) ? (row["Shoe"] as string) : null),
			MaleSuit = ((row["MaleSuit"] != DBNull.Value) ? (row["MaleSuit"] as string) : null),
			MaleShirt = ((row["MaleShirt"] != DBNull.Value) ? (row["MaleShirt"] as string) : null),
			MaleWaist = ((row["MaleWaist"] != DBNull.Value) ? (row["MaleWaist"] as string) : null),
			MaleInseam = ((row["MaleInseam"] != DBNull.Value) ? (row["MaleInseam"] as string) : null),
			MaleShoe = ((row["MaleShoe"] != DBNull.Value) ? (row["MaleShoe"] as string) : null),
			MiddleName = ((row["MiddleName"] != DBNull.Value) ? (row["MiddleName"] as string) : null),
			HasDriverLicencse = ((row["HasDriverLicencse"] != DBNull.Value) ? ((bool?)row["HasDriverLicencse"]) : ((bool?)null)),
			HasPassport = ((row["HasPassport"] != DBNull.Value) ? ((bool?)row["HasPassport"]) : ((bool?)null)),
			FaveCount = Convert.ToInt32(row["FaveCount"]),
			FaceBook = ((row["FaceBook"] != DBNull.Value) ? (row["FaceBook"] as string) : null),
			Instagram = ((row["Instagram"] != DBNull.Value) ? (row["Instagram"] as string) : null),
			TikTok = ((row["TikTok"] != DBNull.Value) ? (row["TikTok"] as string) : null),
			PersonalWebSite = ((row["PersonalWebSite"] != DBNull.Value) ? (row["PersonalWebSite"] as string) : null),
			PaymentLabel1 = ((row["PaymentLabel1"] != DBNull.Value) ? (row["PaymentLabel1"] as string) : null),
			PaymentLink1 = ((row["PaymentLink1"] != DBNull.Value) ? (row["PaymentLink1"] as string) : null),
			PaymentLabel2 = ((row["PaymentLabel2"] != DBNull.Value) ? (row["PaymentLabel2"] as string) : null),
			PaymentLink2 = ((row["PaymentLink2"] != DBNull.Value) ? (row["PaymentLink2"] as string) : null),
			PaymentLabel3 = ((row["PaymentLabel3"] != DBNull.Value) ? (row["PaymentLabel3"] as string) : null),
			PaymentLink3 = ((row["PaymentLink3"] != DBNull.Value) ? (row["PaymentLink3"] as string) : null),
			YouTube = ((row["YouTube"] != DBNull.Value) ? (row["YouTube"] as string) : null),
			IMDB = ((row["IMDB"] != DBNull.Value) ? (row["IMDB"] as string) : null),
			SnapChat = ((row["SnapChat"] != DBNull.Value) ? (row["SnapChat"] as string) : null),
			City = ((row["City"] != DBNull.Value) ? (row["City"] as string) : null),
			Affiliations = ((row["Affiliations"] != DBNull.Value) ? (row["Affiliations"] as string) : null)
		};
	}

	public async Task<bool> UpdateTalentProfile(TalentProfileUpdateModel model)
	{
		string procedureName = "USP_UPDATE_TALENT_FULL_PROFILE";
		Dictionary<string, object> obj = new Dictionary<string, object>
		{
			{ "@UserId", model.UserId },
			{
				"@LastName",
				((object)model.LastName) ?? ((object)DBNull.Value)
			},
			{
				"@MiddleName",
				((object)model.MiddleName) ?? ((object)DBNull.Value)
			},
			{
				"@FirstName",
				((object)model.FirstName) ?? ((object)DBNull.Value)
			}
		};
		int? ageRangeFrom = model.AgeRangeFrom;
		obj.Add("@AgeRangeFrom", ageRangeFrom.HasValue ? ((object)ageRangeFrom.GetValueOrDefault()) : DBNull.Value);
		ageRangeFrom = model.AgeRangeTo;
		obj.Add("@AgeRangeTo", ageRangeFrom.HasValue ? ((object)ageRangeFrom.GetValueOrDefault()) : DBNull.Value);
		obj.Add("@Sex", ((object)model.Sex) ?? ((object)DBNull.Value));
		ageRangeFrom = model.EthnicityCode;
		obj.Add("@EthnicityId", ageRangeFrom.HasValue ? ((object)ageRangeFrom.GetValueOrDefault()) : DBNull.Value);
		obj.Add("@PrimaryContactNumber", ((object)model.PrimaryContactNumber) ?? ((object)DBNull.Value));
		obj.Add("@StateCode", ((object)model.StateCode) ?? ((object)DBNull.Value));
		obj.Add("@Email", ((object)model.Email) ?? ((object)DBNull.Value));
		ageRangeFrom = model.EyeColorCode;
		obj.Add("@EyeColorId", ageRangeFrom.HasValue ? ((object)ageRangeFrom.GetValueOrDefault()) : DBNull.Value);
		ageRangeFrom = model.HairColorCode;
		obj.Add("@HairColorId", ageRangeFrom.HasValue ? ((object)ageRangeFrom.GetValueOrDefault()) : DBNull.Value);
		ageRangeFrom = model.Weight;
		obj.Add("@Weight", ageRangeFrom.HasValue ? ((object)ageRangeFrom.GetValueOrDefault()) : DBNull.Value);
		ageRangeFrom = model.HeightStart;
		obj.Add("@HeightStart", ageRangeFrom.HasValue ? ((object)ageRangeFrom.GetValueOrDefault()) : DBNull.Value);
		ageRangeFrom = model.HeightEnd;
		obj.Add("@HeightEnd", ageRangeFrom.HasValue ? ((object)ageRangeFrom.GetValueOrDefault()) : DBNull.Value);
		ageRangeFrom = model.VocalRangeID;
		obj.Add("@VocalRangeId", ageRangeFrom.HasValue ? ((object)ageRangeFrom.GetValueOrDefault()) : DBNull.Value);
		obj.Add("@Dress", ((object)model.Dress) ?? ((object)DBNull.Value));
		obj.Add("@Bust", ((object)model.Bust) ?? ((object)DBNull.Value));
		obj.Add("@Waist", ((object)model.Waist) ?? ((object)DBNull.Value));
		obj.Add("@Hips", ((object)model.Hips) ?? ((object)DBNull.Value));
		obj.Add("@Shoe", ((object)model.Shoe) ?? ((object)DBNull.Value));
		obj.Add("@MaleSuit", ((object)model.MaleSuit) ?? ((object)DBNull.Value));
		obj.Add("@MaleShirt", ((object)model.MaleShirt) ?? ((object)DBNull.Value));
		obj.Add("@MaleWaist", ((object)model.MaleWaist) ?? ((object)DBNull.Value));
		obj.Add("@MaleInseam", ((object)model.MaleInseam) ?? ((object)DBNull.Value));
		obj.Add("@MaleShoe", ((object)model.MaleShoe) ?? ((object)DBNull.Value));
		bool? hasDriverLicencse = model.HasDriverLicencse;
		obj.Add("@HasDriverLicencse", hasDriverLicencse.HasValue ? ((object)(hasDriverLicencse == true)) : DBNull.Value);
		hasDriverLicencse = model.HasPassport;
		obj.Add("@HasPassport", hasDriverLicencse.HasValue ? ((object)(hasDriverLicencse == true)) : DBNull.Value);
		obj.Add("@FaceBook", ((object)model.FaceBook) ?? ((object)DBNull.Value));
		obj.Add("@Instagram", ((object)model.Instagram) ?? ((object)DBNull.Value));
		obj.Add("@TikTok", ((object)model.TikTok) ?? ((object)DBNull.Value));
		obj.Add("@PersonalWebSite", ((object)model.PersonalWebSite) ?? ((object)DBNull.Value));
		obj.Add("@PaymentLabel1", ((object)model.PaymentLabel1) ?? ((object)DBNull.Value));
		obj.Add("@PaymentLink1", ((object)model.PaymentLink1) ?? ((object)DBNull.Value));
		obj.Add("@PaymentLabel2", ((object)model.PaymentLabel2) ?? ((object)DBNull.Value));
		obj.Add("@PaymentLink2", ((object)model.PaymentLink2) ?? ((object)DBNull.Value));
		obj.Add("@PaymentLabel3", ((object)model.PaymentLabel3) ?? ((object)DBNull.Value));
		obj.Add("@PaymentLink3", ((object)model.PaymentLink3) ?? ((object)DBNull.Value));
		obj.Add("@YouTube", ((object)model.YouTube) ?? ((object)DBNull.Value));
		obj.Add("@IMDB", ((object)model.IMDB) ?? ((object)DBNull.Value));
		obj.Add("@SnapChat", ((object)model.SnapChat) ?? ((object)DBNull.Value));
		obj.Add("@SecondaryContactNumber", ((object)model.SecondaryContactNumber) ?? ((object)DBNull.Value));
		obj.Add("@City", ((object)model.City) ?? ((object)DBNull.Value));
		float? lattitude = model.Lattitude;
		obj.Add("@Lattitude", lattitude.HasValue ? ((object)lattitude.GetValueOrDefault()) : DBNull.Value);
		lattitude = model.Longitude;
		obj.Add("@Longitude", lattitude.HasValue ? ((object)lattitude.GetValueOrDefault()) : DBNull.Value);
		Dictionary<string, object> parameters = obj;
		return _dbManager.InsertOrUpdateData(procedureName, CommandType.StoredProcedure, parameters);
	}

	public bool AddOrSyncUserTalents(UserTalentUpdateModel model)
	{
		string procedureName = "USP_ADD_OR_SYNC_USER_TALENTS";
		Dictionary<string, object> parameters = new Dictionary<string, object>
		{
			{ "@UserId", model.UserId },
			{ "@UserBy", model.UserBy },
			{
				"@TalentIds",
				(model.TalentIds != null) ? string.Join(",", model.TalentIds) : string.Empty
			}
		};
		return _dbManager.InsertOrUpdateData(procedureName, CommandType.StoredProcedure, parameters);
	}

	public IEnumerable<UserTalentModel> GetUserTalents(int userId)
	{
		string procedureName = "USP_GET_USER_TALENTS";
		Dictionary<string, object> parameters = new Dictionary<string, object> { { "@UserId", userId } };
		DataTable dt = _dbManager.ReadData(procedureName, CommandType.StoredProcedure, parameters);
		foreach (DataRow row in dt.Rows)
		{
			yield return new UserTalentModel
			{
				IdnUserTalent = row.Field<int>("IDN_USER_TALENT"),
				IdnUser = row.Field<int>("IDN_USER"),
				IdnTalent = row.Field<int>("IDN_TALENT"),
				NamTalent = row.Field<string>("NAM_TALENT"),
				ActiveFlag = row.Field<bool>("CDE_ACTIVE_FLAG")
			};
		}
	}

	public IEnumerable<UserAffiliationModel> GetUserAffiliations(int userId)
	{
		string procedureName = "USP_GET_USER_AFFILIATIONS";
		Dictionary<string, object> parameters = new Dictionary<string, object> { { "@UserId", userId } };
		DataTable dataTable = _dbManager.ReadData(procedureName, CommandType.StoredProcedure, parameters);
		foreach (DataRow row in dataTable.Rows)
		{
			yield return new UserAffiliationModel
			{
				UserAffiliationId = row.Field<int>("UserAffiliationId"),
				UserId = row.Field<int>("UserId"),
				AffiliationId = row.Field<int>("AffiliationId"),
				AffiliationName = row.Field<string>("AffiliationName"),
				ActiveFlag = row.Field<bool>("ActiveFlag")
			};
		}
	}

	public bool AddOrSyncUserAffiliations(UserAffiliationUpdateModel model)
	{
		string procedureName = "USP_ADD_OR_SYNC_USER_AFFILIATIONS";
		Dictionary<string, object> parameters = new Dictionary<string, object>
		{
			{ "@UserId", model.UserId },
			{ "@UserBy", model.UserBy },
			{
				"@AffiliationIds",
				string.Join(",", model.AffiliationIds)
			}
		};
		return _dbManager.InsertOrUpdateData(procedureName, CommandType.StoredProcedure, parameters);
	}

	public IEnumerable<UserVocalTypeModel> GetUserVocalTypes(int userId)
	{
		string procedureName = "USP_GET_USER_VOCAL_TYPES";
		Dictionary<string, object> parameters = new Dictionary<string, object> { { "@UserId", userId } };
		DataTable dataTable = _dbManager.ReadData(procedureName, CommandType.StoredProcedure, parameters);
		foreach (DataRow row in dataTable.Rows)
		{
			yield return new UserVocalTypeModel
			{
				UserVocalTypeId = row.Field<int>("UserVocalTypeId"),
				UserId = row.Field<int>("UserId"),
				VocalTypeId = row.Field<int>("VocalTypeId"),
				VocalTypeName = row.Field<string>("VocalTypeName"),
				ActiveFlag = row.Field<bool>("ActiveFlag")
			};
		}
	}

	public bool AddOrSyncUserVocalTypes(UpdateUserVocalTypesRequest model)
	{
		string procedureName = "USP_ADD_OR_SYNC_USER_VOCAL_TYPES";
		Dictionary<string, object> parameters = new Dictionary<string, object>
		{
			{ "@UserId", model.UserId },
			{ "@UserBy", model.UserBy },
			{
				"@VocalTypeIds",
				string.Join(",", model.VocalTypeIds)
			}
		};
		return _dbManager.InsertOrUpdateData(procedureName, CommandType.StoredProcedure, parameters);
	}

	public IEnumerable<UserMediaModel> GetUserMedia(int userId)
	{
		string procedureName = "USP_GET_USER_MEDIA";
		Dictionary<string, object> parameters = new Dictionary<string, object> { { "@UserId", userId } };
		DataTable dataTable = _dbManager.ReadData(procedureName, CommandType.StoredProcedure, parameters);
		foreach (DataRow row in dataTable.Rows)
		{
			yield return new UserMediaModel
			{
				MediaId = row.Field<int?>("MediaId"),
				UserId = row.Field<int>("UserId"),
				MediaType = row.Field<string>("MediaType"),
				MediaName = row.Field<string>("MediaName"),
				MediaStyle = row.Field<string>("MediaStyle"),
				MediaTitle = row.Field<string>("MediaTitle"),
				DateRecordCreated = row.Field<DateTime?>("DateRecordCreated"),
				CreatedUserId = row.Field<int?>("CreatedUserId"),
				DateRecordUpdated = row.Field<DateTime?>("DateRecordUpdated"),
				ActiveFlag = row.Field<int>("ActiveFlag"),
				PicSort = row.Field<int?>("PicSort")
			};
		}
	}

	public IEnumerable<UserResumeTextDataModel> GetUserResumeTextData(int userId)
	{
		string procedureName = "USP_GET_USER_RESUME_TEXT_DATA_NEW";
		Dictionary<string, object> parameters = new Dictionary<string, object> { { "@UserId", userId } };
		DataTable dataTable = _dbManager.ReadData(procedureName, CommandType.StoredProcedure, parameters);
		foreach (DataRow row in dataTable.Rows)
		{
			yield return new UserResumeTextDataModel
			{
				ResumeTextDataId = row.Field<int>("ResumeTextDataId"),
				UserId = row.Field<int>("UserId"),
				IsInternal = row.Field<byte>("IsInternal"),
				ResumeTextHeading = row.Field<string>("ResumeTextHeading"),
				ResumeTextDetails = row.Field<string>("ResumeTextDetails"),
				SortOrder = row.Field<short?>("SortOrder"),
				CreatedUserId = row.Field<int?>("CreatedUserId"),
				UpdatedUserId = row.Field<int?>("UpdatedUserId"),
				DateRecordCreated = row.Field<DateTime?>("DateRecordCreated"),
				DateRecordUpdated = row.Field<DateTime?>("DateRecordUpdated"),
				ActiveFlag = row.Field<bool>("ActiveFlag")
			};
		}
	}

	public IEnumerable<ResumeSectionWithDetailsModel> GetResumeSectionsWithDetails(int userId)
	{
		string procedureName = "USP_GET_RESUME_SECTIONS_WITH_DETAILS";
		Dictionary<string, object> parameters = new Dictionary<string, object> { { "@UserId", userId } };
		DataTable dataTable = _dbManager.ReadData(procedureName, CommandType.StoredProcedure, parameters);
		foreach (DataRow row in dataTable.Rows)
		{
			yield return new ResumeSectionWithDetailsModel
			{
				ResumeSectionId = row.Field<int>("ResumeSectionId"),
				SectionHeading = row.Field<string>("SectionHeading"),
				SectionSortOrder = row.Field<short?>("SectionSortOrder"),
				ResumeSectionDetailId = row.Field<int?>("ResumeSectionDetailId"),
				Title = row.Field<string>("Title"),
				Role = row.Field<string>("Role"),
				Company = row.Field<string>("Company"),
				Url = row.Field<string>("Url"),
				DetailSortOrder = row.Field<short?>("DetailSortOrder"),
				DetailDateCreated = row.Field<DateTime?>("DetailDateCreated")
			};
		}
	}

	public bool AddOrUpdateResumeTextData(ResumeTextDataModel model)
	{
		string procedureName = "USP_ADD_OR_UPDATE_USER_RESUME_TEXT_DATA";
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		int? resumeTextDataId = model.ResumeTextDataId;
		dictionary.Add("@ResumeTextDataId", resumeTextDataId.HasValue ? ((object)resumeTextDataId.GetValueOrDefault()) : DBNull.Value);
		dictionary.Add("@UserId", model.UserId);
		dictionary.Add("@IsInternal", model.IsInternal);
		dictionary.Add("@ResumeTextHeading", model.ResumeTextHeading ?? string.Empty);
		dictionary.Add("@ResumeTextDetails", model.ResumeTextDetails ?? string.Empty);
		dictionary.Add("@SortOrder", model.SortOrder);
		dictionary.Add("@UserBy", model.UserBy);
		dictionary.Add("@ActiveFlag", model.ActiveFlag);
		Dictionary<string, object> parameters = dictionary;
		return _dbManager.InsertOrUpdateData(procedureName, CommandType.StoredProcedure, parameters);
	}

	public bool AddOrUpdateResumeSection(ResumeSectionRequest model)
	{
		string procedureName = "USP_ADD_OR_UPDATE_RESUME_SECTION";
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		int? resumeSectionId = model.ResumeSectionId;
		dictionary.Add("@ResumeSectionId", resumeSectionId.HasValue ? ((object)resumeSectionId.GetValueOrDefault()) : DBNull.Value);
		dictionary.Add("@UserId", model.UserId);
		dictionary.Add("@UserBy", model.UserBy);
		dictionary.Add("@SectionHeading", ((object)model.SectionHeading) ?? ((object)DBNull.Value));
		dictionary.Add("@SectionSortOrder", model.SectionSortOrder);
		dictionary.Add("@ActiveFlag", model.ActiveFlag);
		Dictionary<string, object> parameters = dictionary;
		return _dbManager.InsertOrUpdateData(procedureName, CommandType.StoredProcedure, parameters);
	}

	public bool AddOrUpdateResumeSectionDetail(ResumeSectionDetailRequest model)
	{
		string procedureName = "USP_ADD_OR_UPDATE_RESUME_SECTION_DETAIL";
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		int? resumeSectionDetailId = model.ResumeSectionDetailId;
		dictionary.Add("@ResumeSectionDetailId", resumeSectionDetailId.HasValue ? ((object)resumeSectionDetailId.GetValueOrDefault()) : DBNull.Value);
		dictionary.Add("@ResumeSectionId", model.ResumeSectionId);
		dictionary.Add("@Title", string.IsNullOrEmpty(model.Title) ? ((IConvertible)DBNull.Value) : ((IConvertible)model.Title));
		dictionary.Add("@Role", string.IsNullOrEmpty(model.Role) ? ((IConvertible)DBNull.Value) : ((IConvertible)model.Role));
		dictionary.Add("@Company", string.IsNullOrEmpty(model.Company) ? ((IConvertible)DBNull.Value) : ((IConvertible)model.Company));
		dictionary.Add("@Url", string.IsNullOrEmpty(model.Url) ? ((IConvertible)DBNull.Value) : ((IConvertible)model.Url));
		dictionary.Add("@UserId", model.UserId);
		dictionary.Add("@SortOrder", model.SortOrder);
		dictionary.Add("@ActiveFlag", model.ActiveFlag);
		Dictionary<string, object> parameters = dictionary;
		return _dbManager.InsertOrUpdateData(procedureName, CommandType.StoredProcedure, parameters);
	}

	public async Task<bool> AddOrUpdateUserMediaAsync(UserMediaRequest model, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (model.UserId <= 0)
		{
			throw new ArgumentException("Invalid User ID", "UserId");
		}
		if (model.MediaFile != null && model.MediaFile.Length > 0)
		{
			if (model.MediaFile.Length > 524288000)
			{
				throw new InvalidOperationException("File size exceeds the 500MB limit.");
			}
			string baseDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Uploads");
			Directory.CreateDirectory(baseDirectory);
			if (model.MediaFile.Length <= 52428800)
			{
				await UploadNormalAsync(model, baseDirectory, cancellationToken);
			}
			else
			{
				await UploadInChunksAsync(model, baseDirectory, cancellationToken);
			}
		}
		string procedureName = "USP_ADD_OR_UPDATE_USER_MEDIA";
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		int? mediaId = model.MediaId;
		dictionary.Add("@MediaId", mediaId.HasValue ? ((object)mediaId.GetValueOrDefault()) : DBNull.Value);
		dictionary.Add("@UserId", model.UserId);
		dictionary.Add("@MediaType", ((object)model.MediaType) ?? ((object)DBNull.Value));
		dictionary.Add("@MediaStyle", ((object)model.MediaStyle) ?? ((object)DBNull.Value));
		dictionary.Add("@MediaTitle", ((object)model.MediaTitle) ?? ((object)DBNull.Value));
		dictionary.Add("@PicSort", model.PicSort);
		dictionary.Add("@ActiveFlag", model.ActiveFlag);
		dictionary.Add("@MediaName", ((object)model.MediaLink) ?? ((object)DBNull.Value));
		dictionary.Add("@UserBy", model.UserBy);
		Dictionary<string, object> parameters = dictionary;
		return _dbManager.InsertOrUpdateData(procedureName, CommandType.StoredProcedure, parameters);
	}

	private async Task UploadNormalAsync(UserMediaRequest model, string baseDirectory, CancellationToken cancellationToken)
	{
		string uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(model.MediaFile.FileName)}";
		string filePath = Path.Combine(baseDirectory, uniqueFileName);
		try
		{
			await using FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);
			await model.MediaFile.CopyToAsync(stream, cancellationToken);
			HttpRequest request = _httpContextAccessor.HttpContext?.Request;
			model.MediaLink = ((request != null) ? $"{request.Scheme}://{request.Host}/uploads/{uniqueFileName}" : ("/uploads/" + uniqueFileName));
		}
		catch (Exception)
		{
			if (System.IO.File.Exists(filePath))
			{
				System.IO.File.Delete(filePath);
			}
			throw;
		}
	}

	private async Task UploadInChunksAsync(UserMediaRequest model, string baseDirectory, CancellationToken cancellationToken)
	{
		string uploadId = Guid.NewGuid().ToString();
		string tempDirectory = Path.Combine(baseDirectory, "temp", uploadId);
		Directory.CreateDirectory(tempDirectory);
		string finalFileName = uploadId + Path.GetExtension(model.MediaFile.FileName);
		string finalPath = Path.Combine(baseDirectory, finalFileName);
		try
		{
			using Stream inputStream = model.MediaFile.OpenReadStream();
			int chunkIndex = 0;
			byte[] buffer = new byte[5242880];
			int bytesRead;
			while ((bytesRead = await inputStream.ReadAsync(buffer, 0, 5242880, cancellationToken)) > 0)
			{
				string chunkPath = Path.Combine(tempDirectory, $"chunk_{chunkIndex}");
				await using FileStream chunkStream = new FileStream(chunkPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);
				await chunkStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
				chunkIndex++;
			}
			await using FileStream finalStream = new FileStream(finalPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);
			for (int i = 0; i < chunkIndex; i++)
			{
				string chunk = Path.Combine(tempDirectory, $"chunk_{i}");
				await using FileStream chunkStream = new FileStream(chunk, FileMode.Open, FileAccess.Read, FileShare.None, 81920, useAsync: true);
				await chunkStream.CopyToAsync(finalStream, cancellationToken);
			}
			HttpRequest request = _httpContextAccessor.HttpContext?.Request;
			model.MediaLink = ((request != null) ? $"{request.Scheme}://{request.Host}/uploads/{finalFileName}" : ("/uploads/" + finalFileName));
		}
		catch (Exception)
		{
			if (System.IO.File.Exists(finalPath))
			{
				System.IO.File.Delete(finalPath);
			}
			throw;
		}
		finally
		{
			if (Directory.Exists(tempDirectory))
			{
				Directory.Delete(tempDirectory, recursive: true);
			}
		}
	}

	public IEnumerable<RepresentationInfoModel> GetRepresentationInformation(int userId, string headingName)
	{
		string procedureName = "USP_GET_REPRESENTATION_INFORMATION";
		Dictionary<string, object> parameters = new Dictionary<string, object>
		{
			{ "@UserId", userId },
			{ "@HeadingName", headingName }
		};
		DataTable dt = _dbManager.ReadData(procedureName, CommandType.StoredProcedure, parameters);
		foreach (DataRow row in dt.Rows)
		{
			yield return new RepresentationInfoModel
			{
				NameofCompany = (row["NameofCompany"] as string),
				RepresentativeContact = (row["RepresentativeContact"] as string),
				RepresentativeName = (row["RepresentativeName"] as string)
			};
		}
	}

	public bool AddOrUpdateRepresentationInformation(RepresentationInfoRequestModel model)
	{
		string procedureName = "USP_ADD_OR_UPDATE_REPRESENTATION_INFORMATION";
		Dictionary<string, object> parameters = new Dictionary<string, object>
		{
			{ "@UserId", model.UserId },
			{
				"@HeadingName",
				model.HeadingName ?? string.Empty
			},
			{
				"@NameofCompany",
				model.NameofCompany ?? string.Empty
			},
			{
				"@RepresentativeContact",
				model.RepresentativeContact ?? string.Empty
			},
			{
				"@RepresentativeName",
				model.RepresentativeName ?? string.Empty
			}
		};
		return _dbManager.InsertOrUpdateData(procedureName, CommandType.StoredProcedure, parameters);
	}

	public bool SaveTalentFavoriteNotice(SaveTalentFavoriteNoticeRequestModel model)
	{
		string procedureName = "USP_SaveTalentFavoriteNotice";
		Dictionary<string, object> parameters = new Dictionary<string, object>
		{
			{ "@TalentId", model.TalentId },
			{ "@NoticeId", model.NoticeId },
			{
				"@IsFavorite",
				((object)model.IsFavorite) ?? DBNull.Value
			}
		};
		return _dbManager.InsertOrUpdateData(procedureName, CommandType.StoredProcedure, parameters);
	}
}

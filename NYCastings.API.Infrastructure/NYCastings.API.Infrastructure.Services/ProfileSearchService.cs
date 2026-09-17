using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NYCasting.Core.Models;
using NYCasting.Infrastructure.DataAccess;
using NYCastings.API.Core.Contracts.ProfileSearchInterface;
using NYCastings.API.Core.Models.HideUnhideTalentModel;
using NYCastings.API.Core.Models.ProfileSearchModel;

namespace NYCastings.API.Infrastructure.Services;

public class ProfileSearchService : BaseApiService, IProfileSearchInterface
{
	private readonly DbManager _dbManager;

	public ProfileSearchService(IOptions<ConnectionString> dbConfig, IConfiguration configuration)
		: base(dbConfig)
	{
		if (dbConfig == null || string.IsNullOrWhiteSpace(dbConfig.Value.NYCasting))
		{
			throw new ArgumentNullException("Connection string for NYCasting is missing.");
		}
		_dbManager = new DbManager(dbConfig.Value.NYCasting);
	}

	public IEnumerable<SearchProfileModel> SearchProfiles(SearchProfileRequest request)
	{
		string procedureName = "USP_SEARCH_PROFILES_DYNAMIC";
		Dictionary<string, object> obj = new Dictionary<string, object> { 
		{
			"@Experience",
			((object)request.Experience) ?? ((object)DBNull.Value)
		} };
		bool? mostRecent = request.MostRecent;
		obj.Add("@MostRecent", mostRecent.HasValue ? ((object)(mostRecent == true)) : DBNull.Value);
		mostRecent = request.MostLikes;
		obj.Add("@MostLikes", mostRecent.HasValue ? ((object)(mostRecent == true)) : DBNull.Value);
		obj.Add("@TypeofTalent", string.IsNullOrEmpty(request.TypeofTalent) ? ((IConvertible)DBNull.Value) : ((IConvertible)request.TypeofTalent));
		obj.Add("@Union", string.IsNullOrEmpty(request.Union) ? ((IConvertible)DBNull.Value) : ((IConvertible)request.Union));
		obj.Add("@SEX", string.IsNullOrEmpty(request.Sex) ? ((IConvertible)DBNull.Value) : ((IConvertible)request.Sex));
		obj.Add("@Ethnicity", string.IsNullOrEmpty(request.Ethnicity) ? ((IConvertible)DBNull.Value) : ((IConvertible)request.Ethnicity));
		int? ageStart = request.AgeStart;
		obj.Add("@AgeStart", ageStart.HasValue ? ((object)ageStart.GetValueOrDefault()) : DBNull.Value);
		ageStart = request.AgeEnd;
		obj.Add("@AgeEnd", ageStart.HasValue ? ((object)ageStart.GetValueOrDefault()) : DBNull.Value);
		obj.Add("@SpecialSkills", string.IsNullOrEmpty(request.SpecialSkills) ? ((IConvertible)DBNull.Value) : ((IConvertible)request.SpecialSkills));
		obj.Add("@TalentLastName", string.IsNullOrEmpty(request.TalentLastName) ? ((IConvertible)DBNull.Value) : ((IConvertible)request.TalentLastName));
		obj.Add("@EyeColor", string.IsNullOrEmpty(request.EyeColor) ? ((IConvertible)DBNull.Value) : ((IConvertible)request.EyeColor));
		obj.Add("@HairColor", string.IsNullOrEmpty(request.HairColor) ? ((IConvertible)DBNull.Value) : ((IConvertible)request.HairColor));
		ageStart = request.HeightStart;
		obj.Add("@HeightStart", ageStart.HasValue ? ((object)ageStart.GetValueOrDefault()) : DBNull.Value);
		ageStart = request.HeightEnd;
		obj.Add("@HeightEnd", ageStart.HasValue ? ((object)ageStart.GetValueOrDefault()) : DBNull.Value);
		ageStart = request.WeightStart;
		obj.Add("@WeightStart", ageStart.HasValue ? ((object)ageStart.GetValueOrDefault()) : DBNull.Value);
		ageStart = request.WeightEnd;
		obj.Add("@WeightEnd", ageStart.HasValue ? ((object)ageStart.GetValueOrDefault()) : DBNull.Value);
		obj.Add("@VocalRange", string.IsNullOrEmpty(request.VocalRange) ? ((IConvertible)DBNull.Value) : ((IConvertible)request.VocalRange));
		obj.Add("@VocalType", string.IsNullOrEmpty(request.VocalType) ? ((IConvertible)DBNull.Value) : ((IConvertible)request.VocalType));
		mostRecent = request.MustHaveVideo;
		obj.Add("@MustHaveaVideo", mostRecent.HasValue ? ((object)(mostRecent == true)) : DBNull.Value);
		mostRecent = request.MustHaveAudio;
		obj.Add("@MustHaveaAudio", mostRecent.HasValue ? ((object)(mostRecent == true)) : DBNull.Value);
		obj.Add("@Location", string.IsNullOrEmpty(request.Location) ? ((IConvertible)DBNull.Value) : ((IConvertible)request.Location));
		obj.Add("@ClientId", request.DirectorId);
		double? distance = request.Distance;
		obj.Add("@Distance", distance.HasValue ? ((object)distance.GetValueOrDefault()) : DBNull.Value);
		Dictionary<string, object> parameters = obj;
		DataTable dataTable = _dbManager.ReadData(procedureName, CommandType.StoredProcedure, parameters);
		foreach (DataRow row in dataTable.Rows)
		{
			yield return new SearchProfileModel
			{
				UserId = ((row["UserId"] != DBNull.Value) ? row.Field<int>("UserId") : 0),
				FirstName = ((row["FirstName"] != DBNull.Value) ? row.Field<string>("FirstName") : string.Empty),
				LastName = ((row["LastName"] != DBNull.Value) ? row.Field<string>("LastName") : string.Empty),
				Union = ((row["Union"] != DBNull.Value) ? row.Field<string>("Union") : string.Empty),
				AgeStart = ((row["AgeStart"] != DBNull.Value) ? Convert.ToInt32(row["AgeStart"]) : 0),
				AgeEnd = ((row["AgeEnd"] != DBNull.Value) ? Convert.ToInt32(row["AgeEnd"]) : 0),
				TalentImage = ((row["TalentImage"] != DBNull.Value) ? row.Field<string>("TalentImage") : string.Empty),
				HasAudio = (row["HasAudio"] != DBNull.Value && row.Field<string>("HasAudio") == "TRUE"),
				HasVideo = (row["HasVideo"] != DBNull.Value && row.Field<string>("HasVideo") == "TRUE"),
				RecordCreatedDate = ((row["RecordCreatedDate"] != DBNull.Value) ? row.Field<DateTime>("RecordCreatedDate") : default(DateTime)),
				MostLiked = ((row["MostLiked"] != DBNull.Value) ? Convert.ToInt32(row["MostLiked"]) : 0),
				Favorite = (row["Favorite"] != DBNull.Value && Convert.ToBoolean(row["Favorite"])),
				TalentEmail = ((row["TalentEmail"] != DBNull.Value) ? row.Field<string>("TalentEmail") : string.Empty),
				Sex = ((row["Sex"] != DBNull.Value) ? row.Field<string>("Sex") : string.Empty),
				Experience = ((row["Experience"] != DBNull.Value) ? row.Field<int>("Experience") : 0)
			};
		}
	}

	public bool HideOrUnhideTalent(HideUnhideTalentRequest request)
	{
		string Procedure = "USP_HideandunHideTalent";
		Dictionary<string, object> parameters = new Dictionary<string, object>
		{
			{ "@ClientId", request.ClientId },
			{ "@TalentId", request.TalentId },
			{ "@Hide", request.Hide }
		};
		_dbManager.InsertOrUpdateData(Procedure, CommandType.StoredProcedure, parameters);
		return true;
	}

	public IEnumerable<SearchProfileModel> GetHidedTalentDetails(int clientId)
	{
		string procedureName = "USP_GetHidedTalents";
		Dictionary<string, object> parameters = new Dictionary<string, object> { { "@ClientId", clientId } };
		DataTable dataTable = _dbManager.ReadData(procedureName, CommandType.StoredProcedure, parameters);
		foreach (DataRow row in dataTable.Rows)
		{
			yield return new SearchProfileModel
			{
				UserId = ((row["UserId"] != DBNull.Value) ? row.Field<int>("UserId") : 0),
				FirstName = ((row["FirstName"] != DBNull.Value) ? row.Field<string>("FirstName") : string.Empty),
				LastName = ((row["LastName"] != DBNull.Value) ? row.Field<string>("LastName") : string.Empty),
				Union = ((row["Union"] != DBNull.Value) ? row.Field<string>("Union") : string.Empty),
				AgeStart = ((row["AgeStart"] != DBNull.Value) ? Convert.ToInt32(row["AgeStart"]) : 0),
				AgeEnd = ((row["AgeEnd"] != DBNull.Value) ? Convert.ToInt32(row["AgeEnd"]) : 0),
				TalentImage = ((row["TalentImage"] != DBNull.Value) ? row.Field<string>("TalentImage") : string.Empty),
				HasAudio = (row["HasAudio"] != DBNull.Value && row.Field<string>("HasAudio") == "TRUE"),
				HasVideo = (row["HasVideo"] != DBNull.Value && row.Field<string>("HasVideo") == "TRUE"),
				RecordCreatedDate = ((row["RecordCreatedDate"] != DBNull.Value) ? row.Field<DateTime>("RecordCreatedDate") : default(DateTime)),
				MostLiked = ((row["MostLiked"] != DBNull.Value) ? Convert.ToInt32(row["MostLiked"]) : 0),
				Favorite = (row["Favorite"] != DBNull.Value && Convert.ToBoolean(row["Favorite"]))
			};
		}
	}
}

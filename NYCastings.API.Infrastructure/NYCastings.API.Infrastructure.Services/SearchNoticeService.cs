using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NYCasting.Core.Models;
using NYCasting.Infrastructure.DataAccess;
using NYCastings.API.Core.Contracts.SearchNoticeInterface;
using NYCastings.API.Core.Models.DirectSubmitHistoryRequest;
using NYCastings.API.Core.Models.Filtered_Casting_Notices;
using NYCastings.API.Core.Models.JobHistoryModel;
using NYCastings.API.Core.Models.LocationModel;
using NYCastings.API.Core.Models.ResumeViewsModel;
using NYCastings.API.Core.Models.SaveCastingSearchModel;
using NYCastings.API.Core.Models.UserEmail;
using NYCastings.API.Core.Models.UserNotifications;

namespace NYCastings.API.Infrastructure.Services;

public class SearchNoticeService : BaseApiService, ISearchNoticeInterface
{
	private readonly DbManager _dbManager;

	private readonly EmailService _emailService;

	private readonly IHttpContextAccessor _httpContextAccessor;

	public SearchNoticeService(IOptions<ConnectionString> dbConfig, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
		: base(dbConfig)
	{
		if (dbConfig == null || string.IsNullOrWhiteSpace(dbConfig.Value.NYCasting))
		{
			throw new ArgumentNullException("Connection string for NYCasting is missing.");
		}
		_dbManager = new DbManager(dbConfig.Value.NYCasting);
		_httpContextAccessor = httpContextAccessor;
		_emailService = new EmailService(dbConfig);
	}

	public IEnumerable<CastingNoticeResponseModel> GetFilteredCastingNotices(CastingNoticeFilterRequest filter)
	{
		string procedureName = "USP_GET_CASTING_NOTICE_FILTERED";
		Dictionary<string, object> parameters = new Dictionary<string, object>
		{
			{ "@UserId", filter.UserId },
			{
				"@Locations",
				((object)JoinList(filter.Locations)) ?? ((object)DBNull.Value)
			},
			{
				"@Ethnicities",
				((object)JoinList(filter.Ethnicities)) ?? ((object)DBNull.Value)
			},
			{
				"@Unions",
				((object)JoinList(filter.Unions)) ?? ((object)DBNull.Value)
			},
			{
				"@JobCategories",
				((object)JoinList(filter.JobCategories)) ?? ((object)DBNull.Value)
			},
			{
				"@PayLevels",
				((object)JoinList(filter.PayLevels)) ?? ((object)DBNull.Value)
			},
			{
				"@Sex",
				((object)JoinList(filter.Sex)) ?? ((object)DBNull.Value)
			},
			{
				"@Title",
				string.IsNullOrWhiteSpace(filter.Title) ? ((IConvertible)DBNull.Value) : ((IConvertible)filter.Title)
			},
			{
				"@MinAge",
				(filter.MinAge > 0) ? ((object)filter.MinAge) : DBNull.Value
			},
			{
				"@MaxAge",
				(filter.MaxAge > 0) ? ((object)filter.MaxAge) : DBNull.Value
			},
			{
				"@SortOrder",
				filter.SortOrder ?? string.Empty
			},
			{
				"@RoleTypes",
				((object)JoinList(filter.RoleTypes)) ?? ((object)DBNull.Value)
			},
			{
				"@RushCall",
				filter.RushCall == true
			},
			{ "@PaymentType", filter.PaymentType },
			{
				"@Favorite",
				filter.Favorite == true
			},
			{
				"@StartDate",
				filter.StartDate.HasValue ? ((object)filter.StartDate.Value) : DBNull.Value
			},
			{
				"@EndDate",
				filter.EndDate.HasValue ? ((object)filter.EndDate.Value) : DBNull.Value
			}
		};
		return (from row in _dbManager.ReadData(procedureName, CommandType.StoredProcedure, parameters).AsEnumerable()
			group row by row.Field<int>("NoticeId")).Select(delegate(IGrouping<int, DataRow> group)
		{
			DataRow row = group.First();
			return new CastingNoticeResponseModel
			{
				NoticeId = row.Field<int>("NoticeId"),
				ProjectType = row.Field<string>("ProjectType"),
				Union = row.Field<string>("Union"),
				NoticeStartDate = row.Field<DateTime?>("NoticeStartDate"),
				NoticeEndDate = row.Field<DateTime?>("NoticeEndDate"),
				NoticeUpdatedDate = row.Field<DateTime?>("NoticeUpdatedDate"),
				ProtectFlag = (row.Field<bool?>("ProtectFlag") == true),
				Title = row.Field<string>("Title"),
				Pay = row.Field<string>("Pay"),
				PaymentType = row.Field<int?>("Rate").GetValueOrDefault(),
				Category = row.Field<string>("Category"),
				LocationCodes = row.Field<string>("LocationCodes"),
				NoticeDescription = row.Field<string>("NoticeDescription"),
				NoticeShortDescription = row.Field<string>("NoticeShortDescription"),
				NoticeSubmittedByEmail = row.Field<string>("NoticeSubmittedByEmail"),
				DirectorName = row.Field<string>("DirectorName"),
				PicturesandDocumentsName = row.Field<string>("PicturesandDocumentsName"),
				PicturesandDocumentsPath = row.Field<string>("PicturesandDocumentsPath"),
				PhotoReferences = row.Field<string>("PhotoReferences"),
				PhotoReferencesPath = row.Field<string>("PhotoReferencesPath"),
				Scripts = row.Field<string>("Scripts"),
				ScriptsPath = row.Field<string>("ScriptsPath"),
				RushCall = row.Field<bool?>("RushCall"),
				Favorite = row.Field<bool?>("Favorite"),
				RoleCount = row.Field<int?>("RoleCount").GetValueOrDefault(),
				Roles = (from r in @group
					group r by r.Field<int>("RoleId")).Select(delegate(IGrouping<int, DataRow> roleGroup)
				{
					DataRow dataRow = roleGroup.First();
					return new RoleModel
					{
						RoleId = dataRow.Field<int>("RoleId"),
						Rolename = dataRow.Field<string>("Rolename"),
						Sex = dataRow.Field<string>("Sex"),
						Ethnicity = dataRow.Field<string>("Ethnicity"),
						RoleUnion = dataRow.Field<string>("RoleUnion"),
						AgeStart = dataRow.Field<int?>("AgeStart"),
						AgeEnd = dataRow.Field<int?>("AgeEnd"),
						RoleType = dataRow.Field<string>("RoleType"),
						RoleDetails = dataRow.Field<string>("RoleDetails"),
						isSubmitted = dataRow.Field<int>("isSubmitted"),
						Payment = dataRow.Field<string>("Payment"),
						PhotoorScript = dataRow.Field<string>("PhotoorScript"),
						PhotoorScriptPath = dataRow.Field<string>("PhotoorScriptPath"),
						ReelRequired = (dataRow.IsNull("ReelRequired") ? ((int?)null) : new int?(dataRow.Field<int>("ReelRequired"))),
						AudioReelRequired = (dataRow.IsNull("AudioReelRequired") ? ((int?)null) : new int?(dataRow.Field<int>("AudioReelRequired"))),
						HeightStart = (dataRow.IsNull("HeightStart") ? ((int?)null) : new int?(dataRow.Field<int>("HeightStart"))),
						HeightEnd = (dataRow.IsNull("HeightEnd") ? ((int?)null) : new int?(dataRow.Field<int>("HeightEnd"))),
						WeightStart = (dataRow.IsNull("WeightStart") ? ((int?)null) : new int?(dataRow.Field<int>("WeightStart"))),
						WeightEnd = (dataRow.IsNull("WeightEnd") ? ((int?)null) : new int?(dataRow.Field<int>("WeightEnd"))),
						EyeColor = dataRow.Field<string>("EyeColor"),
						SuitOne = (dataRow.IsNull("SuitOne") ? ((int?)null) : new int?(dataRow.Field<int>("SuitOne"))),
						SuitTwo = (dataRow.IsNull("SuitTwo") ? ((int?)null) : new int?(dataRow.Field<int>("SuitTwo"))),
						Shirt = dataRow.Field<string>("Shirt"),
						NeckOne = (dataRow.IsNull("NeckOne") ? ((decimal?)null) : new decimal?(dataRow.Field<decimal>("NeckOne"))),
						NeckTwo = (dataRow.IsNull("NeckTwo") ? ((decimal?)null) : new decimal?(dataRow.Field<decimal>("NeckTwo"))),
						SleeveOne = (dataRow.IsNull("SleeveOne") ? ((int?)null) : new int?(dataRow.Field<int>("SleeveOne"))),
						SleeveTwo = (dataRow.IsNull("SleeveTwo") ? ((int?)null) : new int?(dataRow.Field<int>("SleeveTwo"))),
						WaistOne = (dataRow.IsNull("WaistOne") ? ((int?)null) : new int?(dataRow.Field<int>("WaistOne"))),
						WaistTwo = (dataRow.IsNull("WaistTwo") ? ((int?)null) : new int?(dataRow.Field<int>("WaistTwo"))),
						InseamOne = (dataRow.IsNull("InseamOne") ? ((int?)null) : new int?(dataRow.Field<int>("InseamOne"))),
						InseamTwo = (dataRow.IsNull("InseamTwo") ? ((int?)null) : new int?(dataRow.Field<int>("InseamTwo"))),
						ShoeOne = (dataRow.IsNull("ShoeOne") ? ((decimal?)null) : new decimal?(dataRow.Field<decimal>("ShoeOne"))),
						ShoeTwo = (dataRow.IsNull("ShoeTwo") ? ((decimal?)null) : new decimal?(dataRow.Field<decimal>("ShoeTwo"))),
						DressOne = (dataRow.IsNull("DressOne") ? ((int?)null) : new int?(dataRow.Field<int>("DressOne"))),
						DressTwo = (dataRow.IsNull("DressTwo") ? ((int?)null) : new int?(dataRow.Field<int>("DressTwo"))),
						BustOne = (dataRow.IsNull("BustOne") ? ((int?)null) : new int?(dataRow.Field<int>("BustOne"))),
						BustTwo = (dataRow.IsNull("BustTwo") ? ((int?)null) : new int?(dataRow.Field<int>("BustTwo"))),
						Cup = dataRow.Field<string>("Cup"),
						FemalWaistOne = (dataRow.IsNull("FemalWaistOne") ? ((int?)null) : new int?(dataRow.Field<int>("FemalWaistOne"))),
						FemalWaistTwo = (dataRow.IsNull("FemalWaistTwo") ? ((int?)null) : new int?(dataRow.Field<int>("FemalWaistTwo"))),
						HipsOne = (dataRow.IsNull("HipsOne") ? ((int?)null) : new int?(dataRow.Field<int>("HipsOne"))),
						HipsTwo = (dataRow.IsNull("HipsTwo") ? ((int?)null) : new int?(dataRow.Field<int>("HipsTwo"))),
						FemaleShoeOne = (dataRow.IsNull("FemaleShoeOne") ? ((decimal?)null) : new decimal?(dataRow.Field<decimal>("FemaleShoeOne"))),
						FemaleShoeTwo = (dataRow.IsNull("FemaleShoeTwo") ? ((decimal?)null) : new decimal?(dataRow.Field<decimal>("FemaleShoeTwo"))),
						ChildSize = dataRow.Field<string>("ChildSize"),
						ChildShoeOne = (dataRow.IsNull("ChildShoeOne") ? ((decimal?)null) : new decimal?(dataRow.Field<decimal>("ChildShoeOne"))),
						ChildShoeTwo = (dataRow.IsNull("ChildShoeTwo") ? ((decimal?)null) : new decimal?(dataRow.Field<decimal>("ChildShoeTwo"))),
						OtherMeasurements = dataRow.Field<string>("OtherMeasurements"),
						VocalRange = dataRow.Field<string>("VocalRange"),
						VocalStyle = dataRow.Field<string>("VocalStyle"),
						RoleAlert = dataRow.Field<int?>("RoleAlert")
					};
				}).ToList()
			};
		});
		static string JoinList(List<string> list)
		{
			if (list == null || !list.Any((string s) => !string.IsNullOrWhiteSpace(s)))
			{
				return null;
			}
			return string.Join(",", list.Where((string s) => !string.IsNullOrWhiteSpace(s)));
		}
	}

	public IEnumerable<CastingNoticeResponseModel> GetExpiredFilteredCastingNotices(int userId)
	{
		string procedureName = "USP_GET_EXPIRED_CASTING_NOTICE_FILTERED";
		Dictionary<string, object> parameters = new Dictionary<string, object> { { "@UserId", userId } };
		return (from row in _dbManager.ReadData(procedureName, CommandType.StoredProcedure, parameters).AsEnumerable()
			group row by row.Field<int>("NoticeId")).Select(delegate(IGrouping<int, DataRow> group)
		{
			DataRow row = group.First();
			return new CastingNoticeResponseModel
			{
				NoticeId = row.Field<int>("NoticeId"),
				ProjectType = row.Field<string>("ProjectType"),
				Union = row.Field<string>("Union"),
				NoticeStartDate = row.Field<DateTime?>("NoticeStartDate"),
				NoticeEndDate = row.Field<DateTime?>("NoticeEndDate"),
				ProtectFlag = (row.Field<bool?>("ProtectFlag") == true),
				Title = row.Field<string>("Title"),
				Pay = row.Field<string>("Pay"),
				PaymentType = row.Field<int?>("Rate").GetValueOrDefault(),
				Category = row.Field<string>("Category"),
				LocationCodes = row.Field<string>("LocationCodes"),
				NoticeDescription = row.Field<string>("NoticeDescription"),
				NoticeShortDescription = row.Field<string>("NoticeShortDescription"),
				NoticeSubmittedByEmail = row.Field<string>("NoticeSubmittedByEmail"),
				DirectorName = row.Field<string>("DirectorName"),
				PicturesandDocumentsName = row.Field<string>("PicturesandDocumentsName"),
				PicturesandDocumentsPath = row.Field<string>("PicturesandDocumentsPath"),
				PhotoReferences = row.Field<string>("PhotoReferences"),
				PhotoReferencesPath = row.Field<string>("PhotoReferencesPath"),
				Scripts = row.Field<string>("Scripts"),
				ScriptsPath = row.Field<string>("ScriptsPath"),
				RushCall = row.Field<bool?>("RushCall"),
				Favorite = row.Field<bool?>("Favorite"),
				Roles = (from r in @group
					group r by r.Field<int>("RoleId")).Select(delegate(IGrouping<int, DataRow> roleGroup)
				{
					DataRow dataRow = roleGroup.First();
					return new RoleModel
					{
						RoleId = dataRow.Field<int>("RoleId"),
						Rolename = dataRow.Field<string>("Rolename"),
						Sex = dataRow.Field<string>("Sex"),
						Ethnicity = dataRow.Field<string>("Ethnicity"),
						RoleUnion = dataRow.Field<string>("RoleUnion"),
						AgeStart = dataRow.Field<int?>("AgeStart"),
						AgeEnd = dataRow.Field<int?>("AgeEnd"),
						RoleType = dataRow.Field<string>("RoleType"),
						RoleDetails = dataRow.Field<string>("RoleDetails"),
						isSubmitted = dataRow.Field<int>("isSubmitted"),
						Payment = dataRow.Field<string>("Payment"),
						PhotoorScript = dataRow.Field<string>("PhotoorScript"),
						PhotoorScriptPath = dataRow.Field<string>("PhotoorScriptPath"),
						ReelRequired = (dataRow.IsNull("ReelRequired") ? ((int?)null) : new int?(dataRow.Field<int>("ReelRequired"))),
						AudioReelRequired = (dataRow.IsNull("AudioReelRequired") ? ((int?)null) : new int?(dataRow.Field<int>("AudioReelRequired"))),
						HeightStart = (dataRow.IsNull("HeightStart") ? ((int?)null) : new int?(dataRow.Field<int>("HeightStart"))),
						HeightEnd = (dataRow.IsNull("HeightEnd") ? ((int?)null) : new int?(dataRow.Field<int>("HeightEnd"))),
						WeightStart = (dataRow.IsNull("WeightStart") ? ((int?)null) : new int?(dataRow.Field<int>("WeightStart"))),
						WeightEnd = (dataRow.IsNull("WeightEnd") ? ((int?)null) : new int?(dataRow.Field<int>("WeightEnd"))),
						EyeColor = dataRow.Field<string>("EyeColor"),
						SuitOne = (dataRow.IsNull("SuitOne") ? ((int?)null) : new int?(dataRow.Field<int>("SuitOne"))),
						SuitTwo = (dataRow.IsNull("SuitTwo") ? ((int?)null) : new int?(dataRow.Field<int>("SuitTwo"))),
						Shirt = dataRow.Field<string>("Shirt"),
						NeckOne = (dataRow.IsNull("NeckOne") ? ((decimal?)null) : new decimal?(dataRow.Field<decimal>("NeckOne"))),
						NeckTwo = (dataRow.IsNull("NeckTwo") ? ((decimal?)null) : new decimal?(dataRow.Field<decimal>("NeckTwo"))),
						SleeveOne = (dataRow.IsNull("SleeveOne") ? ((int?)null) : new int?(dataRow.Field<int>("SleeveOne"))),
						SleeveTwo = (dataRow.IsNull("SleeveTwo") ? ((int?)null) : new int?(dataRow.Field<int>("SleeveTwo"))),
						WaistOne = (dataRow.IsNull("WaistOne") ? ((int?)null) : new int?(dataRow.Field<int>("WaistOne"))),
						WaistTwo = (dataRow.IsNull("WaistTwo") ? ((int?)null) : new int?(dataRow.Field<int>("WaistTwo"))),
						InseamOne = (dataRow.IsNull("InseamOne") ? ((int?)null) : new int?(dataRow.Field<int>("InseamOne"))),
						InseamTwo = (dataRow.IsNull("InseamTwo") ? ((int?)null) : new int?(dataRow.Field<int>("InseamTwo"))),
						ShoeOne = (dataRow.IsNull("ShoeOne") ? ((decimal?)null) : new decimal?(dataRow.Field<decimal>("ShoeOne"))),
						ShoeTwo = (dataRow.IsNull("ShoeTwo") ? ((decimal?)null) : new decimal?(dataRow.Field<decimal>("ShoeTwo"))),
						DressOne = (dataRow.IsNull("DressOne") ? ((int?)null) : new int?(dataRow.Field<int>("DressOne"))),
						DressTwo = (dataRow.IsNull("DressTwo") ? ((int?)null) : new int?(dataRow.Field<int>("DressTwo"))),
						BustOne = (dataRow.IsNull("BustOne") ? ((int?)null) : new int?(dataRow.Field<int>("BustOne"))),
						BustTwo = (dataRow.IsNull("BustTwo") ? ((int?)null) : new int?(dataRow.Field<int>("BustTwo"))),
						Cup = dataRow.Field<string>("Cup"),
						FemalWaistOne = (dataRow.IsNull("FemalWaistOne") ? ((int?)null) : new int?(dataRow.Field<int>("FemalWaistOne"))),
						FemalWaistTwo = (dataRow.IsNull("FemalWaistTwo") ? ((int?)null) : new int?(dataRow.Field<int>("FemalWaistTwo"))),
						HipsOne = (dataRow.IsNull("HipsOne") ? ((int?)null) : new int?(dataRow.Field<int>("HipsOne"))),
						HipsTwo = (dataRow.IsNull("HipsTwo") ? ((int?)null) : new int?(dataRow.Field<int>("HipsTwo"))),
						FemaleShoeOne = (dataRow.IsNull("FemaleShoeOne") ? ((decimal?)null) : new decimal?(dataRow.Field<decimal>("FemaleShoeOne"))),
						FemaleShoeTwo = (dataRow.IsNull("FemaleShoeTwo") ? ((decimal?)null) : new decimal?(dataRow.Field<decimal>("FemaleShoeTwo"))),
						ChildSize = dataRow.Field<string>("ChildSize"),
						ChildShoeOne = (dataRow.IsNull("ChildShoeOne") ? ((decimal?)null) : new decimal?(dataRow.Field<decimal>("ChildShoeOne"))),
						ChildShoeTwo = (dataRow.IsNull("ChildShoeTwo") ? ((decimal?)null) : new decimal?(dataRow.Field<decimal>("ChildShoeTwo"))),
						OtherMeasurements = dataRow.Field<string>("OtherMeasurements"),
						VocalRange = dataRow.Field<string>("VocalRange"),
						VocalStyle = dataRow.Field<string>("VocalStyle")
					};
				}).ToList()
			};
		});
	}

	public IEnumerable<LocationModel> GetLocationsByStateAbbreviations(string stateAbbrCsv)
	{
		string procedureName = "USP_GET_LOCATION_BY_STATE_ABBR_LIST";
		Dictionary<string, object> parameters = new Dictionary<string, object> { { "@StateAbbrList", stateAbbrCsv } };
		DataTable dt = _dbManager.ReadData(procedureName, CommandType.StoredProcedure, parameters);
		foreach (DataRow row in dt.Rows)
		{
			yield return new LocationModel
			{
				LocationCity = row.Field<string>("LocationCity"),
				LocationCode = row.Field<string>("LocationCode"),
				LocationState = row.Field<string>("LocationState"),
				LocationStateAbbrevation = row.Field<string>("LocationStateAbbrevation")
			};
		}
	}

	public async Task<string> SubmitDirectHistory(DirectSubmitHistoryRequest model)
	{
		if (model.MediaFile != null)
		{
			string baseDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Uploads");
			if (!Directory.Exists(baseDirectory))
			{
				Directory.CreateDirectory(baseDirectory);
			}
			string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.MediaFile.FileName);
			string filePath = Path.Combine(baseDirectory, uniqueFileName);
			using (FileStream stream = new FileStream(filePath, FileMode.Create))
			{
				await model.MediaFile.CopyToAsync(stream);
			}
			HttpRequest request = _httpContextAccessor.HttpContext?.Request;
			if (request != null)
			{
				string baseUrl = $"{request.Scheme}://{request.Host}/uploads";
				model.MediaURL = baseUrl + "/" + uniqueFileName;
			}
			else
			{
				model.MediaURL = "/uploads/" + uniqueFileName;
			}
		}
		string procedureName = "USP_INSERT_DIRECT_SUBMIT_HISTORY";
		Dictionary<string, object> parameters = new Dictionary<string, object>
		{
			{ "@UserID", model.UserID },
			{ "@Type", model.Type },
			{ "@NoticeID", model.NoticeID },
			{
				"@From",
				model.From ?? string.Empty
			},
			{
				"@To",
				string.IsNullOrEmpty(model.To) ? ((IConvertible)DBNull.Value) : ((IConvertible)model.To)
			},
			{
				"@NoticeTitle",
				model.NoticeTitle ?? string.Empty
			},
			{
				"@CoverMsg",
				model.CoverMsg ?? string.Empty
			},
			{ "@HtmlFormat", model.HtmlFormat },
			{ "@RoleId", model.RoleId },
			{
				"@MediaURL",
				model.MediaURL ?? string.Empty
			},
			{
				"@VideoPath",
				string.IsNullOrEmpty(model.VideoPath) ? ((IConvertible)DBNull.Value) : ((IConvertible)model.VideoPath)
			},
			{
				"@ImageURL",
				model.ImageURL ?? string.Empty
			}
		};
		DataTable dt = _dbManager.ReadData(procedureName, CommandType.StoredProcedure, parameters);
		if (dt.Rows.Count > 0)
		{
			string? result = dt.Rows[0]["Message"].ToString() ?? string.Empty;
			if (Convert.ToInt32(dt.Rows[0]["sendMail"]) == 1)
			{
				NotifyTalentOnSuccessfulSubmission(model.TalentName ?? string.Empty, model.From ?? string.Empty);
				NotifyDirectorOnTalentSubmission(model.DirectorName ?? string.Empty, model.To ?? string.Empty, model.SubmissionLink ?? string.Empty, model.NoticeTitle ?? string.Empty);
			}
			return result;
		}
		return "Unknown response from server.";
	}

	public bool NotifyDirectorOnTalentSubmission(string directorFirstName, string directorEmail, string submissionLink, string noticeTitle)
	{
		string subject = "You've Received a New Submission for Your Casting Notice!";
		string body = $"<!DOCTYPE html>\r\n            <html>\r\n            <head>\r\n              <meta charset='UTF-8'/>\r\n              <meta name='viewport' content='width=device-width, initial-scale=1.0'/>\r\n            </head>\r\n            <body style='margin:0;padding:0;background:#f4f4f4;font-family:Arial,sans-serif;font-size:14px;color:#333333;'>\r\n\r\n            <table width='100%' cellpadding='0' cellspacing='0' border='0' style='background:#f4f4f4;'>\r\n            <tr><td align='center' style='padding:20px 0;'>\r\n\r\n              <table width='600' cellpadding='0' cellspacing='0' border='0'\r\n                     style='background:#ffffff;border:1px solid #dddddd;'>\r\n\r\n                <!-- HEADER — Logo image -->\r\n                <tr>\r\n                  <td style='background:#333333;padding:0;text-align:center;'>\r\n                    <img src='https://files.constantcontact.com/82886fe2301/302cc6b2-5b9a-4673-8b39-143010d6b262.jpg?rdr=true'\r\n                         alt='DirectSubmit' width='600'\r\n                         style='display:block;width:100%;max-width:600px;border:0;'/>\r\n                  </td>\r\n                </tr>\r\n\r\n                <!-- BLUE BANNER -->\r\n                <tr>\r\n                  <td style='background:#789eeb;padding:14px 30px;'>\r\n                    <span style='color:#ffffff;font-size:18px;font-weight:bold;'>\r\n                      You've Received a New Submission!\r\n                    </span>\r\n                  </td>\r\n                </tr>\r\n\r\n                <!-- BODY — plain paragraphs only -->\r\n                <tr>\r\n                  <td style='background:#ffffff;padding:28px 30px;\r\n                             font-family:Arial,sans-serif;font-size:14px;\r\n                             color:#333333;line-height:1.6;'>\r\n\r\n                    <p style='margin:0 0 16px;'>Hi {directorFirstName},</p>\r\n\r\n                    <p style='margin:0 0 16px;'>\r\n                      Great news! Talent has just applied to your casting notice\r\n                      <strong>{noticeTitle}</strong> on <strong>NYCastings.com</strong>.\r\n                    </p>\r\n\r\n                    <p style='margin:0 0 16px;'>\r\n                      You can review their profile, headshots, r&eacute;sum&eacute;,\r\n                      and reel to see if they&apos;re a fit for your project.\r\n                    </p>\r\n\r\n                    <p style='margin:0 0 24px;'>\r\n                      <a href='{submissionLink}'\r\n                         style='background:#789eeb;color:#ffffff;\r\n                                padding:12px 28px;text-decoration:none;\r\n                                border-radius:4px;font-size:14px;\r\n                                font-weight:bold;display:inline-block;'>\r\n                        View Submission Details\r\n                      </a>\r\n                    </p>\r\n\r\n                    <p style='margin:0 0 16px;'>\r\n                      Thank you for using NYCastings. We&apos;re excited to help\r\n                      you find the perfect talent for your project!\r\n                    </p>\r\n\r\n                    <p style='margin:0;'>\r\n                      Best,<br/>\r\n                      The DirectSubmit Team\r\n                    </p>\r\n\r\n                  </td>\r\n                </tr>\r\n\r\n                <!-- DIVIDER -->\r\n                <tr>\r\n                  <td style='padding:0 30px;background:#ffffff;'>\r\n                    <hr style='border:none;border-top:1px solid #dddddd;margin:0;'/>\r\n                  </td>\r\n                </tr>\r\n\r\n                <!-- FOOTER -->\r\n                <tr>\r\n                  <td style='background:#ffffff;padding:14px 20px;text-align:center;\r\n                             font-family:Arial,sans-serif;font-size:12px;color:#888888;line-height:1.6;'>\r\n                    <p style='margin:0 0 4px;'>\r\n                      By using the DirectSubmit platform, you agree to the terms and conditions of our\r\n                      <a href='https://directsubmit.nycastings.com/terms-and-condition'\r\n                         target='_blank' style='color:#789eeb;text-decoration:underline;'>Terms of Service</a>\r\n                      and\r\n                      <a href='https://directsubmit.nycastings.com/privacy-policy'\r\n                         target='_blank' style='color:#789eeb;text-decoration:underline;'>Privacy Policy</a>.\r\n                      &copy; DirectSubmit\r\n                    </p>\r\n                    <p style='margin:0;text-align:center;background:#333333;padding:10px;font-size:13px;color:#ffffff;'>\r\n                      <a href='https://directsubmit.nycastings.com'\r\n                         style='color:#789eeb;text-decoration:none;'>DirectSubmit.com</a>\r\n                      &nbsp;|&nbsp; 1480 Vine St. &nbsp;|&nbsp; Los Angeles, CA 90028\r\n                    </p>\r\n                  </td>\r\n                </tr>\r\n\r\n              </table>\r\n\r\n            </td></tr>\r\n            </table>\r\n            </body>\r\n            </html>";
		return _emailService.SendEmailForNoticeSubmit(directorEmail, subject, body);
	}

	public bool NotifyTalentOnSuccessfulSubmission(string talentFirstName, string talentEmail)
	{
		string subject = "Your Submission Was Successfully Sent!";
		string body = "<!DOCTYPE html>\r\n                <html>\r\n                <head>\r\n                  <meta charset='UTF-8'/>\r\n                  <meta name='viewport' content='width=device-width, initial-scale=1.0'/>\r\n                </head>\r\n                <body style='margin:0;padding:0;background:#f4f4f4;font-family:Arial,sans-serif;font-size:14px;color:#333333;'>\r\n\r\n                <table width='100%' cellpadding='0' cellspacing='0' border='0' style='background:#f4f4f4;'>\r\n                <tr><td align='center' style='padding:20px 0;'>\r\n\r\n                  <table width='600' cellpadding='0' cellspacing='0' border='0'\r\n                         style='background:#ffffff;border:1px solid #dddddd;'>\r\n\r\n                    <!-- HEADER — Logo image -->\r\n                    <tr>\r\n                      <td style='background:#333333;padding:0;text-align:center;'>\r\n                        <img src='https://files.constantcontact.com/82886fe2301/302cc6b2-5b9a-4673-8b39-143010d6b262.jpg?rdr=true'\r\n                             alt='DirectSubmit' width='600'\r\n                             style='display:block;width:100%;max-width:600px;border:0;'/>\r\n                      </td>\r\n                    </tr>\r\n\r\n                    <!-- BLUE BANNER -->\r\n                    <tr>\r\n                      <td style='background:#789eeb;padding:14px 30px;'>\r\n                        <span style='color:#ffffff;font-size:18px;font-weight:bold;'>\r\n                          Your Submission Was Successfully Sent!\r\n                        </span>\r\n                      </td>\r\n                    </tr>\r\n\r\n                    <!-- BODY — plain paragraphs only -->\r\n                    <tr>\r\n                      <td style='background:#ffffff;padding:28px 30px;\r\n                                 font-family:Arial,sans-serif;font-size:14px;\r\n                                 color:#333333;line-height:1.6;'>\r\n\r\n                        <p style='margin:0 0 16px;'>Hi " + talentFirstName + ",</p>\r\n\r\n                        <p style='margin:0 0 16px;'>\r\n                          Congratulations! Your submission for the casting notice on\r\n                          <strong>NYCastings.com</strong> has been successfully sent.\r\n                        </p>\r\n\r\n                        <p style='margin:0 0 16px;'>\r\n                          The casting director will review your profile, headshots,\r\n                          r&eacute;sum&eacute;, and any additional materials you&apos;ve provided.\r\n                        </p>\r\n\r\n                        <p style='margin:0 0 16px;'>\r\n                          Stay tuned — if there&apos;s a match, the director will reach\r\n                          out directly with the next steps.\r\n                        </p>\r\n\r\n                        <p style='margin:0 0 24px;'>\r\n                          Thank you for choosing NYCastings, and best of luck!\r\n                        </p>\r\n\r\n                        <p style='margin:0 0 24px;'>\r\n                          <a href='https://directsubmit.nycastings.com/casting-calls'\r\n                             style='background:#789eeb;color:#ffffff;\r\n                                    padding:12px 28px;text-decoration:none;\r\n                                    border-radius:4px;font-size:14px;\r\n                                    font-weight:bold;display:inline-block;'>\r\n                            Browse More Casting Notices\r\n                          </a>\r\n                        </p>\r\n\r\n                        <p style='margin:0;'>\r\n                          Warm regards,<br/>\r\n                          The DirectSubmit Team\r\n                        </p>\r\n\r\n                      </td>\r\n                    </tr>\r\n\r\n                    <!-- DIVIDER -->\r\n                    <tr>\r\n                      <td style='padding:0 30px;background:#ffffff;'>\r\n                        <hr style='border:none;border-top:1px solid #dddddd;margin:0;'/>\r\n                      </td>\r\n                    </tr>\r\n\r\n                    <!-- FOOTER -->\r\n                             <tr>\r\n                  <td style='background:#ffffff;padding:14px 20px;text-align:center;\r\n                             font-family:Arial,sans-serif;font-size:12px;color:#888888;line-height:1.6;'>\r\n                    <p style='margin:0 0 4px;'>\r\n                      By using the DirectSubmit platform, you agree to the terms and conditions of our\r\n                      <a href='https://directsubmit.nycastings.com/terms-and-condition'\r\n                         target='_blank' style='color:#789eeb;text-decoration:underline;'>Terms of Service</a>\r\n                      and\r\n                      <a href='https://directsubmit.nycastings.com/privacy-policy'\r\n                         target='_blank' style='color:#789eeb;text-decoration:underline;'>Privacy Policy</a>.\r\n                      &copy; DirectSubmit\r\n                    </p>\r\n                    <p style='margin:0;text-align:center;background:#333333;padding:10px;font-size:13px;color:#ffffff;'>\r\n                      <a href='https://directsubmit.nycastings.com'\r\n                         style='color:#789eeb;text-decoration:none;'>DirectSubmit.com</a>\r\n                      &nbsp;|&nbsp; 1480 Vine St. &nbsp;|&nbsp; Los Angeles, CA 90028\r\n                    </p>\r\n                  </td>\r\n                </tr>\r\n\r\n                  </table>\r\n\r\n                </td></tr>\r\n                </table>\r\n                </body>\r\n                </html>";
		return _emailService.SendEmailForNoticeSubmit(talentEmail, subject, body);
	}

	public IEnumerable<JobHistoryModel> GetUserJobHistory(int userId)
	{
		string procedureName = "USP_GET_USER_JOB_HISTORY";
		Dictionary<string, object> parameters = new Dictionary<string, object> { { "@UserId", userId } };
		DataTable dt = _dbManager.ReadData(procedureName, CommandType.StoredProcedure, parameters);
		foreach (DataRow row in dt.Rows)
		{
			yield return new JobHistoryModel
			{
				UserId = row.Field<int?>("UserId").GetValueOrDefault(),
				CoverMessage = row.Field<string>("CoverMessage"),
				DateRecordSubmitted = (row.Field<DateTime?>("DateRecordSubmitted") ?? DateTime.MinValue),
				VideoPath = row.Field<string>("VideoPath"),
				MediaURL = row.Field<string>("MediaURL"),
				NoticeId = row.Field<int?>("NoticeId").GetValueOrDefault(),
				ProjectType = row.Field<string>("ProjectType"),
				Union = row.Field<string>("Union"),
				NoticeStartDate = (row.Field<DateTime?>("NoticeStartDate") ?? DateTime.MinValue),
				NoticeEndDate = row.Field<DateTime?>("NoticeEndDate"),
				ProtectFlag = (row.Field<bool?>("ProtectFlag") == true),
				Title = row.Field<string>("Title"),
				Pay = row.Field<string>("Pay"),
				Rate = row.Field<int?>("Rate").GetValueOrDefault(),
				Category = row.Field<string>("Category"),
				LocationCodes = row.Field<string>("LocationCodes"),
				NoticeDescription = row.Field<string>("NoticeDescription"),
				NoticeShortDescription = row.Field<string>("NoticeShortDescription"),
				RoleId = row.Field<int?>("RoleId").GetValueOrDefault(),
				RoleName = row.Field<string>("Rolename"),
				Sex = row.Field<string>("Sex"),
				Ethnicity = row.Field<string>("Ethnicity"),
				RoleUnion = row.Field<string>("RoleUnion"),
				AgeStart = row.Field<int?>("AgeStart").GetValueOrDefault(),
				AgeEnd = row.Field<int?>("AgeEnd").GetValueOrDefault(),
				RoleType = row.Field<string>("RoleType"),
				RoleDetails = row.Field<string>("RoleDetails"),
				ImageURL = row.Field<string>("ImageURL")
			};
		}
	}

	public ResumeMessageResponse GetResumeMessagesWithStatusCounts(int userId)
	{
		string procedureName = "USP_GetUserResumeViewsWithStatusCounts";
		Dictionary<string, object> parameters = new Dictionary<string, object> { { "@UserId", userId } };
		DataSet ds = _dbManager.ReadDataDataSet(procedureName, CommandType.StoredProcedure, parameters);
		ResumeMessageResponse response = new ResumeMessageResponse
		{
			Messages = new List<ResumeMessageModel>(),
			Counts = new ResumeMessageCounts()
		};
		if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
		{
			foreach (DataRow row in ds.Tables[0].Rows)
			{
				response.Messages.Add(new ResumeMessageModel
				{
					MessageId = row.Field<int?>("MessageId").GetValueOrDefault(),
					FromEmail = row.Field<string>("FromEmail"),
					ToEmail = row.Field<string>("ToEmail"),
					Subject = row.Field<string>("Subject"),
					Message = row.Field<string>("Message"),
					Date = (row.Field<DateTime?>("Date") ?? DateTime.MinValue),
					Status = row.Field<string>("Status"),
					NoticeId = row.Field<long?>("NoticeId").GetValueOrDefault(),
					UserId = row.Field<int?>("UserId").GetValueOrDefault(),
					ActiveFlag = row.Field<byte?>("ActiveFlag"),
					PhoneNumber = row.Field<string>("PhoneNumber")
				});
			}
		}
		if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
		{
			DataRow row2 = ds.Tables[1].Rows[0];
			response.Counts = new ResumeMessageCounts
			{
				UnreadCount = row2.Field<int?>("UnreadCount").GetValueOrDefault(),
				ReadCount = row2.Field<int?>("ReadCount").GetValueOrDefault(),
				TotalCount = row2.Field<int?>("TotalCount").GetValueOrDefault()
			};
		}
		return response;
	}

	public bool AddOrUpdateUserResumeView(UserResumeViewModel model)
	{
		string procedureName = "USP_AddOrUpdateUserResumeViews";
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		long? messageId = model.MessageId;
		dictionary.Add("@MessageId", messageId.HasValue ? ((object)messageId.GetValueOrDefault()) : DBNull.Value);
		dictionary.Add("@FromEmail", model.FromEmail ?? string.Empty);
		dictionary.Add("@ToEmail", model.ToEmail ?? string.Empty);
		dictionary.Add("@Subject", model.Subject ?? string.Empty);
		dictionary.Add("@Message", model.Message ?? string.Empty);
		dictionary.Add("@Date", model.Date);
		dictionary.Add("@Status", model.Status ?? string.Empty);
		dictionary.Add("@NoticeId", model.NoticeId);
		dictionary.Add("@UserId", model.UserId);
		dictionary.Add("@ActiveFlag", model.ActiveFlag);
		dictionary.Add("@PhoneNumber", model.PhoneNumber ?? string.Empty);
		Dictionary<string, object> parameters = dictionary;
		return _dbManager.InsertOrUpdateData(procedureName, CommandType.StoredProcedure, parameters);
	}

	public bool AddOrUpdateUserNotificationPrefs(UserNotificationPrefsRequest model)
	{
		string procedureName = "USP_AddOrUpdate_UserNotifPrefs";
		Dictionary<string, object> obj = new Dictionary<string, object>
		{
			{ "@Uid", model.Uid },
			{ "@UserId", model.UserId },
			{
				"@UserEmail",
				((object)model.UserMail) ?? ((object)DBNull.Value)
			},
			{
				"@UserEmail2",
				((object)model.UserMail2) ?? ((object)DBNull.Value)
			},
			{
				"@UserCell",
				((object)model.UserCell) ?? ((object)DBNull.Value)
			},
			{
				"@CellProvider",
				((object)model.CellProvider) ?? ((object)DBNull.Value)
			},
			{
				"@JobCategories",
				((object)model.JobCategories) ?? ((object)DBNull.Value)
			},
			{
				"@UserSex",
				((object)model.UserSex) ?? ((object)DBNull.Value)
			}
		};
		int? ageFrom = model.AgeFrom;
		obj.Add("@MinAge", ageFrom.HasValue ? ((object)ageFrom.GetValueOrDefault()) : DBNull.Value);
		ageFrom = model.AgeTo;
		obj.Add("@MaxAge", ageFrom.HasValue ? ((object)ageFrom.GetValueOrDefault()) : DBNull.Value);
		obj.Add("@UnionStatus", ((object)model.UnionStatus) ?? ((object)DBNull.Value));
		obj.Add("@Ethnicity", ((object)model.Ethnicity) ?? ((object)DBNull.Value));
		ageFrom = model.HeightFrom;
		obj.Add("@MinHeight", ageFrom.HasValue ? ((object)ageFrom.GetValueOrDefault()) : DBNull.Value);
		ageFrom = model.HeightTo;
		obj.Add("@MaxHeight", ageFrom.HasValue ? ((object)ageFrom.GetValueOrDefault()) : DBNull.Value);
		ageFrom = model.MenSuitFrom;
		obj.Add("@MenSuitMin", ageFrom.HasValue ? ((object)ageFrom.GetValueOrDefault()) : DBNull.Value);
		ageFrom = model.MenSuitTo;
		obj.Add("@MenSuitMax", ageFrom.HasValue ? ((object)ageFrom.GetValueOrDefault()) : DBNull.Value);
		obj.Add("@MenShirt", ((object)model.MenShirt) ?? ((object)DBNull.Value));
		decimal? menNeckFrom = model.MenNeckFrom;
		obj.Add("@MenNeckMin", menNeckFrom.HasValue ? ((object)menNeckFrom.GetValueOrDefault()) : DBNull.Value);
		menNeckFrom = model.MenNeckTo;
		obj.Add("@MenNeckMax", menNeckFrom.HasValue ? ((object)menNeckFrom.GetValueOrDefault()) : DBNull.Value);
		ageFrom = model.MenSleeveFrom;
		obj.Add("@MenSleeveMin", ageFrom.HasValue ? ((object)ageFrom.GetValueOrDefault()) : DBNull.Value);
		ageFrom = model.MenSleeveTo;
		obj.Add("@MenSleeveMax", ageFrom.HasValue ? ((object)ageFrom.GetValueOrDefault()) : DBNull.Value);
		ageFrom = model.MenWaistFrom;
		obj.Add("@MenWaistMin", ageFrom.HasValue ? ((object)ageFrom.GetValueOrDefault()) : DBNull.Value);
		ageFrom = model.MenWaistTo;
		obj.Add("@MenWaistMax", ageFrom.HasValue ? ((object)ageFrom.GetValueOrDefault()) : DBNull.Value);
		ageFrom = model.MenInseamFrom;
		obj.Add("@MenInseamMin", ageFrom.HasValue ? ((object)ageFrom.GetValueOrDefault()) : DBNull.Value);
		ageFrom = model.MenInseamTo;
		obj.Add("@MenInseamMax", ageFrom.HasValue ? ((object)ageFrom.GetValueOrDefault()) : DBNull.Value);
		menNeckFrom = model.MenShoeFrom;
		obj.Add("@MenShoeMin", menNeckFrom.HasValue ? ((object)menNeckFrom.GetValueOrDefault()) : DBNull.Value);
		menNeckFrom = model.MenShoeTo;
		obj.Add("@MenShoeMax", menNeckFrom.HasValue ? ((object)menNeckFrom.GetValueOrDefault()) : DBNull.Value);
		ageFrom = model.WomenDressFrom;
		obj.Add("@WomenDressMin", ageFrom.HasValue ? ((object)ageFrom.GetValueOrDefault()) : DBNull.Value);
		ageFrom = model.WomenDressTo;
		obj.Add("@WomenDressMax", ageFrom.HasValue ? ((object)ageFrom.GetValueOrDefault()) : DBNull.Value);
		ageFrom = model.BustFrom;
		obj.Add("@BustMin", ageFrom.HasValue ? ((object)ageFrom.GetValueOrDefault()) : DBNull.Value);
		ageFrom = model.BustTo;
		obj.Add("@BustMax", ageFrom.HasValue ? ((object)ageFrom.GetValueOrDefault()) : DBNull.Value);
		obj.Add("@CupSize", ((object)model.CupSize) ?? ((object)DBNull.Value));
		ageFrom = model.WomenWaistFrom;
		obj.Add("@WomenWaistMin", ageFrom.HasValue ? ((object)ageFrom.GetValueOrDefault()) : DBNull.Value);
		ageFrom = model.WomenWaistTo;
		obj.Add("@WomenWaistMax", ageFrom.HasValue ? ((object)ageFrom.GetValueOrDefault()) : DBNull.Value);
		ageFrom = model.WomenHipsFrom;
		obj.Add("@WomenHipsMin", ageFrom.HasValue ? ((object)ageFrom.GetValueOrDefault()) : DBNull.Value);
		ageFrom = model.WomenHipsTo;
		obj.Add("@WomenHipsMax", ageFrom.HasValue ? ((object)ageFrom.GetValueOrDefault()) : DBNull.Value);
		menNeckFrom = model.WomenShoeFrom;
		obj.Add("@WomenShoeMin", menNeckFrom.HasValue ? ((object)menNeckFrom.GetValueOrDefault()) : DBNull.Value);
		menNeckFrom = model.WomenShoeTo;
		obj.Add("@WomenShoeMax", menNeckFrom.HasValue ? ((object)menNeckFrom.GetValueOrDefault()) : DBNull.Value);
		obj.Add("@ChildSize", ((object)model.ChildSize) ?? ((object)DBNull.Value));
		menNeckFrom = model.ShoeChildFrom;
		obj.Add("@ChildShoeMin", menNeckFrom.HasValue ? ((object)menNeckFrom.GetValueOrDefault()) : DBNull.Value);
		menNeckFrom = model.ShoeChildTo;
		obj.Add("@ChildShoeMax", menNeckFrom.HasValue ? ((object)menNeckFrom.GetValueOrDefault()) : DBNull.Value);
		obj.Add("@RoleType", ((object)model.RoleType) ?? ((object)DBNull.Value));
		obj.Add("@HairColor", ((object)model.HairColor) ?? ((object)DBNull.Value));
		byte? activeStatus = model.ActiveStatus;
		obj.Add("@ActiveStatus", activeStatus.HasValue ? ((object)activeStatus.GetValueOrDefault()) : DBNull.Value);
		DateTime? dateCreated = model.DateCreated;
		obj.Add("@DateCreated", dateCreated.HasValue ? ((object)dateCreated.GetValueOrDefault()) : DBNull.Value);
		ageFrom = model.WeightFrom;
		obj.Add("@WeightMin", ageFrom.HasValue ? ((object)ageFrom.GetValueOrDefault()) : DBNull.Value);
		ageFrom = model.WeightTo;
		obj.Add("@WeightMax", ageFrom.HasValue ? ((object)ageFrom.GetValueOrDefault()) : DBNull.Value);
		obj.Add("@StrPay", ((object)model.PayStatus) ?? ((object)DBNull.Value));
		ageFrom = model.IpayStatus;
		obj.Add("@IPay", ageFrom.HasValue ? ((object)ageFrom.GetValueOrDefault()) : DBNull.Value);
		bool? activeCell = model.ActiveCell;
		obj.Add("@ActiveCell", activeCell.HasValue ? ((object)(activeCell == true)) : DBNull.Value);
		obj.Add("@CityChoice", ((object)model.CityChoice) ?? ((object)DBNull.Value));
		Dictionary<string, object> parameters = obj;
		bool num = _dbManager.InsertOrUpdateData(procedureName, CommandType.StoredProcedure, parameters);
		if (num && !string.IsNullOrEmpty(model.UserMail))
		{
			(string, string) displayValues = GetDisplayValues(model.CityChoice, model.PayStatus);
			NotifyUserOnPreferencesSaved(model, displayValues.Item1, displayValues.Item2);
		}
		return num;
	}

	private (string cityDisplay, string payTypeDisplay) GetDisplayValues(string? cityChoice, string? iPay)
	{
		string cityDisplay = cityChoice ?? string.Empty;
		string payTypeDisplay = iPay ?? string.Empty;
		try
		{
			DataTable dt = _dbManager.ReadData("USP_GET_UserNotifPrefs_DisplayValues", CommandType.StoredProcedure, new Dictionary<string, object>
			{
				{
					"@CityChoice",
					((object)cityChoice) ?? ((object)DBNull.Value)
				},
				{
					"@IPay",
					((object)iPay) ?? ((object)DBNull.Value)
				}
			});
			if (dt != null && dt.Rows.Count > 0)
			{
				DataRow row = dt.Rows[0];
				if (dt.Columns.Contains("CityDisplayValue") && row["CityDisplayValue"] != DBNull.Value && !string.IsNullOrWhiteSpace(row["CityDisplayValue"].ToString()))
				{
					cityDisplay = row["CityDisplayValue"].ToString();
				}
				if (dt.Columns.Contains("PayTypeDisplayValue") && row["PayTypeDisplayValue"] != DBNull.Value && !string.IsNullOrWhiteSpace(row["PayTypeDisplayValue"].ToString()))
				{
					payTypeDisplay = row["PayTypeDisplayValue"].ToString();
				}
			}
		}
		catch
		{
		}
		return (cityDisplay: cityDisplay, payTypeDisplay: payTypeDisplay);
	}

	private bool NotifyUserOnPreferencesSaved(UserNotificationPrefsRequest model, string cityDisplayValue, string payTypeDisplayValue)
	{
		try
		{
			string subject = "Your Role Alert Preferences Have Been Saved!";
			string prefRows = BuildPreferenceRows(model, cityDisplayValue, payTypeDisplayValue);
			string body = "<!DOCTYPE html>\r\n            <html>\r\n            <head>\r\n              <meta charset='UTF-8'/>\r\n              <meta name='viewport' content='width=device-width, initial-scale=1.0'/>\r\n            </head>\r\n            <body style='margin:0;padding:0;background:#f4f4f4;font-family:Arial,sans-serif;font-size:14px;color:#333333;'>\r\n\r\n            <table width='100%' cellpadding='0' cellspacing='0' border='0' style='background:#f4f4f4;'>\r\n            <tr><td align='center' style='padding:20px 0;'>\r\n\r\n              <table width='600' cellpadding='0' cellspacing='0' border='0'\r\n                     style='background:#ffffff;border:1px solid #dddddd;'>\r\n\r\n                <!-- HEADER — Logo image -->\r\n                <tr>\r\n                  <td style='background:#333333;padding:0;text-align:center;'>\r\n                    <img src='https://files.constantcontact.com/82886fe2301/302cc6b2-5b9a-4673-8b39-143010d6b262.jpg?rdr=true'\r\n                         alt='DirectSubmit' width='600'\r\n                         style='display:block;width:100%;max-width:600px;border:0;'/>\r\n                  </td>\r\n                </tr>\r\n\r\n                <!-- BLUE BANNER -->\r\n                <tr>\r\n                  <td style='background:#789eeb;padding:14px 30px;'>\r\n                    <span style='color:#ffffff;font-size:18px;font-weight:bold;'>\r\n                      Your Role Alert Preferences Have Been Saved!\r\n                    </span>\r\n                  </td>\r\n                </tr>\r\n\r\n                <!-- BODY -->\r\n                <tr>\r\n                  <td style='background:#ffffff;padding:28px 30px;\r\n                             font-family:Arial,sans-serif;font-size:14px;\r\n                             color:#333333;line-height:1.6;'>\r\n\r\n                    <p style='margin:0 0 16px;'>Hi,</p>\r\n\r\n                    <p style='margin:0 0 16px;'>\r\n                      Great news! Your <strong>Role Alert Preferences</strong> have been\r\n                      successfully saved on\r\n                      <a href='https://directsubmit.nycastings.com'\r\n                         target='_blank' style='color:#789eeb;'>DirectSubmit.com</a>.\r\n                    </p>\r\n\r\n                    <p style='margin:0 0 16px;'>\r\n                      We will now notify you when casting notices matching your\r\n                      preferences are posted. Here is a summary of what you saved:\r\n                    </p>\r\n\r\n                    <!-- Preferences Table — table used here because it shows structured data -->\r\n                    <table width='100%' cellpadding='0' cellspacing='0' border='0'\r\n                           style='border-collapse:collapse;margin:0 0 24px;'>\r\n                      <tr style='background:#789eeb;'>\r\n                        <th align='left'\r\n                            style='padding:10px 14px;border:1px solid #dddddd;\r\n                                   color:#ffffff;font-weight:bold;width:35%;'>\r\n                          Preference\r\n                        </th>\r\n                        <th align='left'\r\n                            style='padding:10px 14px;border:1px solid #dddddd;\r\n                                   color:#ffffff;font-weight:bold;'>\r\n                          Your Selection\r\n                        </th>\r\n                      </tr>\r\n                      " + prefRows + "\r\n                    </table>\r\n\r\n                    <p style='margin:0 0 16px;'>\r\n                      You can edit or cancel your Role Alerts preferences on the\r\n                      <a href='https://directsubmit.nycastings.com/castingcalls'\r\n                         target='_blank' style='color:#789eeb;'>Casting Calls</a>\r\n                      page under the <strong>Get Role Alerts By Email</strong> section.\r\n                    </p>\r\n\r\n                    <p style='margin:0 0 24px;'>\r\n                      Thank you for using DirectSubmit. We look forward to helping\r\n                      you find your next role!\r\n                    </p>\r\n\r\n                    <p style='margin:0;'>\r\n                      Best,<br/>\r\n                      The DirectSubmit Team\r\n                    </p>\r\n\r\n                  </td>\r\n                </tr>\r\n\r\n                <!-- DIVIDER -->\r\n                <tr>\r\n                  <td style='padding:0 30px;background:#ffffff;'>\r\n                    <hr style='border:none;border-top:1px solid #dddddd;margin:0;'/>\r\n                  </td>\r\n                </tr>\r\n\r\n                <!-- FOOTER -->\r\n                 <tr>\r\n                  <td style='background:#ffffff;padding:14px 20px;text-align:center;\r\n                             font-family:Arial,sans-serif;font-size:12px;color:#888888;line-height:1.6;'>\r\n                    <p style='margin:0 0 4px;'>\r\n                      By using the DirectSubmit platform, you agree to the terms and conditions of our\r\n                      <a href='https://directsubmit.nycastings.com/terms-and-condition'\r\n                         target='_blank' style='color:#789eeb;text-decoration:underline;'>Terms of Service</a>\r\n                      and\r\n                      <a href='https://directsubmit.nycastings.com/privacy-policy'\r\n                         target='_blank' style='color:#789eeb;text-decoration:underline;'>Privacy Policy</a>.\r\n                      &copy; DirectSubmit\r\n                    </p>\r\n                    <p style='margin:0;text-align:center;background:#333333;padding:10px;font-size:13px;color:#ffffff;'>\r\n                      <a href='https://directsubmit.nycastings.com'\r\n                         style='color:#789eeb;text-decoration:none;'>DirectSubmit.com</a>\r\n                      &nbsp;|&nbsp; 1480 Vine St. &nbsp;|&nbsp; Los Angeles, CA 90028\r\n                    </p>\r\n                  </td>\r\n                </tr>\r\n\r\n              </table>\r\n\r\n            </td></tr>\r\n            </table>\r\n            </body>\r\n            </html>";
			return _emailService.SendTalentWelcomeEmail(model.UserMail, subject, body);
		}
		catch
		{
			return false;
		}
	}

	private string BuildPreferenceRows(UserNotificationPrefsRequest model, string cityDisplayValue, string payTypeDisplayValue)
	{
		StringBuilder rows = new StringBuilder();
		int rowIndex = 0;
		AddRow("Email", model.UserMail);
		AddRow("Location", cityDisplayValue);
		AddRow("Job Categories", model.JobCategories);
		AddRow("Ethnicity", model.Ethnicity);
		AddRow("Union Status", model.UnionStatus);
		if (model.AgeFrom.HasValue && model.AgeFrom > 0)
		{
			AddRow("Age From", model.AgeFrom.ToString());
		}
		if (model.AgeTo.HasValue && model.AgeTo > 0)
		{
			AddRow("Age To", model.AgeTo.ToString());
		}
		AddRow("Role Type", model.RoleType);
		AddRow("Gender", model.UserSex);
		AddRow("Payment Type", payTypeDisplayValue);
		if (rowIndex == 0)
		{
			rows.Append("\r\n          <tr style='background-color:#f9f9f9;'>\r\n            <td colspan='2'\r\n                style='padding:9px 14px;border:1px solid #dddddd;\r\n                       text-align:center;color:#888888;'>\r\n              No specific preferences set — you will receive all role alerts.\r\n            </td>\r\n          </tr>");
		}
		return rows.ToString();
		void AddRow(string label, string value)
		{
			if (!string.IsNullOrWhiteSpace(value))
			{
				string displayValue = Regex.Replace(value.Trim(), ",\\s*", ", ");
				string bg = ((rowIndex % 2 == 0) ? "#f9f9f9" : "#ffffff");
				StringBuilder stringBuilder = rows;
				StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(260, 3, stringBuilder);
				handler.AppendLiteral("\r\n          <tr style='background-color:");
				handler.AppendFormatted(bg);
				handler.AppendLiteral(";'>\r\n            <td style='padding:9px 14px;border:1px solid #dddddd;\r\n                       font-weight:bold;width:35%;'>");
				handler.AppendFormatted(label);
				handler.AppendLiteral("</td>\r\n            <td style='padding:9px 14px;border:1px solid #dddddd;'>");
				handler.AppendFormatted(displayValue);
				handler.AppendLiteral("</td>\r\n          </tr>");
				stringBuilder.Append(ref handler);
				rowIndex++;
			}
		}
	}

	public IEnumerable<UserNotifPrefsModel> GetUserNotifPrefsByUserId(int userId)
	{
		string procedureName = "USP_Get_UserNotifPrefs_ByUserId";
		Dictionary<string, object> parameters = new Dictionary<string, object> { { "@UserId", userId } };
		DataTable dt = _dbManager.ReadData(procedureName, CommandType.StoredProcedure, parameters);
		foreach (DataRow row in dt.Rows)
		{
			yield return new UserNotifPrefsModel
			{
				UserId = row.Field<int?>("UserId").GetValueOrDefault(),
				UserEmail = row.Field<string>("UserEmail"),
				UserEmail2 = row.Field<string>("UserEmail2"),
				UserCell = row.Field<string>("UserCell"),
				CellProvider = row.Field<string>("CellProvider"),
				JobCategories = row.Field<string>("JobCategories"),
				UserSex = row.Field<string>("UserSex"),
				MinAge = row.Field<int?>("MinAge").GetValueOrDefault(),
				MaxAge = row.Field<int?>("MaxAge").GetValueOrDefault(),
				UnionStatus = row.Field<string>("UnionStatus"),
				Ethnicity = row.Field<string>("Ethnicity"),
				MinHeight = row.Field<int?>("MinHeight").GetValueOrDefault(),
				MaxHeight = row.Field<int?>("MaxHeight").GetValueOrDefault(),
				MenSuitMin = row.Field<int?>("MenSuitMin").GetValueOrDefault(),
				MenSuitMax = row.Field<int?>("MenSuitMax").GetValueOrDefault(),
				MenShirt = row.Field<string>("MenShirt"),
				MenNeckMin = row.Field<decimal?>("MenNeckMin").GetValueOrDefault(),
				MenNeckMax = row.Field<decimal?>("MenNeckMax").GetValueOrDefault(),
				MenSleeveMin = row.Field<int?>("MenSleeveMin").GetValueOrDefault(),
				MenSleeveMax = row.Field<int?>("MenSleeveMax").GetValueOrDefault(),
				MenWaistMin = row.Field<int?>("MenWaistMin").GetValueOrDefault(),
				MenWaistMax = row.Field<int?>("MenWaistMax").GetValueOrDefault(),
				MenInseamMin = row.Field<int?>("MenInseamMin").GetValueOrDefault(),
				MenInseamMax = row.Field<int?>("MenInseamMax").GetValueOrDefault(),
				MenShoeMin = row.Field<decimal?>("MenShoeMin").GetValueOrDefault(),
				MenShoeMax = row.Field<decimal?>("MenShoeMax").GetValueOrDefault(),
				WomenDressMin = row.Field<int?>("WomenDressMin").GetValueOrDefault(),
				WomenDressMax = row.Field<int?>("WomenDressMax").GetValueOrDefault(),
				BustMin = row.Field<int?>("BustMin").GetValueOrDefault(),
				BustMax = row.Field<int?>("BustMax").GetValueOrDefault(),
				CupSize = row.Field<string>("CupSize"),
				WomenWaistMin = row.Field<int?>("WomenWaistMin").GetValueOrDefault(),
				WomenWaistMax = row.Field<int?>("WomenWaistMax").GetValueOrDefault(),
				WomenHipsMin = row.Field<int?>("WomenHipsMin").GetValueOrDefault(),
				WomenHipsMax = row.Field<int?>("WomenHipsMax").GetValueOrDefault(),
				WomenShoeMin = row.Field<decimal?>("WomenShoeMin").GetValueOrDefault(),
				WomenShoeMax = row.Field<decimal?>("WomenShoeMax").GetValueOrDefault(),
				ChildSize = row.Field<string>("ChildSize"),
				ChildShoeMin = row.Field<decimal?>("ChildShoeMin").GetValueOrDefault(),
				ChildShoeMax = row.Field<decimal?>("ChildShoeMax").GetValueOrDefault(),
				RoleType = row.Field<string>("RoleType"),
				HairColor = row.Field<string>("HairColor"),
				ActiveStatus = row.Field<byte?>("ActiveStatus").GetValueOrDefault(),
				DateCreated = (row.Field<DateTime?>("DateCreated") ?? DateTime.MinValue),
				WeightMin = row.Field<int?>("WeightMin").GetValueOrDefault(),
				WeightMax = row.Field<int?>("WeightMax").GetValueOrDefault(),
				StrPay = row.Field<string>("StrPay"),
				IPay = row.Field<int?>("IPay").GetValueOrDefault(),
				ActiveCell = row.Field<byte?>("ActiveCell").GetValueOrDefault(),
				CityChoice = row.Field<string>("CityChoice")
			};
		}
	}

	public async Task<int> SendCityBasedRoleAlertEmails(RoleAlertEmailRequest request)
	{
		string procedureName = "USP_GetEmailsByCityChoices";
		Dictionary<string, object> parameters = new Dictionary<string, object> { { "@CityChoices", request.CityChoices } };
		DataTable dataTable = _dbManager.ReadData(procedureName, CommandType.StoredProcedure, parameters);
		List<string> emails = new List<string>();
		foreach (DataRow row in dataTable.Rows)
		{
			string email = row["usermail"].ToString();
			if (!string.IsNullOrWhiteSpace(email))
			{
				emails.Add(email);
			}
		}
		if (!emails.Any())
		{
			return 0;
		}
		string subject = request.Subject;
		string EmailDisclaimer = "\r\n                <hr style='margin-top:20px;margin-bottom:10px;' />\r\n                <p style='font-size:11px;color:#888;'>\r\n                    This message was sent to you as part of your subscription to Role Alerts from Direct Submit.<br />\r\n                    You are receiving this email because your preferences match a potential opportunity.<br />\r\n                    To manage your email preferences or unsubscribe, please log in to your profile and visit your notification settings.\r\n                </p>";
		string body = $"\r\n            <p><strong>Role You May Be a Good Match For</strong></p>\r\n            <p><strong>Project Title:</strong> {request.ProjectTitle}</p>\r\n            <p><strong>Reviewed By:</strong> Casting Director, {request.CastingDirectorName}, CSA</p>\r\n            <p><strong>Talent Type:</strong> {request.TalentGender} | <strong>Age Range:</strong> {request.TalentAgeRange}</p>\r\n\r\n            <p>If you choose to submit, please ensure the following:</p>\r\n            <ul>\r\n                <li>Your video reels are functioning properly.</li>\r\n                <li>Your credits, size card, training, and special skills are filled in correctly and free of spelling errors.</li>\r\n                <li>If you do not currently have any video reels uploaded, please record a self-taped monologue and add it to your resume as soon as possible.</li>\r\n            </ul>\r\n\r\n            <p>This opportunity was selected based on your Role Alerts preferences.</p>\r\n\r\n            <p><strong>Available Location(s):</strong></p>\r\n            <p>{request.LocationList}</p>\r\n        " + EmailDisclaimer;
		await _emailService.SendBulkEmailsAsync(emails, subject, body);
		return emails.Count;
	}

	public bool SaveUserCastingSearch(SaveCastingSearchRequest model)
	{
		if (model == null)
		{
			throw new ArgumentNullException("model");
		}
		string procedureName = "USP_SaveUserCastingSearch";
		Dictionary<string, object> parameters = TakeSearchParams(model);
		return _dbManager.InsertOrUpdateData(procedureName, CommandType.StoredProcedure, parameters);
	}

	private Dictionary<string, object> TakeSearchParams(SaveCastingSearchRequest model)
	{
		Dictionary<string, object> obj = new Dictionary<string, object>
		{
			{
				"@Email",
				GetStringOrDbNull(model.Email)
			},
			{
				"@Locations",
				ConvertListToDbValue(model.Locations)
			},
			{
				"@Ethnicities",
				ConvertListToDbValue(model.Ethnicities)
			},
			{
				"@Unions",
				ConvertListToDbValue(model.Unions)
			},
			{
				"@JobCategories",
				ConvertListToDbValue(model.JobCategories)
			},
			{
				"@PayLevels",
				ConvertListToDbValue(model.PayLevels)
			},
			{
				"@Sex",
				ConvertListToDbValue(model.Sex)
			},
			{
				"@RoleTypes",
				ConvertListToDbValue(model.RoleTypes)
			},
			{
				"@Title",
				GetStringOrDbNull(model.Title)
			}
		};
		int? minAge = model.MinAge;
		obj.Add("@MinAge", minAge.HasValue ? ((object)minAge.GetValueOrDefault()) : DBNull.Value);
		minAge = model.MaxAge;
		obj.Add("@MaxAge", minAge.HasValue ? ((object)minAge.GetValueOrDefault()) : DBNull.Value);
		obj.Add("@SortOrder", GetStringOrDbNull(model.SortOrder));
		obj.Add("@RushCall", model.RushCall == true);
		obj.Add("@JobType", ConvertListToDbValue(model.JobType));
		return obj;
	}

	private object GetStringOrDbNull(string? value)
	{
		if (!string.IsNullOrWhiteSpace(value))
		{
			return value.Trim();
		}
		return DBNull.Value;
	}

	private object ConvertListToDbValue(List<string>? list)
	{
		if (list == null || !list.Any())
		{
			return DBNull.Value;
		}
		return string.Join(",", list);
	}

	public SaveCastingSearchRequest GetUserCastingSearch(string email)
	{
		string procedureName = "USP_GetUserCastingSearch";
		Dictionary<string, object> parameters = new Dictionary<string, object> { { "@Email", email } };
		DataTable dt = _dbManager.ReadData(procedureName, CommandType.StoredProcedure, parameters);
		if (dt.Rows.Count == 0)
		{
			return null;
		}
		DataRow row = dt.Rows[0];
		return new SaveCastingSearchRequest
		{
			Email = (row["Email"] as string),
			Locations = Split(row["Locations"]),
			Ethnicities = Split(row["Ethnicities"]),
			Unions = Split(row["Unions"]),
			JobCategories = Split(row["JobCategories"]),
			PayLevels = Split(row["PayLevels"]),
			Sex = Split(row["Sex"]),
			RoleTypes = Split(row["RoleTypes"]),
			Title = (row["Title"] as string),
			MinAge = (row["MinAge"] as int?),
			MaxAge = (row["MaxAge"] as int?),
			SortOrder = (row["SortOrder"] as string),
			RushCall = (row["RushCall"] as bool?)
		};
	}

	private List<string> Split(object value)
	{
		if (value == DBNull.Value || value == null)
		{
			return new List<string>();
		}
		return value.ToString().Split(',').ToList();
	}
}

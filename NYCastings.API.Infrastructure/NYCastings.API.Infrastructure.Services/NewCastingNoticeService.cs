using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NYCasting.Core.Models;
using NYCasting.Infrastructure.DataAccess;
using NYCastings.API.Core.Contracts.NewCastingNoticeInterface;
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
using NYCastings.API.Hubs;

namespace NYCastings.API.Infrastructure.Services;

public class NewCastingNoticeService : BaseApiService, INewCastingNoticeService
{
	private readonly DbManager _dbManager;

	private readonly EmailService _emailService;

	private readonly IHttpContextAccessor _httpContextAccessor;

	private readonly IHubContext<ChatHub> _chatHub;

	public NewCastingNoticeService(IOptions<ConnectionString> dbConfig, IHttpContextAccessor httpContextAccessor, IConfiguration configuration, IHubContext<ChatHub> chatHub)
		: base(dbConfig)
	{
		if (dbConfig == null || string.IsNullOrWhiteSpace(dbConfig.Value.NYCasting))
		{
			throw new ArgumentNullException("Connection string for NYCasting is missing.");
		}
		_dbManager = new DbManager(dbConfig.Value.NYCasting);
		_httpContextAccessor = httpContextAccessor;
		_emailService = new EmailService(dbConfig, configuration);
		_chatHub = chatHub;
	}

	public async Task<int> AddNoticeDetailsAsync(AddNewCastingNoticeDetails noticeDetails)
	{
		string baseDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Uploads");
		if (!Directory.Exists(baseDirectory))
		{
			Directory.CreateDirectory(baseDirectory);
		}
		HttpRequest request = _httpContextAccessor.HttpContext?.Request;
		string baseUrl = ((request != null) ? $"{request.Scheme}://{request.Host}/uploads" : "/uploads");
		if (noticeDetails.PicturesFile != null)
		{
			string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(noticeDetails.PicturesFile.FileName);
			string filePath = Path.Combine(baseDirectory, uniqueFileName);
			using (FileStream stream = new FileStream(filePath, FileMode.Create))
			{
				await noticeDetails.PicturesFile.CopyToAsync(stream);
			}
			noticeDetails.PicturesandDocumentsName = noticeDetails.PicturesFile.FileName;
			noticeDetails.PicturesandDocumentsPath = baseUrl + "/" + uniqueFileName;
		}
		if (noticeDetails.ReferencesFile != null)
		{
			string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(noticeDetails.ReferencesFile.FileName);
			string filePath2 = Path.Combine(baseDirectory, uniqueFileName);
			using (FileStream stream = new FileStream(filePath2, FileMode.Create))
			{
				await noticeDetails.ReferencesFile.CopyToAsync(stream);
			}
			noticeDetails.PhotoReferences = noticeDetails.ReferencesFile.FileName;
			noticeDetails.PhotoReferencesPath = baseUrl + "/" + uniqueFileName;
		}
		if (noticeDetails.ScriptsFile != null)
		{
			string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(noticeDetails.ScriptsFile.FileName);
			string filePath3 = Path.Combine(baseDirectory, uniqueFileName);
			using (FileStream stream = new FileStream(filePath3, FileMode.Create))
			{
				await noticeDetails.ScriptsFile.CopyToAsync(stream);
			}
			noticeDetails.Scripts = noticeDetails.ScriptsFile.FileName;
			noticeDetails.ScriptsPath = baseUrl + "/" + uniqueFileName;
		}
		string procedureName = "USP_ADD_NEW_NOTICE12";
		Dictionary<string, object> parameters = TakeNoticeDataFields(noticeDetails);
		DataTable dataTable = _dbManager.ReadData(procedureName, CommandType.StoredProcedure, parameters);
		if (dataTable != null && dataTable.Rows.Count > 0)
		{
			return Convert.ToInt32(dataTable.Rows[0][0]);
		}
		throw new Exception("Failed to retrieve NoticeId.");
	}

	public async Task<bool> AddOrUpdateRoleWithFile(AddRoleDetails roleDetails)
	{
		if (roleDetails.PhotoorScriptFile != null)
		{
			string baseDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Uploads");
			if (!Directory.Exists(baseDirectory))
			{
				Directory.CreateDirectory(baseDirectory);
			}
			string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(roleDetails.PhotoorScriptFile.FileName);
			string filePath = Path.Combine(baseDirectory, uniqueFileName);
			using (FileStream stream = new FileStream(filePath, FileMode.Create))
			{
				await roleDetails.PhotoorScriptFile.CopyToAsync(stream);
			}
			HttpRequest request = _httpContextAccessor.HttpContext?.Request;
			if (request != null)
			{
				string baseUrl = $"{request.Scheme}://{request.Host}/uploads";
				roleDetails.PhotoorScriptPath = baseUrl + "/" + uniqueFileName;
			}
			else
			{
				roleDetails.PhotoorScriptPath = "/uploads/" + uniqueFileName;
			}
			roleDetails.PhotoorScriptName = roleDetails.PhotoorScriptFile.FileName;
		}
		string procedureName = "USP_ADD_NEW_ROLECREATOR";
		Dictionary<string, object> parameters = TakeRoleDataFields(roleDetails);
		return _dbManager.InsertOrUpdateData(procedureName, CommandType.StoredProcedure, parameters);
	}

	private bool FileNeedsUpdate(string existingFilePath, IFormFile newFile)
	{
		if (!System.IO.File.Exists(existingFilePath))
		{
			return true;
		}
		using (FileStream existingFileStream = System.IO.File.OpenRead(existingFilePath))
		{
			using Stream newFileStream = newFile.OpenReadStream();
			if (existingFileStream.Length != newFileStream.Length)
			{
				return true;
			}
			int existingByte;
			int newByte;
			while ((existingByte = existingFileStream.ReadByte()) != -1 && (newByte = newFileStream.ReadByte()) != -1)
			{
				if (existingByte != newByte)
				{
					return true;
				}
			}
		}
		return false;
	}

	private async Task SaveFileAsync(IFormFile file, string destinationPath)
	{
		using FileStream fileStream = new FileStream(destinationPath, FileMode.Create);
		await file.CopyToAsync(fileStream);
	}

	private string GenerateUniqueFileName(string originalFileName)
	{
		string extension = Path.GetExtension(originalFileName);
		return $"{Guid.NewGuid()}{extension}";
	}

	private static Dictionary<string, object> TakeNoticeDataFields(AddNewCastingNoticeDetails noticeDetails)
	{
		Dictionary<string, object> obj = new Dictionary<string, object>
		{
			{ "@NoticeId", noticeDetails.NoticeId },
			{ "@UserId", noticeDetails.UserId },
			{
				"@ContactName",
				noticeDetails?.ContactName ?? string.Empty
			},
			{
				"@Company",
				noticeDetails?.Company ?? string.Empty
			},
			{
				"@ContactPhone",
				noticeDetails?.ContactPhone ?? string.Empty
			},
			{
				"@ContactEmail",
				noticeDetails?.ContactEmail ?? string.Empty
			},
			{
				"@ProjectTitle",
				noticeDetails?.ProjectTitle ?? string.Empty
			},
			{
				"@ProjectDescription",
				noticeDetails?.ProjectDescription ?? string.Empty
			},
			{
				"@ProjectType",
				noticeDetails?.ProjectType ?? string.Empty
			},
			{
				"@AuditionLocations",
				noticeDetails?.AuditionLocations ?? string.Empty
			},
			{
				"@CastingOrShootDatesInfo",
				noticeDetails?.CastingOrShootDatesInfo ?? string.Empty
			},
			{
				"@PicturesandDocumentsPath",
				noticeDetails?.PicturesandDocumentsPath ?? string.Empty
			},
			{
				"@PhotoReferencesPath",
				noticeDetails?.PhotoReferencesPath ?? string.Empty
			},
			{
				"@ScriptsPath",
				noticeDetails?.ScriptsPath ?? string.Empty
			},
			{
				"@UnionStatus",
				noticeDetails?.UnionStatus ?? string.Empty
			},
			{
				"@PayScale",
				noticeDetails?.PayScale ?? string.Empty
			}
		};
		DateTime? dateTime = noticeDetails?.RemovalDate;
		obj.Add("@RemovalDate", dateTime.HasValue ? ((object)dateTime.GetValueOrDefault()) : DBNull.Value);
		obj.Add("@EmailAddress", noticeDetails?.EmailAddress ?? string.Empty);
		obj.Add("@PicturesandDocumentsName", noticeDetails?.PicturesandDocumentsName ?? string.Empty);
		obj.Add("@PhotoReferences", noticeDetails?.PhotoReferences ?? string.Empty);
		obj.Add("@Scripts", noticeDetails?.Scripts ?? string.Empty);
		obj.Add("@JobCategory", noticeDetails?.JobCategory ?? string.Empty);
		obj.Add("@RushCall", noticeDetails?.RushCall == true);
		obj.Add("@UpdatedUserId", (noticeDetails?.UpdatedUserId).GetValueOrDefault());
		obj.Add("@PaymentType", noticeDetails.PaymentType);
		return obj;
	}

	private static Dictionary<string, object> TakeRoleDataFields(AddRoleDetails roleDetails)
	{
		Dictionary<string, object> obj = new Dictionary<string, object>
		{
			{ "@RoleId", roleDetails.RoleId },
			{ "@NoticeId", roleDetails.NoticeId },
			{
				"@RoleUnion",
				roleDetails?.RoleUnion ?? string.Empty
			},
			{
				"@RoleName",
				roleDetails?.RoleName ?? string.Empty
			},
			{
				"@Sex",
				roleDetails?.Sex ?? string.Empty
			}
		};
		int? num = roleDetails?.AgeStart;
		obj.Add("@AgeStart", num.HasValue ? ((object)num.GetValueOrDefault()) : DBNull.Value);
		num = roleDetails?.AgeEnd;
		obj.Add("@AgeEnd", num.HasValue ? ((object)num.GetValueOrDefault()) : DBNull.Value);
		obj.Add("@Ethnicity", roleDetails?.Ethnicity ?? string.Empty);
		obj.Add("@RoleType", roleDetails?.RoleType ?? string.Empty);
		obj.Add("@Payment", roleDetails?.Payment ?? string.Empty);
		num = roleDetails?.ReelRequired;
		obj.Add("@ReelRequired", num.HasValue ? ((object)num.GetValueOrDefault()) : DBNull.Value);
		num = roleDetails?.AudioReelRequired;
		obj.Add("@AudioReelRequired", num.HasValue ? ((object)num.GetValueOrDefault()) : DBNull.Value);
		num = roleDetails?.HeightStart;
		obj.Add("@HeightStart", num.HasValue ? ((object)num.GetValueOrDefault()) : DBNull.Value);
		num = roleDetails?.HeightEnd;
		obj.Add("@HeightEnd", num.HasValue ? ((object)num.GetValueOrDefault()) : DBNull.Value);
		num = roleDetails?.WeightStart;
		obj.Add("@WeightStart", num.HasValue ? ((object)num.GetValueOrDefault()) : DBNull.Value);
		num = roleDetails?.WeightEnd;
		obj.Add("@WeightEnd", num.HasValue ? ((object)num.GetValueOrDefault()) : DBNull.Value);
		obj.Add("@EyeColor", roleDetails?.EyeColor ?? string.Empty);
		num = roleDetails?.SuitOne;
		obj.Add("@SuitOne", num.HasValue ? ((object)num.GetValueOrDefault()) : DBNull.Value);
		num = roleDetails?.SuitTwo;
		obj.Add("@SuitTwo", num.HasValue ? ((object)num.GetValueOrDefault()) : DBNull.Value);
		obj.Add("@Shirt", roleDetails?.Shirt ?? string.Empty);
		decimal? num2 = roleDetails?.NeckOne;
		obj.Add("@NeckOne", num2.HasValue ? ((object)num2.GetValueOrDefault()) : DBNull.Value);
		num2 = roleDetails?.NeckTwo;
		obj.Add("@NeckTwo", num2.HasValue ? ((object)num2.GetValueOrDefault()) : DBNull.Value);
		num = roleDetails?.SleeveOne;
		obj.Add("@SleeveOne", num.HasValue ? ((object)num.GetValueOrDefault()) : DBNull.Value);
		num = roleDetails?.SleeveTwo;
		obj.Add("@SleeveTwo", num.HasValue ? ((object)num.GetValueOrDefault()) : DBNull.Value);
		num = roleDetails?.WaistOne;
		obj.Add("@WaistOne", num.HasValue ? ((object)num.GetValueOrDefault()) : DBNull.Value);
		num = roleDetails?.WaistTwo;
		obj.Add("@WaistTwo", num.HasValue ? ((object)num.GetValueOrDefault()) : DBNull.Value);
		num = roleDetails?.InseamOne;
		obj.Add("@InseamOne", num.HasValue ? ((object)num.GetValueOrDefault()) : DBNull.Value);
		num = roleDetails?.InseamTwo;
		obj.Add("@InseamTwo", num.HasValue ? ((object)num.GetValueOrDefault()) : DBNull.Value);
		num2 = roleDetails?.ShoeOne;
		obj.Add("@ShoeOne", num2.HasValue ? ((object)num2.GetValueOrDefault()) : DBNull.Value);
		num2 = roleDetails?.ShoeTwo;
		obj.Add("@ShoeTwo", num2.HasValue ? ((object)num2.GetValueOrDefault()) : DBNull.Value);
		num = roleDetails?.DressOne;
		obj.Add("@DressOne", num.HasValue ? ((object)num.GetValueOrDefault()) : DBNull.Value);
		num = roleDetails?.DressTwo;
		obj.Add("@DressTwo", num.HasValue ? ((object)num.GetValueOrDefault()) : DBNull.Value);
		num = roleDetails?.BustOne;
		obj.Add("@BustOne", num.HasValue ? ((object)num.GetValueOrDefault()) : DBNull.Value);
		num = roleDetails?.BustTwo;
		obj.Add("@BustTwo", num.HasValue ? ((object)num.GetValueOrDefault()) : DBNull.Value);
		obj.Add("@Cup", roleDetails?.Cup ?? string.Empty);
		num = roleDetails?.FemalWaistOne;
		obj.Add("@FemalWaistOne", num.HasValue ? ((object)num.GetValueOrDefault()) : DBNull.Value);
		num = roleDetails?.FemalWaistTwo;
		obj.Add("@FemalWaistTwo", num.HasValue ? ((object)num.GetValueOrDefault()) : DBNull.Value);
		num = roleDetails?.HipsOne;
		obj.Add("@HipsOne", num.HasValue ? ((object)num.GetValueOrDefault()) : DBNull.Value);
		num = roleDetails?.HipsTwo;
		obj.Add("@HipsTwo", num.HasValue ? ((object)num.GetValueOrDefault()) : DBNull.Value);
		num2 = roleDetails?.FemaleShoeOne;
		obj.Add("@FemaleShoeOne", num2.HasValue ? ((object)num2.GetValueOrDefault()) : DBNull.Value);
		num2 = roleDetails?.FemaleShoeTwo;
		obj.Add("@FemaleShoeTwo", num2.HasValue ? ((object)num2.GetValueOrDefault()) : DBNull.Value);
		obj.Add("@ChildSize", roleDetails?.ChildSize ?? string.Empty);
		num2 = roleDetails?.ChildShoeOne;
		obj.Add("@ChildShoeOne", num2.HasValue ? ((object)num2.GetValueOrDefault()) : DBNull.Value);
		num2 = roleDetails?.ChildShoeTwo;
		obj.Add("@ChildShoeTwo", num2.HasValue ? ((object)num2.GetValueOrDefault()) : DBNull.Value);
		obj.Add("@OtherMeasurements", roleDetails?.OtherMeasurements ?? string.Empty);
		obj.Add("@VocalRange", roleDetails?.VocalRange ?? string.Empty);
		obj.Add("@VocalStyle", roleDetails?.VocalStyle ?? string.Empty);
		obj.Add("@MoreAboutRole", roleDetails?.MoreAboutRole ?? string.Empty);
		obj.Add("@PhotoorScriptName", roleDetails?.PhotoorScriptName ?? string.Empty);
		obj.Add("@PhotoorScriptPath", roleDetails?.PhotoorScriptPath ?? string.Empty);
		obj.Add("@ProjectId", roleDetails?.ProjectId ?? string.Empty);
		DateTime? dateTime = roleDetails?.RecordCreatedDate;
		obj.Add("@RecordCreatedDate", dateTime.HasValue ? ((object)dateTime.GetValueOrDefault()) : DBNull.Value);
		obj.Add("HairColor", roleDetails?.HairColor ?? string.Empty);
		return obj;
	}

	public IEnumerable<GetNewCastingNoticeDetails> GetNewCastingNoticeDetails(int noticeId, string email)
	{
		string procedureName = "USP_GET_NEW_NOTICE_DETAILS";
		Dictionary<string, object> parameters = new Dictionary<string, object>
		{
			{ "@NoticeId", noticeId },
			{ "@Email", email }
		};
		return (from row in _dbManager.ReadData(procedureName, CommandType.StoredProcedure, parameters).AsEnumerable()
			group row by row.Field<int>("NoticeId")).Select(delegate(IGrouping<int, DataRow> group)
		{
			DataRow row = group.First();
			return new GetNewCastingNoticeDetails
			{
				NoticeId = row.Field<int>("NoticeId"),
				ContactName = row.Field<string>("ContactName"),
				Company = row.Field<string>("Company"),
				ContactPhone = row.Field<string>("ContactPhone"),
				ContactEmail = row.Field<string>("ContactEmail"),
				ProjectTitle = row.Field<string>("ProjectTitle"),
				ProjectDescription = row.Field<string>("ProjectDescription"),
				ProjectType = row.Field<string>("ProjectType"),
				AuditionLocations = row.Field<string>("AuditionLocations"),
				CastingOrShootDatesInfo = row.Field<string>("CastingOrShootDatesInfo"),
				PicturesandDocumentsName = row.Field<string>("PicturesandDocumentsName"),
				PicturesandDocumentsPath = row.Field<string>("PicturesandDocumentsPath"),
				PhotoReferences = row.Field<string>("PhotoReferences"),
				PhotoReferencesPath = row.Field<string>("PhotoReferencesPath"),
				Scripts = row.Field<string>("Scripts"),
				ScriptsPath = row.Field<string>("ScriptsPath"),
				UnionStatus = row.Field<string>("UnionStatus"),
				PayScale = row.Field<string>("PayScale"),
				PaymentType = row.Field<int?>("Rate").GetValueOrDefault(),
				Category = row.Field<string>("Category"),
				NoticeStartDate = row.Field<DateTime?>("NoticeStartDate"),
				NoticeEndDate = row.Field<DateTime?>("NoticeEndDate"),
				EmailAddress = row.Field<string>("EmailAddress"),
				NoticeCreatedDate = row.Field<DateTime?>("NoticeCreatedDate"),
				NoticeUpdatedDate = row.Field<DateTime?>("NoticeUpdatedDate"),
				NoticeStatus = row.Field<string>("NoticeStatus"),
				DirectorName = row.Field<string>("DirectorName"),
				RushCall = (row.Field<bool?>("RushCall") == true),
				CreatedUserId = row.Field<int>("CreatedUserId"),
				UpdatedUserId = row.Field<int>("UpdatedUserId"),
				Favorite = (row.Field<bool?>("Favorite") == true),
				Roles = (from dataRow in @group
					where !dataRow.IsNull("RoleId")
					select new RoleModel
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
						HairColor = dataRow.Field<string>("HairColor"),
						isSubmitted = dataRow.Field<int>("isSubmitted")
					}).ToList()
			};
		});
	}

	public bool DeleteCastingNoticeData(int noticeId)
	{
		string procedureName = "[USP_DELETE_NEW_NOTICE_BY_ID]";
		Dictionary<string, object> parameters = new Dictionary<string, object> { { "@NoticeId", noticeId } };
		return _dbManager.DeleteData(procedureName, CommandType.StoredProcedure, parameters);
	}

	public IEnumerable<GetRoleDetails> GetRoleDetails(int? roleId, int? noticeId, string? roleName, string? sex)
	{
		string procedureName = "USP_GET_NEW_USER_ROLES";
		Dictionary<string, object> parameters = new Dictionary<string, object>
		{
			{ "@RoleId", roleId },
			{ "@NoticeId", noticeId },
			{ "@RoleName", roleName },
			{ "@Sex", sex }
		};
		DataTable dataTable = _dbManager.ReadData(procedureName, CommandType.StoredProcedure, parameters);
		foreach (DataRow row in dataTable.Rows)
		{
			yield return new GetRoleDetails
			{
				RoleId = row.Field<int>("RoleId"),
				NoticeId = row.Field<int>("NoticeId"),
				RoleUnion = row.Field<string>("RoleUnion"),
				RoleName = row.Field<string>("RoleName"),
				Sex = row.Field<string>("Sex"),
				AgeStart = (row.IsNull("AgeStart") ? ((int?)null) : new int?(row.Field<int>("AgeStart"))),
				AgeEnd = (row.IsNull("AgeEnd") ? ((int?)null) : new int?(row.Field<int>("AgeEnd"))),
				Ethnicity = row.Field<string>("Ethnicity"),
				RoleType = row.Field<string>("RoleType"),
				Payment = row.Field<string>("Payment"),
				ReelRequired = (row.IsNull("ReelRequired") ? ((int?)null) : new int?(row.Field<int>("ReelRequired"))),
				AudioReelRequired = (row.IsNull("AudioReelRequired") ? ((int?)null) : new int?(row.Field<int>("AudioReelRequired"))),
				HeightStart = (row.IsNull("HeightStart") ? ((int?)null) : new int?(row.Field<int>("HeightStart"))),
				HeightEnd = (row.IsNull("HeightEnd") ? ((int?)null) : new int?(row.Field<int>("HeightEnd"))),
				WeightStart = (row.IsNull("WeightStart") ? ((int?)null) : new int?(row.Field<int>("WeightStart"))),
				WeightEnd = (row.IsNull("WeightEnd") ? ((int?)null) : new int?(row.Field<int>("WeightEnd"))),
				EyeColor = row.Field<string>("EyeColor"),
				SuitOne = (row.IsNull("SuitOne") ? ((int?)null) : new int?(row.Field<int>("SuitOne"))),
				SuitTwo = (row.IsNull("SuitTwo") ? ((int?)null) : new int?(row.Field<int>("SuitTwo"))),
				Shirt = row.Field<string>("Shirt"),
				NeckOne = (row.IsNull("NeckOne") ? ((decimal?)null) : new decimal?(row.Field<decimal>("NeckOne"))),
				NeckTwo = (row.IsNull("NeckTwo") ? ((decimal?)null) : new decimal?(row.Field<decimal>("NeckTwo"))),
				SleeveOne = (row.IsNull("SleeveOne") ? ((int?)null) : new int?(row.Field<int>("SleeveOne"))),
				SleeveTwo = (row.IsNull("SleeveTwo") ? ((int?)null) : new int?(row.Field<int>("SleeveTwo"))),
				WaistOne = (row.IsNull("WaistOne") ? ((int?)null) : new int?(row.Field<int>("WaistOne"))),
				WaistTwo = (row.IsNull("WaistTwo") ? ((int?)null) : new int?(row.Field<int>("WaistTwo"))),
				InseamOne = (row.IsNull("InseamOne") ? ((int?)null) : new int?(row.Field<int>("InseamOne"))),
				InseamTwo = (row.IsNull("InseamTwo") ? ((int?)null) : new int?(row.Field<int>("InseamTwo"))),
				ShoeOne = (row.IsNull("ShoeOne") ? ((decimal?)null) : new decimal?(row.Field<decimal>("ShoeOne"))),
				ShoeTwo = (row.IsNull("ShoeTwo") ? ((decimal?)null) : new decimal?(row.Field<decimal>("ShoeTwo"))),
				DressOne = (row.IsNull("DressOne") ? ((int?)null) : new int?(row.Field<int>("DressOne"))),
				DressTwo = (row.IsNull("DressTwo") ? ((int?)null) : new int?(row.Field<int>("DressTwo"))),
				BustOne = (row.IsNull("BustOne") ? ((int?)null) : new int?(row.Field<int>("BustOne"))),
				BustTwo = (row.IsNull("BustTwo") ? ((int?)null) : new int?(row.Field<int>("BustTwo"))),
				Cup = row.Field<string>("Cup"),
				FemalWaistOne = (row.IsNull("FemalWaistOne") ? ((int?)null) : new int?(row.Field<int>("FemalWaistOne"))),
				FemalWaistTwo = (row.IsNull("FemalWaistTwo") ? ((int?)null) : new int?(row.Field<int>("FemalWaistTwo"))),
				HipsOne = (row.IsNull("HipsOne") ? ((int?)null) : new int?(row.Field<int>("HipsOne"))),
				HipsTwo = (row.IsNull("HipsTwo") ? ((int?)null) : new int?(row.Field<int>("HipsTwo"))),
				FemaleShoeOne = (row.IsNull("FemaleShoeOne") ? ((decimal?)null) : new decimal?(row.Field<decimal>("FemaleShoeOne"))),
				FemaleShoeTwo = (row.IsNull("FemaleShoeTwo") ? ((decimal?)null) : new decimal?(row.Field<decimal>("FemaleShoeTwo"))),
				ChildSize = row.Field<string>("ChildSize"),
				ChildShoeOne = (row.IsNull("ChildShoeOne") ? ((decimal?)null) : new decimal?(row.Field<decimal>("ChildShoeOne"))),
				ChildShoeTwo = (row.IsNull("ChildShoeTwo") ? ((decimal?)null) : new decimal?(row.Field<decimal>("ChildShoeTwo"))),
				OtherMeasurements = row.Field<string>("OtherMeasurements"),
				VocalRange = row.Field<string>("VocalRange"),
				VocalStyle = row.Field<string>("VocalStyle"),
				MoreAboutRole = row.Field<string>("MoreAboutRole"),
				PhotoorScriptName = row.Field<string>("PhotoorScriptName"),
				PhotoorScriptPath = row.Field<string>("PhotoorScriptPath"),
				ProjectId = row.Field<string>("ProjectId"),
				RecordCreatedDate = (row.IsNull("RecordCreatedDate") ? ((DateTime?)null) : new DateTime?(row.Field<DateTime>("RecordCreatedDate"))),
				ActiveFlag = (row.IsNull("ActiveFlag") ? ((bool?)null) : new bool?(row.Field<bool>("ActiveFlag"))),
				IsModified = (row.IsNull("IsModified") ? ((bool?)null) : new bool?(row.Field<bool>("IsModified"))),
				DateModified = (row.IsNull("DateModified") ? ((DateTime?)null) : new DateTime?(row.Field<DateTime>("DateModified"))),
				ModificationMessage = row.Field<string>("ModificationMessage"),
				HairColor = row.Field<string>("HairColor")
			};
		}
	}

	public bool DeleteRoleData(int roleId)
	{
		string procedureName = "USP_DELETE_NEW_ROLE";
		Dictionary<string, object> parameters = new Dictionary<string, object> { { "@RoleId", roleId } };
		return _dbManager.DeleteData(procedureName, CommandType.StoredProcedure, parameters);
	}

	public bool ApproveNotice(int noticeId, bool approve, int userId)
	{
		string procedureName = "USP_APPROVE_NOTICE12";
		Dictionary<string, object> parameters = new Dictionary<string, object>
		{
			{ "@idn_notice", noticeId },
			{ "@bool_approve", approve },
			{ "@idn_user", userId }
		};
		return _dbManager.DeleteData(procedureName, CommandType.StoredProcedure, parameters);
	}

	public bool NotifyUserOnNoticeSubmission(string userName, string userEmail)
	{
		string subject = "Thank You for Submitting Your Notice!";
		string body = $"<!DOCTYPE html>\r\n                <html>\r\n                <head>\r\n                  <meta charset='UTF-8'/>\r\n                  <meta name='viewport' content='width=device-width, initial-scale=1.0'/>\r\n                </head>\r\n                <body style='margin:0;padding:0;background:#f4f4f4;font-family:Arial,sans-serif;font-size:14px;color:#333333;'>\r\n                <table width='100%' cellpadding='0' cellspacing='0' border='0' style='background:#f4f4f4;'>\r\n                <tr><td align='center' style='padding:20px 0;'>\r\n                  <table width='600' cellpadding='0' cellspacing='0' border='0'\r\n                         style='background:#ffffff;border:1px solid #dddddd;'>\r\n\r\n                    <!-- HEADER: Logo -->\r\n                    <tr>\r\n                      <td style='background:#333333;padding:0;text-align:center;'>\r\n                        <img src='https://files.constantcontact.com/82886fe2301/302cc6b2-5b9a-4673-8b39-143010d6b262.jpg?rdr=true'\r\n                             alt='DirectSubmit' width='600'\r\n                             style='display:block;width:100%;max-width:600px;border:0;'/>\r\n                      </td>\r\n                    </tr>\r\n\r\n                    <!-- BLUE BANNER -->\r\n                    <tr>\r\n                      <td style='background:#789eeb;padding:10px 20px;'>\r\n                        <span style='color:#ffffff;font-size:16px;font-weight:bold;'>Notice Submission Confirmation</span>\r\n                      </td>\r\n                    </tr>\r\n\r\n                    <!-- WHITE BODY -->\r\n                    <tr>\r\n                      <td style='background:#ffffff;padding:20px;font-family:Arial,sans-serif;\r\n                                 font-size:14px;color:#333333;line-height:1.6;'>\r\n\r\n                        <p style='margin:0 0 16px;font-size:16px;font-weight:bold;color:#333333;'>\r\n                          Hello {userName},\r\n                        </p>\r\n\r\n                        <p style='margin:0 0 16px;'>\r\n                          Thank you for submitting your notice! We have received it and will review it shortly.\r\n                        </p>\r\n\r\n                        <p style='margin:0 0 10px;font-weight:bold;'>Here are some key details:</p>\r\n\r\n                        <table width='100%' cellpadding='0' cellspacing='0' border='0'\r\n                               style='background:#f9f9f9;border:1px solid #dddddd;margin:0 0 20px;'>\r\n                          <tr>\r\n                            <td style='padding:12px 14px;font-size:13px;color:#333333;'>\r\n                              <strong>Submission Time:</strong>\r\n                              <span style='color:#789eeb;'>{DateTime.UtcNow:dddd, MMMM dd, yyyy HH:mm} UTC</span>\r\n                            </td>\r\n                          </tr>\r\n                        </table>\r\n\r\n                        <p style='margin:0 0 20px;'>\r\n                          Good luck with your career!<br/>\r\n                          <strong>Your Team @ DirectSubmit</strong><br/>                          \r\n                        </p>\r\n\r\n                        <p style='margin:0 0 20px;'>\r\n                          <a href='https://directsubmit.nycastings.com'\r\n                             style='background:#789eeb;color:#ffffff;padding:11px 24px;\r\n                                    text-decoration:none;border-radius:4px;font-size:13px;\r\n                                    font-weight:bold;display:inline-block;'>\r\n                            View DirectSubmit\r\n                          </a>\r\n                        </p>\r\n                      </td>\r\n                    </tr>\r\n\r\n                    <!-- DIVIDER -->\r\n                    <tr>\r\n                      <td style='padding:0 20px;background:#ffffff;'>\r\n                        <hr style='border:none;border-top:1px solid #dddddd;margin:0;'/>\r\n                      </td>\r\n                    </tr>\r\n\r\n                    <!-- FOOTER -->\r\n                    <tr>\r\n                      <td style='background:#ffffff;padding:14px 20px;text-align:center;\r\n                                 font-family:Arial,sans-serif;font-size:12px;color:#888888;line-height:1.6;'>\r\n                        <p style='margin:0 0 4px;'>\r\n                          By using the\r\n                          <a href='https://directsubmit.nycastings.com/login'\r\n                             target='_blank' style='color:#789eeb;text-decoration:underline;'>DirectSubmit</a>\r\n                          messaging system, you agree to the terms and conditions of our\r\n                          <a href='https://directsubmit.nycastings.com/terms-and-condition'\r\n                             target='_blank' style='color:#789eeb;text-decoration:underline;'>Terms of Service</a>\r\n                          and\r\n                          <a href='https://directsubmit.nycastings.com/privacy-policy'\r\n                             target='_blank' style='color:#789eeb;text-decoration:underline;'>Privacy Policy</a>.\r\n                          &copy; DirectSubmit\r\n                        </p>\r\n                        <p style='margin:0;text-align:center;background:#333333;padding:10px;font-size:13px;color:#ffffff;'>\r\n                          <a href='https://directsubmit.nycastings.com'\r\n                             style='color:#789eeb;text-decoration:none;'>DirectSubmit.com</a>\r\n                          &nbsp;|&nbsp; 1480 Vine St. &nbsp;|&nbsp; Los Angeles, CA 90028\r\n                        </p>\r\n                      </td>\r\n                    </tr>\r\n\r\n                  </table>\r\n                </td></tr>\r\n                </table>\r\n                </body>\r\n                </html>";
		return _emailService.SendEmailForNoticeSubmit(userEmail, subject, body);
	}

	public IEnumerable<ProjectSubmissionModel> GetProjectSubmissions(int noticeId)
	{
		if (noticeId <= 0)
		{
			throw new ArgumentException("Invalid Notice ID", "noticeId");
		}
		string procedureName = "USP_GET_PROJECT_SUBMISSIONS";
		Dictionary<string, object> parameters = new Dictionary<string, object> { { "@NoticeId", noticeId } };
		DataTable dataTable = _dbManager.ReadData(procedureName, CommandType.StoredProcedure, parameters);
		foreach (DataRow row in dataTable.Rows)
		{
			yield return new ProjectSubmissionModel
			{
				RoleName = (row["RoleName"] as string),
				AList = ((row["AList"] != DBNull.Value) ? new int?(Convert.ToInt32(row["AList"])) : ((int?)null)),
				AListUserIds = (row["AListUserIds"] as string),
				BList = ((row["BList"] != DBNull.Value) ? new int?(Convert.ToInt32(row["BList"])) : ((int?)null)),
				BListUserIds = (row["BListUserIds"] as string),
				CList = ((row["CList"] != DBNull.Value) ? new int?(Convert.ToInt32(row["CList"])) : ((int?)null)),
				CListUserIds = (row["CListUserIds"] as string),
				XList = ((row["XList"] != DBNull.Value) ? new int?(Convert.ToInt32(row["XList"])) : ((int?)null)),
				XListUserIds = (row["XListUserIds"] as string),
				HRoleId = ((row["HRoleId"] != DBNull.Value) ? new int?(Convert.ToInt32(row["HRoleId"])) : ((int?)null)),
				New = ((row["New"] != DBNull.Value) ? new int?(Convert.ToInt32(row["New"])) : ((int?)null)),
				NewUserIds = (row["NewUserIds"] as string),
				RoleId = ((row["RoleId"] != DBNull.Value) ? new int?(Convert.ToInt32(row["RoleId"])) : ((int?)null))
			};
		}
	}

	public IEnumerable<TalentDetailsModel> GetTalentDetails(string userIds, int roleId, int clientId, string sortBy)
	{
		if (string.IsNullOrWhiteSpace(userIds))
		{
			throw new ArgumentException("User IDs cannot be empty", "userIds");
		}
		string procedureName = "USP_GET_TALENT_DETAILS_FROM_LIST";
		Dictionary<string, object> parameters = new Dictionary<string, object>
		{
			{ "@UserIds", userIds },
			{ "@RoleId", roleId },
			{ "@ClientId", clientId },
			{ "@SortBy", sortBy }
		};
		DataTable dataTable = _dbManager.ReadData(procedureName, CommandType.StoredProcedure, parameters);
		foreach (DataRow row in dataTable.Rows)
		{
			yield return new TalentDetailsModel
			{
				UserId = ((row["UserId"] != DBNull.Value) ? Convert.ToInt32(row["UserId"]) : 0),
				FirstName = (row["FirstName"] as string),
				LastName = (row["LastName"] as string),
				UserEmail = (row["UserEmail"] as string),
				Union = (row["Union"] as string),
				AgeStart = ((row["AgeStart"] != DBNull.Value) ? Convert.ToInt32(row["AgeStart"]) : 0),
				AgeEnd = ((row["AgeEnd"] != DBNull.Value) ? Convert.ToInt32(row["AgeEnd"]) : 0),
				HeightStart = ((row["HeightStart"] != DBNull.Value) ? Convert.ToInt32(row["HeightStart"]) : 0),
				HeightEnd = ((row["HeightEnd"] != DBNull.Value) ? Convert.ToInt32(row["HeightEnd"]) : 0),
				TalentImage = (row["TalentImage"] as string),
				City = (row["City"] as string),
				LocationCode = (row["LocationCode"] as string),
				MostLiked = ((row["MostLiked"] != DBNull.Value) ? new int?(Convert.ToInt32(row["MostLiked"])) : ((int?)null)),
				RoleId = ((row["RoleId"] != DBNull.Value) ? Convert.ToInt32(row["RoleId"]) : 0),
				Favorite = (row["Favorite"] != DBNull.Value && row.Field<string>("Favorite") == "TRUE"),
				CoverLetters = (row["CoverLetters"] as string),
				AdditionalLinks = (row["AdditionalLinks"] as string),
				Media = (row["Media"] as string),
				HasAudio = (row["HasAudio"] != DBNull.Value && row.Field<string>("HasAudio") == "TRUE"),
				HasVideo = (row["HasVideo"] != DBNull.Value && row.Field<string>("HasVideo") == "TRUE"),
				Experience = ((row["Experience"] != DBNull.Value) ? Convert.ToInt32(row["Experience"]) : 0),
				ImageURL = (row["ImageURL"] as string)
			};
		}
	}

	public bool UpdateUserList(UpdateUserListModel updateModel)
	{
		string procedureName = "USP_UPDATE_USER_LIST";
		Dictionary<string, object> parameters = new Dictionary<string, object>
		{
			{ "@UserId", updateModel.UserId },
			{ "@RoleId", updateModel.RoleId },
			{ "@CurrentList", updateModel.CurrentList },
			{ "@TargetList", updateModel.TargetList }
		};
		return _dbManager.InsertOrUpdateData(procedureName, CommandType.StoredProcedure, parameters);
	}

	public bool AddOrUpdateTalentNote(TalentNotes note)
	{
		string procedureName = "USP_ADD_TALENT_NOTE";
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		int? noteId = note.NoteId;
		dictionary.Add("@NoteId", noteId.HasValue ? ((object)noteId.GetValueOrDefault()) : DBNull.Value);
		dictionary.Add("@TalentId", note.TalentId);
		dictionary.Add("@DirectorId", note.DirectorId);
		dictionary.Add("@NoteText", note.NoteText);
		Dictionary<string, object> parameters = dictionary;
		return _dbManager.InsertOrUpdateData(procedureName, CommandType.StoredProcedure, parameters);
	}

	public List<TalentNotes> GetTalentNotes(int? talentId = null, int? directorId = null)
	{
		string procedureName = "USP_GET_TALENT_NOTES";
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		int? num = talentId;
		dictionary.Add("@TalentId", num.HasValue ? ((object)num.GetValueOrDefault()) : DBNull.Value);
		num = directorId;
		dictionary.Add("@DirectorId", num.HasValue ? ((object)num.GetValueOrDefault()) : DBNull.Value);
		Dictionary<string, object> parameters = dictionary;
		DataTable dataTable = _dbManager.ReadData(procedureName, CommandType.StoredProcedure, parameters);
		List<TalentNotes> notes = new List<TalentNotes>();
		foreach (DataRow row in dataTable.Rows)
		{
			notes.Add(new TalentNotes
			{
				NoteId = Convert.ToInt32(row["NoteId"]),
				TalentId = Convert.ToInt32(row["TalentId"]),
				DirectorId = Convert.ToInt32(row["DirectorId"]),
				NoteText = row["NoteText"].ToString(),
				CreatedAt = Convert.ToDateTime(row["CreatedAt"])
			});
		}
		return notes;
	}

	public bool AddOrUpdateClientFave(ClientFaveModel clientFaveModel)
	{
		string procedureName = "USP_AddOrUpdateClientFave";
		Dictionary<string, object> parameters = new Dictionary<string, object>
		{
			{ "@ClientId", clientFaveModel.ClientId },
			{ "@TalentId", clientFaveModel.TalentId },
			{ "@Fave", clientFaveModel.Fave }
		};
		return _dbManager.InsertOrUpdateData(procedureName, CommandType.StoredProcedure, parameters);
	}

	public IEnumerable<ClientFaveTalentModel> GetClientFavTalentDetails(int clientId)
	{
		string procedureName = "USP_GetClientFaveList";
		Dictionary<string, object> parameters = new Dictionary<string, object> { { "@ClientId", clientId } };
		DataTable dataTable = _dbManager.ReadData(procedureName, CommandType.StoredProcedure, parameters);
		foreach (DataRow row in dataTable.Rows)
		{
			yield return new ClientFaveTalentModel
			{
				UserId = ((row["UserId"] != DBNull.Value) ? Convert.ToInt32(row["UserId"]) : 0),
				FirstName = (row["FirstName"] as string),
				LastName = (row["LastName"] as string),
				Union = (row["Union"] as string),
				AgeStart = ((row["AgeStart"] != DBNull.Value) ? Convert.ToInt32(row["AgeStart"]) : 0),
				AgeEnd = ((row["AgeEnd"] != DBNull.Value) ? Convert.ToInt32(row["AgeEnd"]) : 0),
				HeightStart = ((row["HeightStart"] != DBNull.Value) ? Convert.ToInt32(row["HeightStart"]) : 0),
				HeightEnd = ((row["HeightEnd"] != DBNull.Value) ? Convert.ToInt32(row["HeightEnd"]) : 0),
				TalentImage = (row["TalentImage"] as string),
				City = (row["City"] as string),
				LocationCode = (row["LocationCode"] as string),
				MostLiked = ((row["MostLiked"] != DBNull.Value) ? new int?(Convert.ToInt32(row["MostLiked"])) : ((int?)null)),
				FavoratedDate = ((row["FavoratedDate"] != DBNull.Value) ? new DateTime?(Convert.ToDateTime(row["FavoratedDate"])) : ((DateTime?)null)),
				HasAudio = (row["HasAudio"] != DBNull.Value && Convert.ToBoolean(row["HasAudio"])),
				HasVideo = (row["HasVideo"] != DBNull.Value && Convert.ToBoolean(row["HasVideo"]))
			};
		}
	}

	public IEnumerable<InboxMessageDetails> GetInboxMessagesDetails(string email, string sortOrder)
	{
		if (string.IsNullOrEmpty(email))
		{
			throw new ArgumentException("Email is required", "email");
		}
		string procedureName = "USP_GETINBOXMESSAGESDETAILS";
		Dictionary<string, object> parameters = new Dictionary<string, object>
		{
			{ "@Email", email },
			{ "@SortOrder", sortOrder }
		};
		DataTable dataTable = _dbManager.ReadData(procedureName, CommandType.StoredProcedure, parameters);
		foreach (DataRow row in dataTable.Rows)
		{
			yield return new InboxMessageDetails
			{
				NoticeId = ((row["NoticeId"] != DBNull.Value) ? Convert.ToInt32(row["NoticeId"]) : 0),
				NoticeTitle = (row["NoticeTitle"] as string),
				LatestMessageId = ((row["LatestMessageId"] != DBNull.Value) ? Convert.ToInt32(row["LatestMessageId"]) : 0),
				MessageId = Convert.ToInt32(row["MessageId"]),
				ConversationDate = Convert.ToDateTime(row["ConversationDate"]),
				MessageText = (row["MessageText"] as string),
				MessageSubject = (row["MessageSubject"] as string),
				TalentName = (row["TalentName"] as string),
				OriginalMessageId = Convert.ToInt32(row["OriginalMessageId"]),
				TalentId = Convert.ToInt32(row["TalentId"]),
				Status = (row["Status"] as string),
				StatusOfTalent = (row["StatusOfTalent"] as string),
				TalentImageURL = (row["TalentImage"] as string),
				DirectorImageURL = (row["DirectorImage"] as string)
			};
		}
	}

	public IEnumerable<SentMessageDetails> GetSentMessagesDetails(string email)
	{
		if (string.IsNullOrEmpty(email))
		{
			throw new ArgumentException("Email is required", "email");
		}
		string procedureName = "USP_GETSENTMESSAGEDETAILS";
		Dictionary<string, object> parameters = new Dictionary<string, object> { { "@Email", email } };
		DataTable dataTable = _dbManager.ReadData(procedureName, CommandType.StoredProcedure, parameters);
		foreach (DataRow row in dataTable.Rows)
		{
			yield return new SentMessageDetails
			{
				NoticeId = ((row["NoticeId"] != DBNull.Value) ? Convert.ToInt32(row["NoticeId"]) : 0),
				NoticeTitle = (row["NoticeTitle"] as string),
				LatestMessageId = ((row["LatestMessage"] != DBNull.Value) ? Convert.ToInt32(row["LatestMessage"]) : 0),
				MessageId = Convert.ToInt32(row["MessageId"]),
				ConversationDate = Convert.ToDateTime(row["ConversationDate"]),
				MessageText = (row["MessageText"] as string),
				MessageSubject = (row["MessageSubject"] as string),
				TalentName = (row["TalentName"] as string),
				OriginalMessageId = Convert.ToInt32(row["OriginalMessageId"]),
				TalentId = Convert.ToInt32(row["TalentId"]),
				Status = (row["Status"] as string),
				StatusOfTalent = (row["StatusOfTalent"] as string),
				TalentImageURL = (row["TalentImage"] as string),
				DirectorImageURL = (row["DirectorImage"] as string)
			};
		}
	}

	public IEnumerable<ArchiveMessageDetails> GetArchiveMessagesDetails(string email, string user, string sortOrder)
	{
		if (string.IsNullOrEmpty(email))
		{
			throw new ArgumentException("Email is required", "email");
		}
		string procedureName = "USP_GETARCHIVEMESSAGEDETAILS";
		Dictionary<string, object> parameters = new Dictionary<string, object>
		{
			{ "@Email", email },
			{ "@User", user },
			{ "@SortOrder", sortOrder }
		};
		DataTable dataTable = _dbManager.ReadData(procedureName, CommandType.StoredProcedure, parameters);
		foreach (DataRow row in dataTable.Rows)
		{
			yield return new ArchiveMessageDetails
			{
				NoticeId = ((row["NoticeId"] != DBNull.Value) ? Convert.ToInt32(row["NoticeId"]) : 0),
				NoticeTitle = (row["NoticeTitle"] as string),
				LatestMessageId = ((row["LatestMessage"] != DBNull.Value) ? Convert.ToInt32(row["LatestMessage"]) : 0),
				MessageId = Convert.ToInt32(row["MessageId"]),
				ConversationDate = Convert.ToDateTime(row["ConversationDate"]),
				MessageText = (row["MessageText"] as string),
				MessageSubject = (row["MessageSubject"] as string),
				TalentName = (row["TalentName"] as string),
				OriginalMessageId = Convert.ToInt32(row["OriginalMessageId"]),
				TalentId = Convert.ToInt32(row["TalentId"]),
				Status = (row["Status"] as string),
				StatusOfTalent = (row["StatusOfTalent"] as string),
				TalentImageURL = (row["TalentImage"] as string),
				DirectorImageURL = (row["DirectorImage"] as string),
				Origin = (row["Origin"] as string)
			};
		}
	}

	public IEnumerable<MessageModel> ShowMessages(int originalMessageId, string status, string user)
	{
		if (originalMessageId <= 0)
		{
			throw new ArgumentException("Invalid Message ID", "originalMessageId");
		}
		string procedureName = "USP_SHOWMESSAGES";
		Dictionary<string, object> parameters = new Dictionary<string, object>
		{
			{ "@OriginalMessageId", originalMessageId },
			{ "@Status", status },
			{ "@User", user }
		};
		DataTable dataTable = _dbManager.ReadData(procedureName, CommandType.StoredProcedure, parameters);
		foreach (DataRow row in dataTable.Rows)
		{
			yield return new MessageModel
			{
				Id = Convert.ToInt32(row["Id"]),
				MsgText = (row["msg_text"] as string),
				FromEmail = (row["FromEmail"] as string),
				ConversationDate = ((row["ConversationDate"] != DBNull.Value) ? Convert.ToDateTime(row["ConversationDate"]) : DateTime.MinValue),
				Subject = (row["Subject"] as string),
				MessageReceipt = (row["MessageReceipt"] as string),
				JobCategory = (row["RE"] as string),
				TalentImage = (row["TalentImage"] as string),
				FileURL = (row["FileURL"] as string),
				DirectorImage = (row["DirectorImage"] as string),
				CanReply = ((row["CanReply"] == DBNull.Value) ? ((bool?)null) : new bool?(Convert.ToBoolean(row["CanReply"]))),
				SenderEmail = (row["SenderEmail"] as string),
				RecipientEmail = (row["RecipientEmail"] as string)
			};
		}
	}

	public async Task<int> SendMessage(SendMessageModel message)
	{
		string baseDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Uploads");
		if (!Directory.Exists(baseDirectory))
		{
			Directory.CreateDirectory(baseDirectory);
		}
		bool hasAttachment = false;
		bool isUrlOnly = false;
		string originalFileName = string.Empty;
		if (message.File != null && message.File.Length > 0)
		{
			originalFileName = message.File.FileName;
			string uniqueFileName = GenerateUniqueFileName(message.File.FileName);
			string filePath = Path.Combine(baseDirectory, uniqueFileName);
			await SaveFileAsync(message.File, filePath);
			HttpRequest request = _httpContextAccessor.HttpContext?.Request;
			if (request != null)
			{
				string baseUrl = $"{request.Scheme}://{request.Host}/uploads";
				message.FileURL = baseUrl + "/" + uniqueFileName;
			}
			else
			{
				message.FileURL = "/uploads/" + uniqueFileName;
			}
			hasAttachment = true;
		}
		else if (!string.IsNullOrWhiteSpace(message.FileURL))
		{
			hasAttachment = true;
			isUrlOnly = true;
		}
		string procedureName = "USP_INSERT_MESSAGE";
		Dictionary<string, object> parameters = TakeMessageDataFields(message);
		DataTable resultTable = _dbManager.ReadData(procedureName, CommandType.StoredProcedure, parameters);
		int originalMessageId = 0;
		int newMessageId = 0;
		bool result = false;
		if (resultTable != null && resultTable.Rows.Count > 0)
		{
			DataRow row = resultTable.Rows[0];
			originalMessageId = ((row["OriginalMessageId"] != DBNull.Value) ? Convert.ToInt32(row["OriginalMessageId"]) : 0);
			newMessageId = ((row["NewMessageId"] != DBNull.Value) ? Convert.ToInt32(row["NewMessageId"]) : 0);
			result = originalMessageId > 0;
		}
		if (result && !string.IsNullOrWhiteSpace(message.ToEmail))
		{
			var realtimePayload = new
			{
				OriginalMessageId = originalMessageId,
				NewMessageId = newMessageId,
				NoticeId = message.NoticeId,
				MessageText = message.MessageText,
				MessageSubject = message.MessageSubject,
				FromEmail = message.FromEmail,
				ToEmail = message.ToEmail,
				MessageFrom = message.MessageFrom,
				FileURL = message.FileURL,
				ConversationDate = DateTime.UtcNow
			};
			await _chatHub.Clients.Group($"conv-{originalMessageId}").SendAsync("ReceiveMessage", realtimePayload);
		}
		if (result && !string.IsNullOrWhiteSpace(message.ToEmail))
		{
			string subject = ((!string.IsNullOrWhiteSpace(message.MessageSubject)) ? message.MessageSubject : "You have a new message on DirectSubmit");
			string senderName = (string.Equals(message.MessageFrom, "Director", StringComparison.OrdinalIgnoreCase) ? "Casting Director" : ((!string.IsNullOrWhiteSpace(message.MessageFrom)) ? message.MessageFrom : "DirectSubmit"));
			_ = _httpContextAccessor.HttpContext?.Request;
			string messageFromName = (string.IsNullOrWhiteSpace(message.MessageFromName) ? "" : (", " + message.MessageFromName));
			string loginInboxUrl = "https://directsubmit.nycastings.com" + "/login";
			string attachmentBlock = string.Empty;
			if (hasAttachment)
			{
				string fileUrl = message.FileURL;
				if (isUrlOnly)
				{
					string cleanUrl = fileUrl;
					Match match = Regex.Match(fileUrl, "https?://[^\\s\\]]+");
					if (match.Success)
					{
						cleanUrl = match.Value;
					}
					attachmentBlock = $"\r\n                        <div style='background:#f9f9f9;border:1px solid #dddddd;border-radius:4px;\r\n                                    padding:12px 16px;margin:0 0 24px;font-size:14px;'>\r\n                          <span style='color:#333333;font-weight:bold;'>Link:&nbsp;</span>\r\n                          <a href='{cleanUrl}' target='_blank'\r\n                             style='color:#789eeb;text-decoration:underline;word-break:break-all;'>{cleanUrl}</a>\r\n                        </div>";
				}
				else
				{
					string displayName = ((!string.IsNullOrWhiteSpace(originalFileName)) ? originalFileName : Path.GetFileName(fileUrl));
					attachmentBlock = $"\r\n                    <div style='background:#f9f9f9;border:1px solid #dddddd;border-radius:4px;\r\n                                padding:12px 16px;margin:0 0 24px;font-size:14px;'>\r\n                      <span style='color:#333333;font-weight:bold;'>Attachment:&nbsp;</span>\r\n                      <a href='{fileUrl}' target='_blank'\r\n                         style='color:#789eeb;text-decoration:underline;'>{displayName}</a>\r\n                    </div>";
				}
			}
			string messageText = ((!string.IsNullOrWhiteSpace(message.MessageText)) ? message.MessageText : (hasAttachment ? "New attachment sent" : string.Empty));
			string messageTextBlock = string.Empty;
			if (!string.IsNullOrWhiteSpace(messageText))
			{
				messageTextBlock = "\r\n                <div style='background:#f9f9f9;border:1px solid #dddddd;\r\n                            border-left:5px solid #789eeb;\r\n                            padding:16px;margin:0 0 24px;\r\n                            font-size:14px;color:#333333;line-height:1.6;'>\r\n                  " + messageText + "\r\n                </div>";
			}
			string body = $"<!DOCTYPE html>\r\n<html>\r\n<head>\r\n  <meta charset='UTF-8'/>\r\n  <meta name='viewport' content='width=device-width, initial-scale=1.0'/>\r\n</head>\r\n<body style='margin:0;padding:0;background:#f4f4f4;font-family:Arial,sans-serif;font-size:14px;color:#333333;'>\r\n\r\n<table width='100%' cellpadding='0' cellspacing='0' border='0' style='background:#f4f4f4;'>\r\n<tr><td align='center' style='padding:20px 0;'>\r\n\r\n  <table width='600' cellpadding='0' cellspacing='0' border='0'\r\n         style='background:#ffffff;border:1px solid #dddddd;'>\r\n\r\n    <!-- HEADER / LOGO -->\r\n    <tr>\r\n      <td style='background:#333333;padding:0;text-align:center;'>\r\n        <img src='https://files.constantcontact.com/82886fe2301/302cc6b2-5b9a-4673-8b39-143010d6b262.jpg?rdr=true'\r\n             alt='DirectSubmit' width='600'\r\n             style='display:block;width:100%;max-width:600px;border:0;'/>\r\n      </td>\r\n    </tr>\r\n\r\n    <!-- BLUE BANNER : sender name -->\r\n    <tr>\r\n      <td style='background:#789eeb;padding:14px 30px;'>\r\n        <span style='color:#ffffff;font-size:18px;font-weight:bold;'>\r\n          Message From {senderName}{messageFromName}\r\n        </span>\r\n      </td>\r\n    </tr>\r\n\r\n    <!-- MESSAGE + ATTACHMENT + LOGIN BUTTON -->\r\n    <tr>\r\n      <td style='background:#ffffff;padding:28px 30px;'>\r\n\r\n        {messageTextBlock}\r\n\r\n        {attachmentBlock}\r\n\r\n        <!-- Login to Your Account -> goes straight to inbox -->\r\n        <table cellpadding='0' cellspacing='0' border='0' align='center' style='margin:0 auto;'>\r\n          <tr>\r\n            <td style='background:#789eeb;border-radius:4px;'>\r\n              <a href='{loginInboxUrl}' target='_blank'\r\n                 style='display:inline-block;padding:12px 28px;\r\n                        font-family:Arial,sans-serif;font-size:14px;font-weight:bold;\r\n                        color:#ffffff;text-decoration:none;'>\r\n                Login to Your Account\r\n              </a>\r\n            </td>\r\n          </tr>\r\n        </table>\r\n\r\n      </td>\r\n    </tr>\r\n\r\n    <!-- DIVIDER -->\r\n    <tr>\r\n      <td style='padding:0 30px;background:#ffffff;'>\r\n        <hr style='border:none;border-top:1px solid #dddddd;margin:0;'/>\r\n      </td>\r\n    </tr>\r\n\r\n    <!-- FOOTER -->\r\n    <tr>\r\n      <td style='background:#ffffff;padding:14px 20px;text-align:center;\r\n                 font-family:Arial,sans-serif;font-size:12px;color:#888888;line-height:1.6;'>\r\n        <p style='margin:0 0 8px;'>\r\n          By using the DirectSubmit platform, you agree to the terms and conditions of our\r\n          <a href='https://directsubmit.nycastings.com/terms-and-condition'\r\n             target='_blank' style='color:#789eeb;text-decoration:underline;'>Terms of Service</a>\r\n          and\r\n          <a href='https://directsubmit.nycastings.com/privacy-policy'\r\n             target='_blank' style='color:#789eeb;text-decoration:underline;'>Privacy Policy</a>.\r\n          &copy; DirectSubmit\r\n        </p>\r\n        <p style='margin:0;text-align:center;background:#333333;padding:10px;font-size:13px;color:#ffffff;'>\r\n          <a href='https://directsubmit.nycastings.com'\r\n             style='color:#789eeb;text-decoration:none;'>DirectSubmit.com</a>\r\n          &nbsp;|&nbsp; 1480 Vine St. &nbsp;|&nbsp; Los Angeles, CA 90028\r\n        </p>\r\n      </td>\r\n    </tr>\r\n\r\n  </table>\r\n\r\n</td></tr>\r\n</table>\r\n</body>\r\n</html>";
			_emailService.SendTalentWelcomeEmail(message.ToEmail, subject, body);
		}
		return result ? originalMessageId : 0;
	}

	private static Dictionary<string, object> TakeMessageDataFields(SendMessageModel message)
	{
		Dictionary<string, object> obj = new Dictionary<string, object>
		{
			{
				"@MessageText",
				message?.MessageText ?? string.Empty
			},
			{
				"@FromEmail",
				message?.FromEmail ?? string.Empty
			},
			{
				"@MessageSubject",
				message?.MessageSubject ?? string.Empty
			},
			{
				"@ToEmail",
				message?.ToEmail ?? string.Empty
			}
		};
		int? num = message?.NoticeId;
		obj.Add("@NoticeId", num.HasValue ? ((object)num.GetValueOrDefault()) : DBNull.Value);
		obj.Add("@RecipientName", message?.RecipientName ?? string.Empty);
		num = message?.TalentId;
		obj.Add("@TalentId", num.HasValue ? ((object)num.GetValueOrDefault()) : DBNull.Value);
		obj.Add("@FileURL", message?.FileURL ?? string.Empty);
		num = message?.ParentId;
		obj.Add("@ParentId", num.HasValue ? ((object)num.GetValueOrDefault()) : DBNull.Value);
		obj.Add("@MessageFrom", message?.MessageFrom ?? string.Empty);
		bool? flag = message?.CanReply;
		obj.Add("@CanReply", flag.HasValue ? ((object)(flag == true)) : DBNull.Value);
		return obj;
	}

	public bool ToggleArchiveMessages(string originalMsgIdsCsv, bool activeFlag, string archiveFrom)
	{
		string procedureName = "USP_ARCHIVE_MESSAGES";
		Dictionary<string, object> parameters = new Dictionary<string, object>
		{
			{ "@OriginalMsgIds", originalMsgIdsCsv },
			{
				"@ActiveFlag",
				activeFlag ? 1 : 0
			},
			{ "@ArchiveFrom", archiveFrom }
		};
		return _dbManager.InsertOrUpdateData(procedureName, CommandType.StoredProcedure, parameters);
	}

	public bool DeleteMessages(string originalMsgIdsCsv, bool activeFlag, bool permanentArchive, string userId)
	{
		string procedureName = "USP_DELETE_MESSAGES";
		Dictionary<string, object> parameters = new Dictionary<string, object>
		{
			{ "@OriginalMsgIds", originalMsgIdsCsv },
			{
				"@ActiveFlag",
				activeFlag ? 1 : 0
			},
			{
				"@PermanentArchive",
				permanentArchive ? 1 : 0
			},
			{ "@UserId", userId }
		};
		return _dbManager.InsertOrUpdateData(procedureName, CommandType.StoredProcedure, parameters);
	}

	public IEnumerable<ClientDetailsModel> GetClientDetails(string clientEmail)
	{
		if (string.IsNullOrWhiteSpace(clientEmail))
		{
			throw new ArgumentException("Client Email cannot be empty", "clientEmail");
		}
		string procedureName = "USP_GETCLIENTDETAILS";
		Dictionary<string, object> parameters = new Dictionary<string, object> { { "@ClientEmail", clientEmail } };
		DataTable dataTable = _dbManager.ReadData(procedureName, CommandType.StoredProcedure, parameters);
		foreach (DataRow row in dataTable.Rows)
		{
			yield return new ClientDetailsModel
			{
				UserId = ((row["Userid"] != DBNull.Value) ? Convert.ToInt32(row["Userid"]) : 0),
				Password = (row["Password"] as string),
				ContactEmail = (row["ContactEmail"] as string),
				YourName = (row["YourName"] as string),
				Name2 = (row["name2"] as string),
				Name3 = (row["name3"] as string),
				PhoneNumber = (row["name4"] as string),
				CompanyName = (row["CompanyName"] as string),
				RecId = ((row["rec_id"] != DBNull.Value) ? Convert.ToInt32(row["rec_id"]) : 0),
				Address1 = (row["address1"] as string),
				Address2 = (row["address2"] as string),
				City = (row["city"] as string),
				State1 = (row["state1"] as string),
				Zip = (row["zip"] as string),
				Logo = (row["Logo"] as string),
				FirstName = row["FirstName"].ToString(),
				LastName = row["LastName"].ToString(),
				LoginName = row["LoginName"].ToString(),
				UserStatus = row["UserStatus"].ToString(),
				UserCredits = Convert.ToInt32(row["UserCredits"])
			};
		}
	}

	public async Task<bool> UpdateClientDetails(UpdateClientDetailsModel client)
	{
		if (client.UserId <= 0)
		{
			throw new ArgumentException("Invalid User ID", "UserId");
		}
		string baseDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Uploads");
		if (!Directory.Exists(baseDirectory))
		{
			Directory.CreateDirectory(baseDirectory);
		}
		if (client.Logo != null)
		{
			string uniqueFileName = GenerateUniqueFileName(client.Logo.FileName);
			string filePath = Path.Combine(baseDirectory, uniqueFileName);
			await SaveFileAsync(client.Logo, filePath);
			HttpRequest request = _httpContextAccessor.HttpContext?.Request;
			if (request != null)
			{
				string baseUrl = $"{request.Scheme}://{request.Host}/uploads";
				client.FileURL = baseUrl + "/" + uniqueFileName;
			}
			else
			{
				client.FileURL = "/uploads/" + uniqueFileName;
			}
		}
		string procedureName = "USP_UPDATE_CLIENT_DETAILS";
		Dictionary<string, object> parameters = new Dictionary<string, object>
		{
			{ "@UserId", client.UserId },
			{
				"@Password",
				((object)client.Password) ?? ((object)DBNull.Value)
			},
			{
				"@ContactEmail",
				((object)client.ContactEmail) ?? ((object)DBNull.Value)
			},
			{
				"@FirstName",
				((object)client.FirstName) ?? ((object)DBNull.Value)
			},
			{
				"@LastName",
				((object)client.LastName) ?? ((object)DBNull.Value)
			},
			{
				"@PhonNumber",
				((object)client.PhonNumber) ?? ((object)DBNull.Value)
			},
			{
				"@CompanyName",
				((object)client.CompanyName) ?? ((object)DBNull.Value)
			},
			{
				"@Address1",
				((object)client.Address1) ?? ((object)DBNull.Value)
			},
			{
				"@Address2",
				((object)client.Address2) ?? ((object)DBNull.Value)
			},
			{
				"@City",
				((object)client.City) ?? ((object)DBNull.Value)
			},
			{
				"@State1",
				((object)client.State1) ?? ((object)DBNull.Value)
			},
			{
				"@Zip",
				((object)client.Zip) ?? ((object)DBNull.Value)
			},
			{
				"@Logo",
				client?.FileURL ?? string.Empty
			}
		};
		return _dbManager.InsertOrUpdateData(procedureName, CommandType.StoredProcedure, parameters);
	}

	public bool CheckEmailExists(string email)
	{
		if (string.IsNullOrWhiteSpace(email))
		{
			throw new ArgumentException("Email cannot be empty", "email");
		}
		string procedureName = "USP_CHECK_EMAIL_EXISTS";
		Dictionary<string, object> parameters = new Dictionary<string, object> { { "@Email", email } };
		DataTable dataTable = _dbManager.ReadData(procedureName, CommandType.StoredProcedure, parameters);
		if (dataTable.Rows.Count > 0)
		{
			return Convert.ToBoolean(dataTable.Rows[0]["EmailExists"]);
		}
		return false;
	}

	public bool CheckUserNameExists(string userName)
	{
		if (string.IsNullOrWhiteSpace(userName))
		{
			throw new ArgumentException("UserId cannot be empty", "userName");
		}
		string procedureName = "USP_CHECK_USERNAME_EXISTS";
		Dictionary<string, object> parameters = new Dictionary<string, object> { { "@UserName", userName } };
		DataTable dataTable = _dbManager.ReadData(procedureName, CommandType.StoredProcedure, parameters);
		if (dataTable.Rows.Count > 0)
		{
			return Convert.ToBoolean(dataTable.Rows[0]["UserNameExists"]);
		}
		return false;
	}

	public int GetUserActiveStatus(int userId)
	{
		string procedureName = "USP_GET_USER_ACTIVE_STATUS";
		Dictionary<string, object> parameters = new Dictionary<string, object> { { "@UserId", userId } };
		DataTable dataTable = _dbManager.ReadData(procedureName, CommandType.StoredProcedure, parameters);
		if (dataTable == null || dataTable.Rows.Count == 0)
		{
			return 0;
		}
		DataRow row = dataTable.Rows[0];
		if (row["IsActive"] == DBNull.Value)
		{
			return 0;
		}
		return Convert.ToInt32(row["IsActive"]);
	}

	public IEnumerable<InboxTalentMessageModel> GetInboxTalentMessages(string email, string sortOrder)
	{
		string procedureName = "USP_GetInboxTalentMessages";
		Dictionary<string, object> parameters = new Dictionary<string, object>
		{
			{ "@Email", email },
			{ "@SortOrder", sortOrder }
		};
		DataTable dataTable = _dbManager.ReadData(procedureName, CommandType.StoredProcedure, parameters);
		List<InboxTalentMessageModel> result = new List<InboxTalentMessageModel>();
		foreach (DataRow row in dataTable.Rows)
		{
			result.Add(new InboxTalentMessageModel
			{
				LatestMessageId = ((row["LatestMessageId"] != DBNull.Value) ? Convert.ToInt32(row["LatestMessageId"]) : 0),
				NoticeId = ((row["NoticeId"] != DBNull.Value) ? Convert.ToInt32(row["NoticeId"]) : 0),
				NoticeTitle = ((row["NoticeTitle"] != DBNull.Value) ? row["NoticeTitle"].ToString() : null),
				ConversationDate = ((row["Conversationdate"] != DBNull.Value) ? Convert.ToDateTime(row["Conversationdate"]) : DateTime.MinValue),
				MessageText = ((row["MessageText"] != DBNull.Value) ? row["MessageText"].ToString() : null),
				MessageSubject = ((row["MessageSubject"] != DBNull.Value) ? row["MessageSubject"].ToString() : null),
				TalentName = ((row["TalentName"] != DBNull.Value) ? row["TalentName"].ToString() : null),
				OriginalMessageId = ((row["OriginalMessageId"] != DBNull.Value) ? Convert.ToInt32(row["OriginalMessageId"]) : 0),
				TalentId = ((row["TalentId"] != DBNull.Value) ? Convert.ToInt32(row["TalentId"]) : 0),
				Status = ((row["Status"] != DBNull.Value) ? row["Status"].ToString() : null),
				TalentImage = ((row["TalentImage"] != DBNull.Value) ? row["TalentImage"].ToString() : null),
				DirectorImage = ((row["DirectorImage"] != DBNull.Value) ? row["DirectorImage"].ToString() : null)
			});
		}
		return result;
	}

	public int GetUnreadMessageCount(string email)
	{
		if (string.IsNullOrEmpty(email))
		{
			throw new ArgumentException("Email is required", "email");
		}
		string procedureName = "USP_GetUnreadMessageCount";
		Dictionary<string, object> parameters = new Dictionary<string, object> { { "@Email", email } };
		DataTable dataTable = _dbManager.ReadData(procedureName, CommandType.StoredProcedure, parameters);
		if (dataTable.Rows.Count > 0)
		{
			if (dataTable.Rows[0]["UnreadMessageCount"] == DBNull.Value)
			{
				return 0;
			}
			return Convert.ToInt32(dataTable.Rows[0]["UnreadMessageCount"]);
		}
		return 0;
	}
}

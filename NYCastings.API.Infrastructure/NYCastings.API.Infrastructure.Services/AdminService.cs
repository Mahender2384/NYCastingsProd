using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NYCasting.Core.Models;
using NYCasting.Infrastructure.DataAccess;
using NYCastings.API.Core.Contracts.AdminInterface;
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
using NYCastings.API.Core.Models.UnapprovedNoticeResponseModel;
using NYCastings.API.Core.Models.UserBillingModel;
using NYCastings.API.Core.Models.UserDetailsByRoleModel;
using NYCastings.API.Core.Models.UserResume;
using NYCastings.API.Core.Models.UserVisibility;

namespace NYCastings.API.Infrastructure.Services;

public class AdminService : BaseApiService, IAdminService
{
	private readonly DbManager _dbManager;

	private readonly EmailService _emailService;

	private readonly IHttpContextAccessor _httpContextAccessor;

	private readonly IHttpClientFactory _httpFactory;

	private readonly IConfiguration _config;

	private readonly RoleAlertService _roleAlertService;

	private readonly string _webhookUsername;

	private readonly string _webhookPassword;

	private readonly string _site;

	private readonly string _apiKey;

	private readonly string _webhookSecret;

	public AdminService(IOptions<ConnectionString> dbConfig, IHttpContextAccessor httpContextAccessor, IHttpClientFactory httpFactory, IConfiguration configuration, RoleAlertService roleAlertService, EmailService emailService)
		: base(dbConfig)
	{
		if (dbConfig == null || string.IsNullOrWhiteSpace(dbConfig.Value.NYCasting))
		{
			throw new ArgumentNullException("Connection string for NYCasting is missing.");
		}
		_dbManager = new DbManager(dbConfig.Value.NYCasting);
		_httpContextAccessor = httpContextAccessor;
		_roleAlertService = roleAlertService;
		_httpFactory = httpFactory;
		_config = configuration;
		_emailService = emailService;
		_site = _config["Chargebee:Site"]?.Trim();
		_apiKey = _config["Chargebee:ApiKey"]?.Trim();
		_webhookSecret = _config["Chargebee:WebhookSecret"]?.Trim();
		_webhookUsername = configuration["Chargebee:WebhookUsername"];
		_webhookPassword = configuration["Chargebee:WebhookPassword"];
		if (string.IsNullOrEmpty(_site) || string.IsNullOrEmpty(_apiKey))
		{
			throw new Exception("Chargebee configuration missing in appsettings.json");
		}
	}

	public bool DismissNotification(DismissNotificationRequestModel request)
	{
		string procedureName = "USP_DismissNotification";
		Dictionary<string, object> parameters = TakeDismissNotificationParams(request);
		return _dbManager.InsertOrUpdateData(procedureName, CommandType.StoredProcedure, parameters);
	}

	private Dictionary<string, object> TakeDismissNotificationParams(DismissNotificationRequestModel request)
	{
		return new Dictionary<string, object>
		{
			{ "@UserId", request.UserId },
			{ "@NotificationId", request.NotificationId }
		};
	}

	public int AddOrUpdateNotificationMessage(NotificationMessageRequest request)
	{
		string procedureName = "USP_AddOrUpdateNotificationMessage";
		Dictionary<string, object> parameters = TakeAddOrUpdateNotificationParams(request);
		DataTable result = _dbManager.ReadData(procedureName, CommandType.StoredProcedure, parameters);
		if (result.Rows.Count > 0 && int.TryParse(result.Rows[0]["NotificationId"].ToString(), out var notificationId))
		{
			return notificationId;
		}
		return 0;
	}

	private Dictionary<string, object> TakeAddOrUpdateNotificationParams(NotificationMessageRequest request)
	{
		return new Dictionary<string, object>
		{
			{ "@ID", request.Id },
			{
				"@Title",
				request.Title ?? string.Empty
			},
			{
				"@Message",
				request.Message ?? string.Empty
			},
			{
				"@ForRole",
				request.ForRole ?? string.Empty
			},
			{ "@ActiveFlag", request.ActiveFlag }
		};
	}

	public (IEnumerable<UserDetailsByRoleModel> Users, int Count) GetUserDetailsBasedOnRole(int roleId, string searchText)
	{
		Dictionary<string, object> parameters = new Dictionary<string, object>
		{
			{ "@RoleId", roleId },
			{ "@SearchText", searchText }
		};
		DataTable dataTable = _dbManager.ReadData("USP_GetUserDetailsBasedonRole", CommandType.StoredProcedure, parameters);
		List<UserDetailsByRoleModel> result = new List<UserDetailsByRoleModel>();
		foreach (DataRow row in dataTable.Rows)
		{
			result.Add(new UserDetailsByRoleModel
			{
				UserId = Convert.ToInt32(row["UserId"]),
				RoleName = row["RoleName"].ToString(),
				FirstName = row["FirstName"].ToString(),
				LastName = row["LastName"].ToString(),
				LoginName = row["LoginName"].ToString(),
				Email = row["Email"].ToString(),
				UserStatus = row["UserStatus"].ToString(),
				Password = row["Password"].ToString(),
				TalentImage = row["TalentImage"].ToString(),
				UserCredits = Convert.ToInt32(row["UserCredits"])
			});
		}
		return (Users: result, Count: result.Count);
	}

	public bool DeleteUser(DeleteUserRequestModel request)
	{
		string procedureName = "USP_DeleteUser";
		Dictionary<string, object> parameters = TakeDeleteUserParams(request);
		return _dbManager.InsertOrUpdateData(procedureName, CommandType.StoredProcedure, parameters);
	}

	private Dictionary<string, object> TakeDeleteUserParams(DeleteUserRequestModel request)
	{
		return new Dictionary<string, object> { { "@Email", request.Email } };
	}

	public IEnumerable<NotificationMessageResponse> GetNotificationMessages(int notificationId)
	{
		string procedureName = "USP_GetNotificationMessages";
		Dictionary<string, object> parameters = new Dictionary<string, object> { { "@NotificationId", notificationId } };
		DataTable dataTable = _dbManager.ReadData(procedureName, CommandType.StoredProcedure, parameters);
		List<NotificationMessageResponse> list = new List<NotificationMessageResponse>();
		foreach (DataRow row in dataTable.Rows)
		{
			list.Add(new NotificationMessageResponse
			{
				NotificationId = Convert.ToInt32(row["NotificationId"]),
				Title = row["Title"].ToString(),
				Message = row["Message"].ToString(),
				ForRole = row["ForRole"].ToString(),
				CreatedAt = Convert.ToDateTime(row["CreatedAt"]),
				ActiveFlag = Convert.ToBoolean(row["ActiveFlag"])
			});
		}
		return list;
	}

	public IEnumerable<ActiveRolesModel> GetAllRoles()
	{
		string procedureName = "USP_GetAllRoles";
		DataTable dataTable = _dbManager.ReadData(procedureName, CommandType.StoredProcedure);
		List<ActiveRolesModel> roleList = new List<ActiveRolesModel>();
		foreach (DataRow row in dataTable.Rows)
		{
			roleList.Add(new ActiveRolesModel
			{
				RoleId = Convert.ToInt32(row["RoleId"]),
				RoleName = row["RoleName"].ToString(),
				RecordCreatedDate = Convert.ToDateTime(row["RecordCreatedDate"]),
				ActiveFlag = Convert.ToBoolean(row["ActiveFlag"])
			});
		}
		return roleList;
	}

	public PaginatedUnapprovedNoticeResponse GetUnapprovedNotices(string status, string email, int page, int pageSize, string sortOrder)
	{
		Dictionary<string, object> parameters = new Dictionary<string, object>
		{
			{
				"@Status",
				((object)status) ?? ((object)DBNull.Value)
			},
			{
				"@Email",
				((object)email) ?? ((object)DBNull.Value)
			},
			{ "@Page", page },
			{ "@PageSize", pageSize },
			{ "@SortOrder", sortOrder }
		};
		DataSet dataSet = _dbManager.ReadDataSet("USP_GetunApprovedNotices", CommandType.StoredProcedure, parameters);
		DataTable dtNotices = dataSet.Tables[0];
		int totalRecords = Convert.ToInt32(dataSet.Tables[1].Rows[0]["TotalCount"]);
		List<UnapprovedNoticeModel> grouped = (from x in dtNotices.AsEnumerable()
			group x by Convert.ToInt32(x["NoticeId"])).Select(delegate(IGrouping<int, DataRow> group)
		{
			DataRow dataRow = group.First();
			return new UnapprovedNoticeModel
			{
				NoticeId = Convert.ToInt32(dataRow["NoticeId"]),
				ProjectType = Convert.ToString(dataRow["ProjectType"]),
				Union = Convert.ToString(dataRow["Union"]),
				NoticeStartDate = ((dataRow["NoticeStartDate"] == DBNull.Value) ? ((DateTime?)null) : new DateTime?(Convert.ToDateTime(dataRow["NoticeStartDate"]))),
				NoticeEndDate = ((dataRow["NoticeEndDate"] == DBNull.Value) ? ((DateTime?)null) : new DateTime?(Convert.ToDateTime(dataRow["NoticeEndDate"]))),
				ProtectFlag = (dataRow["ProtectFlag"] != DBNull.Value && Convert.ToBoolean(dataRow["ProtectFlag"])),
				Title = Convert.ToString(dataRow["Title"]),
				Pay = Convert.ToString(dataRow["Pay"]),
				Rate = ((dataRow["Rate"] != DBNull.Value) ? Convert.ToInt32(dataRow["Rate"]) : 0),
				Category = Convert.ToString(dataRow["Category"]),
				LocationCodes = Convert.ToString(dataRow["LocationCodes"]),
				NoticeDescription = Convert.ToString(dataRow["NoticeDescription"]),
				NoticeShortDescription = Convert.ToString(dataRow["NoticeShortDescription"]),
				NoticeSubmittedByEmail = Convert.ToString(dataRow["NoticeSubmittedByEmail"]),
				DirectorName = Convert.ToString(dataRow["DirectorName"]),
				NoticeStatus = Convert.ToString(dataRow["NoticeStatus"]),
				NoticeCreatedDate = ((dataRow["NoticeCreatedDate"] == DBNull.Value) ? ((DateTime?)null) : new DateTime?(Convert.ToDateTime(dataRow["NoticeCreatedDate"]))),
				NoticeUpdatedDate = ((dataRow["NoticeUpdatedDate"] == DBNull.Value) ? ((DateTime?)null) : new DateTime?(Convert.ToDateTime(dataRow["NoticeUpdatedDate"]))),
				Roles = group.Select((DataRow r) => new UnapprovedRoleModel
				{
					RoleId = ((r["RoleId"] != DBNull.Value) ? Convert.ToInt32(r["RoleId"]) : 0),
					Rolename = Convert.ToString(r["Rolename"]),
					Sex = Convert.ToString(r["Sex"]),
					Ethnicity = Convert.ToString(r["Ethnicity"]),
					RoleUnion = Convert.ToString(r["RoleUnion"]),
					AgeStart = ((r["AgeStart"] == DBNull.Value) ? ((int?)null) : new int?(Convert.ToInt32(r["AgeStart"]))),
					AgeEnd = ((r["AgeEnd"] == DBNull.Value) ? ((int?)null) : new int?(Convert.ToInt32(r["AgeEnd"]))),
					RoleType = Convert.ToString(r["RoleType"]),
					RoleDetails = Convert.ToString(r["RoleDetails"])
				}).ToList()
			};
		}).ToList();
		return new PaginatedUnapprovedNoticeResponse
		{
			Notices = grouped,
			TotalCount = totalRecords
		};
	}

	public bool ApproveOrRejectNotice(NoticeApprovalRequest request)
	{
		string Procedure = "USP_ApproveOrRejectNotice";
		Dictionary<string, object> parameters = new Dictionary<string, object>
		{
			{ "@NoticeId", request.NoticeId },
			{ "@IsApproved", request.IsApproved }
		};
		bool num = _dbManager.InsertOrUpdateData(Procedure, CommandType.StoredProcedure, parameters);
		if (num && request.IsApproved)
		{
			Task.Run(delegate
			{
				try
				{
					_roleAlertService.QueueEmailsForNotice(request.NoticeId);
				}
				catch
				{
				}
			});
		}
		return num;
	}

	public IEnumerable<UserBillingModel> GetUserBillingDetails()
	{
		string procedure = "USP_GetUserBillingDetails";
		List<UserBillingModel> list = new List<UserBillingModel>();
		foreach (DataRow row in _dbManager.ReadData(procedure, CommandType.StoredProcedure).Rows)
		{
			list.Add(new UserBillingModel
			{
				UserBillingId = ((row["UserBillingId"] != DBNull.Value) ? Convert.ToInt32(row["UserBillingId"]) : 0),
				UserId = ((row["UserId"] != DBNull.Value) ? Convert.ToInt32(row["UserId"]) : 0),
				ProductRecurId = ((row["ProductRecurId"] != DBNull.Value) ? Convert.ToInt32(row["ProductRecurId"]) : 0),
				RoleId = ((row["RoleId"] != DBNull.Value) ? Convert.ToInt32(row["RoleId"]) : 0),
				UserName = (row["UserName"]?.ToString() ?? string.Empty),
				NextBillDate = ((row["NextBillDate"] != DBNull.Value) ? new DateTime?(Convert.ToDateTime(row["NextBillDate"])) : ((DateTime?)null)),
				DateRecordUpdated = ((row["DateRecordUpdated"] != DBNull.Value) ? Convert.ToDateTime(row["DateRecordUpdated"]) : DateTime.MinValue),
				ActiveFlag = (row["ActiveFlag"] != DBNull.Value && Convert.ToBoolean(row["ActiveFlag"]))
			});
		}
		return list;
	}

	public bool AddOrUpdateUserBilling(UserBillingRequestModel model)
	{
		string procedureName = "USP_AddOrUpdateUserBilling";
		Dictionary<string, object> parameters = TakeAddOrUpdateUserBillingParams(model);
		return _dbManager.InsertOrUpdateData(procedureName, CommandType.StoredProcedure, parameters);
	}

	private Dictionary<string, object> TakeAddOrUpdateUserBillingParams(UserBillingRequestModel model)
	{
		Dictionary<string, object> obj = new Dictionary<string, object>
		{
			{ "@UserBillingId", model.UserBillingId },
			{ "@UserId", model.UserId },
			{ "@ProductRecurId", model.ProductRecurId },
			{ "@RoleId", model.RoleId },
			{
				"@UserName",
				model.UserName ?? string.Empty
			}
		};
		DateTime? nextBillDate = model.NextBillDate;
		obj.Add("@NextBillDate", nextBillDate.HasValue ? ((object)nextBillDate.GetValueOrDefault()) : DBNull.Value);
		obj.Add("@ActiveFlag", model.ActiveFlag);
		return obj;
	}

	public IEnumerable<PlanDetail> GetPlanDetails()
	{
		string procedureName = "USP_PlanDetails";
		DataTable dataTable = _dbManager.ReadData(procedureName, CommandType.StoredProcedure);
		foreach (DataRow row in dataTable.Rows)
		{
			yield return new PlanDetail
			{
				ProductRecurId = ((row["ProductRecurId"] != DBNull.Value) ? Convert.ToInt32(row["ProductRecurId"]) : 0),
				Product = ((row["Product"] != DBNull.Value) ? row["Product"].ToString() : string.Empty),
				ProductName = ((row["ProductName"] != DBNull.Value) ? row["ProductName"].ToString() : string.Empty),
				ProductPrice = ((row["ProductPrice"] != DBNull.Value) ? Convert.ToDecimal(row["ProductPrice"]) : 0m),
				RecordCreatedDate = ((row["RecordCreatedDate"] != DBNull.Value) ? Convert.ToDateTime(row["RecordCreatedDate"]) : DateTime.MinValue),
				RecordUpdatedDate = ((row["RecordUpdatedDate"] != DBNull.Value) ? Convert.ToDateTime(row["RecordUpdatedDate"]) : DateTime.MinValue),
				UserId = ((row["UserId"] != DBNull.Value) ? Convert.ToInt32(row["UserId"]) : 0),
				ActiveFlag = (row["ActiveFlag"] != DBNull.Value && Convert.ToBoolean(row["ActiveFlag"]))
			};
		}
	}

	public bool AddOrUpdatePlanDetails(PlanDetailsRequestModel model)
	{
		string procedureName = "USP_AddOrUpdatePlanDetails";
		Dictionary<string, object> parameters = TakePlanDetailsParams(model);
		return _dbManager.InsertOrUpdateData(procedureName, CommandType.StoredProcedure, parameters);
	}

	private Dictionary<string, object> TakePlanDetailsParams(PlanDetailsRequestModel model)
	{
		return new Dictionary<string, object>
		{
			{ "@ProductRecurId", model.ProductRecurId },
			{
				"@Product",
				model.Product ?? string.Empty
			},
			{
				"@ProductName",
				model.ProductName ?? string.Empty
			},
			{ "@ProductPrice", model.ProductPrice },
			{ "@UserId", model.UserId },
			{ "@ActiveFlag", model.ActiveFlag }
		};
	}

	public bool AddOrUpdateUserVisibility(UserVisibilityRequestModel model)
	{
		string procedureName = "USP_AddOrUpdateUserVisibility";
		Dictionary<string, object> parameters = TakeAddOrUpdateUserVisibilityParams(model);
		return _dbManager.InsertOrUpdateData(procedureName, CommandType.StoredProcedure, parameters);
	}

	private Dictionary<string, object> TakeAddOrUpdateUserVisibilityParams(UserVisibilityRequestModel model)
	{
		return new Dictionary<string, object>
		{
			{ "@TalentId", model.TalentId },
			{ "@ClientId", model.ClientId },
			{
				"@IsHide",
				model.IsHide.HasValue ? ((object)model.IsHide.Value) : DBNull.Value
			},
			{
				"@IsBlock",
				model.IsBlock.HasValue ? ((object)model.IsBlock.Value) : DBNull.Value
			}
		};
	}

	public bool UpdateUserDetailsById(UpdateUserDetailsRequestModel model)
	{
		string procedureName = "USP_UpdateUserDetailsById";
		Dictionary<string, object> parameters = TakeUpdateUserDetailsParams(model);
		return _dbManager.InsertOrUpdateData(procedureName, CommandType.StoredProcedure, parameters);
	}

	private Dictionary<string, object> TakeUpdateUserDetailsParams(UpdateUserDetailsRequestModel model)
	{
		return new Dictionary<string, object>
		{
			{ "@UserId", model.UserId },
			{
				"@FirstName",
				((object)model.FirstName) ?? ((object)DBNull.Value)
			},
			{
				"@LastName",
				((object)model.LastName) ?? ((object)DBNull.Value)
			},
			{
				"@LoginName",
				((object)model.LoginName) ?? ((object)DBNull.Value)
			},
			{
				"@Email",
				((object)model.Email) ?? ((object)DBNull.Value)
			},
			{
				"@Password",
				((object)model.Password) ?? ((object)DBNull.Value)
			},
			{
				"@ActiveFlag",
				model.ActiveFlag.HasValue ? ((object)model.ActiveFlag.Value) : DBNull.Value
			},
			{
				"@IsHide",
				model.IsHide.HasValue ? ((object)model.IsHide.Value) : DBNull.Value
			},
			{
				"@IsBlock",
				model.IsBlock.HasValue ? ((object)model.IsBlock.Value) : DBNull.Value
			}
		};
	}

	public NoticeStatusCountResponseModel GetNoticeStatusCounts()
	{
		string procedureName = "USP_GetNoticeStatusCounts";
		DataTable dt = _dbManager.ReadData(procedureName, CommandType.StoredProcedure);
		if (dt.Rows.Count == 0)
		{
			return new NoticeStatusCountResponseModel();
		}
		DataRow row = dt.Rows[0];
		return new NoticeStatusCountResponseModel
		{
			ApprovedCount = ((row["ApprovedCount"] != DBNull.Value) ? Convert.ToInt32(row["ApprovedCount"]) : 0),
			RejectedCount = ((row["RejectedCount"] != DBNull.Value) ? Convert.ToInt32(row["RejectedCount"]) : 0),
			PendingCount = ((row["PendingCount"] != DBNull.Value) ? Convert.ToInt32(row["PendingCount"]) : 0),
			TotalNotices = ((row["TotalNotices"] != DBNull.Value) ? Convert.ToInt32(row["TotalNotices"]) : 0)
		};
	}

	public bool SaveOrUpdateCreditMaster(CreditMasterRequestModel model)
	{
		string procedureName = "USP_SAVE_CREDIT_MASTER";
		Dictionary<string, object> parameters = TakeCreditMasterParams(model);
		return _dbManager.InsertOrUpdateData(procedureName, CommandType.StoredProcedure, parameters);
	}

	private Dictionary<string, object> TakeCreditMasterParams(CreditMasterRequestModel model)
	{
		return new Dictionary<string, object>
		{
			{
				"@CreditNames",
				model.CreditNames ?? string.Empty
			},
			{ "@CreditPoints", model.CreditPoints }
		};
	}

	public bool GrantCreditsToInactiveTalents(AdminGrantCreditsRequestModel model)
	{
		string procedureName = "USP_ADMIN_GRANT_CREDITS_TO_INACTIVE_TALENTS";
		Dictionary<string, object> parameters = new Dictionary<string, object> { { "@CreditPoints", model.CreditPoints } };
		return _dbManager.InsertOrUpdateData(procedureName, CommandType.StoredProcedure, parameters);
	}

	public List<ChargebeePlanModel> GetPlans()
	{
		string procedureName = "USP_GetChargebeePlans";
		DataTable dataTable = _dbManager.ReadData(procedureName, CommandType.StoredProcedure);
		List<ChargebeePlanModel> plans = new List<ChargebeePlanModel>();
		foreach (DataRow row in dataTable.Rows)
		{
			plans.Add(new ChargebeePlanModel
			{
				Id = Convert.ToInt32(row["Id"]),
				PlanId = row["PlanId"].ToString(),
				PlanName = row["PlanName"].ToString(),
				BillingPeriod = row["BillingPeriod"].ToString(),
				Price = Convert.ToDecimal(row["Price"]),
				Currency = row["Currency"].ToString(),
				ChargeModel = row["ChargeModel"].ToString()
			});
		}
		return plans;
	}

	public async Task<string> CreateCheckoutAsync(string email, string planId)
	{
		if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(planId))
		{
			return null;
		}
		email = email.Trim();
		HttpClient client = _httpFactory.CreateClient();
		client.Timeout = TimeSpan.FromSeconds(15.0);
		byte[] authBytes = Encoding.UTF8.GetBytes(_apiKey + ":");
		client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));
		string customerId = null;
		string custUrl = "https://" + _site + ".chargebee.com/api/v2/customers?email[is]=" + Uri.EscapeDataString(email);
		HttpResponseMessage custResponse = await client.GetAsync(custUrl);
		if (custResponse.IsSuccessStatusCode)
		{
			dynamic custResult = JsonConvert.DeserializeObject(await custResponse.Content.ReadAsStringAsync());
			if (custResult?.list != null)
			{
				foreach (dynamic c in custResult.list)
				{
					if (((string)(c.customer.email ?? "")).Equals(email, StringComparison.OrdinalIgnoreCase))
					{
						customerId = (string)c.customer.id;
						break;
					}
				}
			}
		}
		Dictionary<string, string> body = new Dictionary<string, string>
		{
			{ "subscription[plan_id]", planId },
			{ "redirect_url", "https://directsubmit.com/account" }
		};
		if (customerId != null)
		{
			body.Add("customer[id]", customerId);
		}
		else
		{
			body.Add("customer[email]", email);
		}
		FormUrlEncodedContent content = new FormUrlEncodedContent(body);
		string url = "https://" + _site + ".chargebee.com/api/v2/hosted_pages/checkout_new";
		HttpResponseMessage response = await client.PostAsync(url, content);
		string responseText = await response.Content.ReadAsStringAsync();
		if (!response.IsSuccessStatusCode)
		{
			throw new Exception("Chargebee error: " + responseText);
		}
		dynamic result = JsonConvert.DeserializeObject(responseText);
		return (string)result.hosted_page.url;
	}

	public bool SaveChargebeePayment(PaymentSaveRequest model)
	{
		string procedureName = "USP_SaveChargebeePayment";
		Dictionary<string, object> parameters = TakeSavePaymentParams(model);
		return _dbManager.InsertOrUpdateData(procedureName, CommandType.StoredProcedure, parameters);
	}

	private Dictionary<string, object> TakeSavePaymentParams(PaymentSaveRequest req)
	{
		return new Dictionary<string, object>
		{
			{ "@InvoiceId", req.InvoiceId },
			{ "@SubscriptionId", req.SubscriptionId },
			{ "@CustomerId", req.CustomerId },
			{ "@Email", req.Email },
			{ "@PlanId", req.PlanId },
			{ "@ItemPriceId", req.ItemPriceId },
			{ "@Amount", req.Amount },
			{ "@TermStart", req.TermStart },
			{ "@TermEnd", req.TermEnd }
		};
	}

	public async Task<string> RedirectToBillingPortalIfActiveAsync(string email)
	{
		if (string.IsNullOrWhiteSpace(email))
		{
			return null;
		}
		email = email.Trim();
		HttpClient client = _httpFactory.CreateClient();
		client.Timeout = TimeSpan.FromSeconds(15.0);
		byte[] authBytes = Encoding.UTF8.GetBytes(_apiKey + ":");
		client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));
		string customerSearchUrl = "https://" + _site + ".chargebee.com/api/v2/customers?email[is]=" + Uri.EscapeDataString(email);
		HttpResponseMessage customerResponse = await client.GetAsync(customerSearchUrl);
		string customerText = await customerResponse.Content.ReadAsStringAsync();
		if (!customerResponse.IsSuccessStatusCode)
		{
			throw new Exception("Customer lookup failed: " + customerText);
		}
		dynamic customerResult = JsonConvert.DeserializeObject(customerText);
		if (customerResult.list == null || customerResult.list.Count == 0)
		{
			return null;
		}
		string customerId = null;
		foreach (dynamic custItem in customerResult.list)
		{
			if (((string)(custItem.customer.email ?? "")).Equals(email, StringComparison.OrdinalIgnoreCase))
			{
				customerId = (string)custItem.customer.id;
				break;
			}
		}
		if (customerId == null)
		{
			return null;
		}
		string subscriptionUrl = $"https://{_site}.chargebee.com/api/v2/subscriptions?customer_id[is]={Uri.EscapeDataString(customerId)}&limit=100";
		HttpResponseMessage subscriptionResponse = await client.GetAsync(subscriptionUrl);
		string subscriptionText = await subscriptionResponse.Content.ReadAsStringAsync();
		if (!subscriptionResponse.IsSuccessStatusCode)
		{
			throw new Exception("Subscription check failed: " + subscriptionText);
		}
		dynamic subscriptionResult = JsonConvert.DeserializeObject(subscriptionText);
		bool isActive = false;
		if (subscriptionResult.list != null)
		{
			foreach (dynamic item in subscriptionResult.list)
			{
				switch ((string)item.subscription.status)
				{
				case "active":
				case "in_trial":
				case "non_renewing":
				case "future":
					isActive = true;
					goto end_IL_0af6;
				}
				continue;
				end_IL_0af6:
				break;
			}
		}
		if (!isActive)
		{
			return null;
		}
		string portalUrl = "https://" + _site + ".chargebee.com/api/v2/portal_sessions";
		FormUrlEncodedContent portalContent = new FormUrlEncodedContent(new Dictionary<string, string>
		{
			{ "customer[id]", customerId },
			{ "redirect_url", "https://directsubmit.com/account" }
		});
		HttpResponseMessage portalResponse = await client.PostAsync(portalUrl, portalContent);
		string portalText = await portalResponse.Content.ReadAsStringAsync();
		if (!portalResponse.IsSuccessStatusCode)
		{
			throw new Exception("Portal creation failed: " + portalText);
		}
		dynamic portalResult = JsonConvert.DeserializeObject(portalText);
		return (string)portalResult.portal_session.access_url;
	}

	public async Task<(bool Authenticated, bool Success, string Email, string SubscriptionId, string Status, string EventType)> ProcessWebhookAsync(Stream bodyStream, string authHeader)
	{
		if (!IsValidBasicAuth(authHeader))
		{
			return (Authenticated: false, Success: false, Email: null, SubscriptionId: null, Status: null, EventType: null);
		}
		using StreamReader reader = new StreamReader(bodyStream);
		ChargebeeWebhookModel data = JsonConvert.DeserializeObject<ChargebeeWebhookModel>(await reader.ReadToEndAsync());
		if (data == null || data.content?.customer?.email == null || data.content.subscription?.id == null)
		{
			return (Authenticated: true, Success: false, Email: null, SubscriptionId: null, Status: null, EventType: null);
		}
		string email = data.content.customer.email;
		string eventType = data.event_type;
		string subscriptionId = data.content.subscription.id;
		string status = data.content.subscription?.status;
		return (Authenticated: true, Success: true, Email: email, SubscriptionId: subscriptionId, Status: status, EventType: eventType);
	}

	private bool IsActiveStatus(string status)
	{
		switch (status)
		{
		default:
			return status == "past_due";
		case "active":
		case "in_trial":
		case "future":
		case "non_renewing":
			return true;
		}
	}

	private bool IsValidBasicAuth(string authHeader)
	{
		if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		if (string.IsNullOrEmpty(_webhookUsername) || string.IsNullOrEmpty(_webhookPassword))
		{
			return false;
		}
		try
		{
			string encoded = authHeader.Substring("Basic ".Length).Trim();
			string[] parts = Encoding.UTF8.GetString(Convert.FromBase64String(encoded)).Split(':', 2);
			if (parts.Length != 2)
			{
				return false;
			}
			return parts[0] == _webhookUsername && parts[1] == _webhookPassword;
		}
		catch
		{
			return false;
		}
	}

	public bool UpdateUserActiveStatusByEmail(string email, bool isActive)
	{
		string procedureName = "USP_UPDATE_USER_ACTIVE_STATUS_BY_EMAIL";
		Dictionary<string, object> parameters = new Dictionary<string, object>
		{
			{ "@Email", email },
			{
				"@IsActive",
				isActive ? 1 : 0
			}
		};
		DataTable dt = _dbManager.ReadData(procedureName, CommandType.StoredProcedure, parameters);
		bool result = false;
		string userLogin = null;
		string talentName = null;
		if (dt != null && dt.Rows.Count > 0)
		{
			DataRow row = dt.Rows[0];
			result = row["Success"] != DBNull.Value && Convert.ToBoolean(row["Success"]);
			userLogin = ((row["UserLogin"] != DBNull.Value) ? row["UserLogin"].ToString() : null);
			talentName = ((row["TalentName"] != DBNull.Value) ? row["TalentName"].ToString() : null);
		}
		if (result)
		{
			try
			{
				if (isActive)
				{
					_emailService.SendSubscriptionActiveEmail(email, userLogin, talentName);
				}
				else
				{
					_emailService.SendSubscriptionInactiveEmail(email, userLogin, talentName);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("❌ Failed to send subscription status email to " + email + ": " + ex.Message);
			}
		}
		return result;
	}

	public bool UpsertUserSubscriptionStatus(string email, string subscriptionId, string status, out bool isActive)
	{
		string procedureName = "USP_UPSERT_USER_SUBSCRIPTION_STATUS";
		Dictionary<string, object> parameters = new Dictionary<string, object>
		{
			{ "@Email", email },
			{ "@SubscriptionId", subscriptionId },
			{ "@Status", status }
		};
		DataTable dt = _dbManager.ReadData(procedureName, CommandType.StoredProcedure, parameters);
		bool result = false;
		string userLogin = null;
		string talentName = null;
		isActive = false;
		if (dt != null && dt.Rows.Count > 0)
		{
			DataRow row = dt.Rows[0];
			result = row["Success"] != DBNull.Value && Convert.ToBoolean(row["Success"]);
			userLogin = ((row["UserLogin"] != DBNull.Value) ? row["UserLogin"].ToString() : null);
			talentName = ((row["TalentName"] != DBNull.Value) ? row["TalentName"].ToString() : null);
			isActive = row["IsActive"] != DBNull.Value && Convert.ToBoolean(row["IsActive"]);
		}
		if (result)
		{
			try
			{
				if (isActive)
				{
					_emailService.SendSubscriptionActiveEmail(email, userLogin, talentName);
				}
				else
				{
					_emailService.SendSubscriptionInactiveEmail(email, userLogin, talentName);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("❌ Failed to send subscription status email to " + email + ": " + ex.Message);
			}
		}
		return result;
	}

	public async Task<SubscriptionDetailsModel> GetSubscriptionDetailsByEmail(string email)
	{
		if (string.IsNullOrWhiteSpace(email))
		{
			return null;
		}
		email = email.Trim();
		HttpClient client = _httpFactory.CreateClient();
		client.Timeout = TimeSpan.FromSeconds(15.0);
		byte[] authBytes = Encoding.UTF8.GetBytes(_apiKey + ":");
		client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));
		string customerUrl = "https://" + _site + ".chargebee.com/api/v2/customers?email[is]=" + Uri.EscapeDataString(email);
		HttpResponseMessage customerResponse = await client.GetAsync(customerUrl);
		if (!customerResponse.IsSuccessStatusCode)
		{
			return null;
		}
		dynamic customerResult = JsonConvert.DeserializeObject(await customerResponse.Content.ReadAsStringAsync());
		if (customerResult?.list == null || customerResult.list.Count == 0)
		{
			return null;
		}
		List<SubscriptionDetailsModel> validSubs = new List<SubscriptionDetailsModel>();
		foreach (dynamic custItem in customerResult.list)
		{
			if (!((string)(custItem.customer.email ?? "")).Equals(email, StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}
			string customerId = (string)custItem.customer.id;
			string customerName = ((string)(custItem.customer.first_name ?? "") + " " + (string)(custItem.customer.last_name ?? "")).Trim();
			string subUrl = $"https://{_site}.chargebee.com/api/v2/subscriptions?customer_id[is]={Uri.EscapeDataString(customerId)}&limit=100";
			string offset = null;
			do
			{
				string pageUrl = ((offset == null) ? subUrl : (subUrl + "&offset=" + Uri.EscapeDataString(offset)));
				HttpResponseMessage subResponse = await client.GetAsync(pageUrl);
				if (!subResponse.IsSuccessStatusCode)
				{
					break;
				}
				dynamic subResult = JsonConvert.DeserializeObject(await subResponse.Content.ReadAsStringAsync());
				if (subResult?.list == null)
				{
					break;
				}
				foreach (dynamic item in subResult.list)
				{
					dynamic sub = item.subscription;
					string status = (string)sub.status;
					object obj;
					switch (status)
					{
					default:
						obj = status;
						break;
					case "active":
					case "in_trial":
					case "non_renewing":
						obj = "active";
						break;
					}
					string frontendStatus = (string)obj;
					validSubs.Add(new SubscriptionDetailsModel
					{
						CustomerEmail = email,
						CustomerName = customerName,
						PlanName = (string)sub.plan_id,
						SubscriptionId = (string)sub.id,
						Status = frontendStatus,
						NextBillingDate = ((sub.next_billing_at != null) ? new DateTime?(DateTimeOffset.FromUnixTimeSeconds((long)sub.next_billing_at).UtcDateTime) : ((DateTime?)null)),
						CreatedDate = ((sub.created_at != null) ? new DateTime?(DateTimeOffset.FromUnixTimeSeconds((long)sub.created_at).UtcDateTime) : ((DateTime?)null)),
						UpdatedDate = ((sub.updated_at != null) ? new DateTime?(DateTimeOffset.FromUnixTimeSeconds((long)sub.updated_at).UtcDateTime) : ((DateTime?)null)),
						Amount = ((sub.plan_amount != null) ? ((decimal)sub.plan_amount / 100m) : 0m)
					});
				}
				offset = ((subResult.next_offset != null) ? ((string)subResult.next_offset) : null);
			}
			while (!string.IsNullOrEmpty(offset));
		}
		if (validSubs.Count == 0)
		{
			return null;
		}
		return (from s in validSubs
			orderby (s.PlanName ?? string.Empty).IndexOf("lifetime", StringComparison.OrdinalIgnoreCase) >= 0 descending,
				s.UpdatedDate ?? s.CreatedDate ?? DateTime.MinValue descending
			select s).First();
	}

	public async Task<List<SubscriptionDetailsModel>> GetActiveSubscribersAsync()
	{
		HttpClient client = _httpFactory.CreateClient();
		client.Timeout = TimeSpan.FromSeconds(30.0);
		byte[] authBytes = Encoding.UTF8.GetBytes(_apiKey + ":");
		client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));
		List<SubscriptionDetailsModel> activeSubscribers = new List<SubscriptionDetailsModel>();
		string statusFilter = Uri.EscapeDataString("[\"active\",\"in_trial\",\"non_renewing\"]");
		string subUrl = $"https://{_site}.chargebee.com/api/v2/subscriptions?status[in]={statusFilter}&limit=100&sort_by[asc]=created_at";
		string offset = null;
		do
		{
			string pageUrl = ((offset == null) ? subUrl : (subUrl + "&offset=" + Uri.EscapeDataString(offset)));
			HttpResponseMessage response = await client.GetAsync(pageUrl);
			if (!response.IsSuccessStatusCode)
			{
				string errorBody = await response.Content.ReadAsStringAsync();
				throw new Exception($"Chargebee API error ({(int)response.StatusCode}): {errorBody}");
			}
			dynamic result = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());
			if (result?.list == null)
			{
				break;
			}
			foreach (dynamic item in result.list)
			{
				dynamic sub = item.subscription;
				dynamic cust = item.customer;
				string status = (string)sub.status;
				object obj;
				switch (status)
				{
				default:
					obj = status;
					break;
				case "active":
				case "in_trial":
				case "non_renewing":
					obj = "active";
					break;
				}
				string frontendStatus = (string)obj;
				activeSubscribers.Add(new SubscriptionDetailsModel
				{
					CustomerEmail = (string)(cust?.email ?? ""),
					CustomerName = ((string)(cust?.first_name ?? "") + " " + (string)(cust?.last_name ?? "")).Trim(),
					PlanName = (string)sub.plan_id,
					SubscriptionId = (string)sub.id,
					Status = frontendStatus,
					NextBillingDate = ((sub.next_billing_at != null) ? new DateTime?(DateTimeOffset.FromUnixTimeSeconds((long)sub.next_billing_at).UtcDateTime) : ((DateTime?)null)),
					CreatedDate = ((sub.created_at != null) ? new DateTime?(DateTimeOffset.FromUnixTimeSeconds((long)sub.created_at).UtcDateTime) : ((DateTime?)null)),
					UpdatedDate = ((sub.updated_at != null) ? new DateTime?(DateTimeOffset.FromUnixTimeSeconds((long)sub.updated_at).UtcDateTime) : ((DateTime?)null)),
					Amount = ((sub.plan_amount != null) ? ((decimal)sub.plan_amount / 100m) : 0m)
				});
			}
			offset = ((result.next_offset != null) ? ((string)result.next_offset) : null);
		}
		while (!string.IsNullOrEmpty(offset));
		return activeSubscribers;
	}

	public bool UpdateUserResumeStatus(UserResumeStatusModel model)
	{
		string procedureName = "USP_UPDATE_USER_RESUME_STATUS";
		Dictionary<string, object> parameters = new Dictionary<string, object>
		{
			{ "@UserId", model.UserId },
			{ "@IsDelete", model.IsDelete }
		};
		return _dbManager.InsertOrUpdateData(procedureName, CommandType.StoredProcedure, parameters);
	}

	public bool GetUserResumeStatus(int userId)
	{
		string procedureName = "USP_GET_USER_RESUME_STATUS";
		Dictionary<string, object> parameters = new Dictionary<string, object> { { "@UserId", userId } };
		DataTable dt = _dbManager.ReadData(procedureName, CommandType.StoredProcedure, parameters);
		if (dt.Rows.Count == 0)
		{
			return false;
		}
		return dt.Rows[0].Field<bool>("IsResumeDeleted");
	}

	public bool AddOrUpdateQuestion(QuestionModel model)
	{
		string procedureName = "USP_ADD_UPDATE_QUESTION";
		Dictionary<string, object> parameters = new Dictionary<string, object>
		{
			{ "@QuestionId", model.QuestionId },
			{ "@QuestionText", model.QuestionText },
			{ "@AnswerText", model.AnswerText },
			{ "@UserId", model.UserId },
			{ "@Category", model.Category },
			{ "Position", model.Position }
		};
		return _dbManager.InsertOrUpdateData(procedureName, CommandType.StoredProcedure, parameters);
	}

	public bool DeleteQuestion(int questionId, int userId)
	{
		string procedureName = "USP_DELETE_QUESTION";
		Dictionary<string, object> parameters = new Dictionary<string, object>
		{
			{ "@QuestionId", questionId },
			{ "@UserId", userId }
		};
		return _dbManager.InsertOrUpdateData(procedureName, CommandType.StoredProcedure, parameters);
	}

	public IEnumerable<QuestionResponseModel> GetQuestions()
	{
		string procedureName = "USP_GET_QUESTIONS";
		DataTable dt = _dbManager.ReadData(procedureName, CommandType.StoredProcedure);
		foreach (DataRow row in dt.Rows)
		{
			yield return new QuestionResponseModel
			{
				QuestionId = row.Field<int>("QuestionId"),
				QuestionText = row.Field<string>("QuestionText"),
				AnswerText = row.Field<string>("AnswerText"),
				CreatedDate = row.Field<DateTime>("CreatedDate"),
				Category = row.Field<string>("Category"),
				Position = row.Field<int?>("Position").GetValueOrDefault()
			};
		}
	}

	public bool AddOrUpdateQuestionMaster(QuestionMasterModel model)
	{
		string procedureName = "USP_ADD_UPDATE_QUESTION_MASTER";
		Dictionary<string, object> parameters = new Dictionary<string, object>
		{
			{ "@Id", model.QuestionId },
			{ "@QuestionName", model.QuestionName },
			{ "@Position", model.Position }
		};
		return _dbManager.InsertOrUpdateData(procedureName, CommandType.StoredProcedure, parameters);
	}

	public IEnumerable<QuestionMasterModel> GetQuestionMaster()
	{
		string procedureName = "USP_GET_QUESTION_MASTER";
		DataTable dt = _dbManager.ReadData(procedureName, CommandType.StoredProcedure);
		foreach (DataRow row in dt.Rows)
		{
			yield return new QuestionMasterModel
			{
				QuestionId = row.Field<int>("Id"),
				QuestionName = row.Field<string>("QuestionName"),
				Position = row.Field<int?>("Position").GetValueOrDefault()
			};
		}
	}

	public bool DeleteQuestionMaster(int questionId)
	{
		string procedureName = "USP_DELETE_QUESTIONMASTER";
		Dictionary<string, object> parameters = new Dictionary<string, object> { { "@QuestionId", questionId } };
		return _dbManager.InsertOrUpdateData(procedureName, CommandType.StoredProcedure, parameters);
	}

	public bool SendNoticeStatusEmail(int noticeId, bool isApproved, string rejectionReason = null)
	{
		try
		{
			DataTable dt = _dbManager.ReadData("USP_GET_NOTICE_APPROVAL_EMAIL_DATA", CommandType.StoredProcedure, new Dictionary<string, object> { { "@NoticeId", noticeId } });
			if (dt == null || dt.Rows.Count == 0)
			{
				return false;
			}
			DataRow row = dt.Rows[0];
			string contactEmail = row["Email"]?.ToString()?.Trim() ?? "";
			if (string.IsNullOrWhiteSpace(contactEmail))
			{
				return false;
			}
			string directorName = row["DirectorName"]?.ToString()?.Trim() ?? "";
			string noticeTitle = row["NoticeTitle"]?.ToString() ?? "";
			string noticeUnion = string.Join(", ", from u in (row["NoticeUnion"]?.ToString() ?? "").Split(',')
				select u.Trim() into u
				where !string.IsNullOrWhiteSpace(u)
				select u);
			string noticePay = row["NoticePay"]?.ToString() ?? "";
			string locationCodes = row["LocationCodes"]?.ToString() ?? "";
			string noticePosted = ((row["NoticePosted"] != DBNull.Value) ? Convert.ToDateTime(row["NoticePosted"]).ToString("MM/dd/yyyy") : "");
			string noticeEndDate = ((row["NoticeEndDate"] != DBNull.Value) ? Convert.ToDateTime(row["NoticeEndDate"]).ToString("MM/dd/yyyy") : "TBD");
			string noticeDescOne = row["NoticeDescriptionOne"]?.ToString() ?? "";
			string noticeDesc = row["NoticeDescription"]?.ToString() ?? "";
			DataTable allRolesDt = _dbManager.ReadData("USP_GET_ALL_ROLES_FOR_NOTICE", CommandType.StoredProcedure, new Dictionary<string, object> { { "@NoticeId", noticeId } });
			string subject;
			string body;
			if (isApproved)
			{
				subject = "Casting Call Approved: " + noticeTitle;
				body = BuildNoticeApprovedEmailBody(directorName, noticeTitle, noticeUnion, noticePay, locationCodes, noticePosted, noticeEndDate, noticeDescOne, noticeDesc, allRolesDt, noticeId);
			}
			else
			{
				subject = "Casting Notice Not Approved: " + noticeTitle;
				body = BuildNoticeRejectedEmailBody(directorName, noticeTitle, noticeUnion, noticePay, locationCodes, noticePosted, noticeEndDate, noticeDescOne, noticeDesc, allRolesDt, noticeId, rejectionReason);
			}
			_emailService.SendEmailForNoticeSubmit(contactEmail, subject, body);
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	private string BuildNoticeRejectedEmailBody(string directorName, string noticeTitle, string noticeUnion, string noticePay, string locationDisplay, string noticePosted, string noticeEndDate, string noticeDescOne, string noticeDesc, DataTable allRoles, int noticeId, string rejectionReason = null)
	{
		string loginUrl = "https://directsubmit.nycastings.com" + "/login";
		string safeTitle = Enc(noticeTitle);
		string safeDirector = Enc(directorName);
		string safeUnion = Enc(noticeUnion);
		string safePay = Enc(noticePay);
		string safeLocation = Enc(locationDisplay);
		string safeReason = Enc(rejectionReason);
		string formattedDescOne = FormatDescription(noticeDescOne);
		string formattedDesc = FormatDescription(noticeDesc);
		StringBuilder rolesHtml = new StringBuilder();
		if (allRoles != null)
		{
			foreach (DataRow row in allRoles.Rows)
			{
				string roleName = Enc(row["RoleName"]?.ToString());
				string roleSex = Enc(row["RoleSex"]?.ToString());
				string roleEth = Enc(row["RoleEthnicity"]?.ToString());
				string ageStart = row["RoleAgeStart"]?.ToString() ?? "0";
				string ageEnd = row["RoleAgeEnd"]?.ToString() ?? "0";
				string roleType = Enc(row["RoleType"]?.ToString());
				string roleUnion = Enc(row["RoleUnion"]?.ToString());
				string roleDetails = Enc(row["RoleDetails"]?.ToString());
				string ageDisplay = ((ageStart != "0" && ageEnd != "0") ? ("Age: " + ageStart + "-" + ageEnd) : "");
				List<string> attrParts = new List<string>();
				if (!string.IsNullOrWhiteSpace(roleSex))
				{
					attrParts.Add(roleSex);
				}
				if (!string.IsNullOrWhiteSpace(roleEth))
				{
					attrParts.Add(roleEth);
				}
				if (!string.IsNullOrWhiteSpace(ageDisplay))
				{
					attrParts.Add(ageDisplay);
				}
				if (!string.IsNullOrWhiteSpace(roleType))
				{
					attrParts.Add("Role Type: " + roleType);
				}
				if (!string.IsNullOrWhiteSpace(roleUnion))
				{
					attrParts.Add(roleUnion);
				}
				string attrsLine = string.Join("&nbsp;&nbsp;|&nbsp;&nbsp;", attrParts);
				StringBuilder stringBuilder = rolesHtml;
				StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(568, 3, stringBuilder);
				handler.AppendLiteral("\r\n            <table width='100%' cellpadding='0' cellspacing='0' border='0'\r\n                   style='margin:0 0 12px;border:1px solid #dddddd;'>\r\n              <tr>\r\n                <td style='padding:12px 14px;vertical-align:top;'>\r\n                  <p style='margin:0 0 4px;font-weight:bold;font-size:14px;color:#333333;'>");
				handler.AppendFormatted(roleName);
				handler.AppendLiteral("</p>\r\n                  <p style='margin:0 0 8px;font-size:12px;color:#666666;'>");
				handler.AppendFormatted(attrsLine);
				handler.AppendLiteral("</p>\r\n                  <p style='margin:0;font-size:13px;color:#333333;line-height:1.5;'>");
				handler.AppendFormatted(roleDetails);
				handler.AppendLiteral("</p>\r\n                </td>\r\n              </tr>\r\n            </table>");
				stringBuilder.Append(ref handler);
			}
		}
		string seekingLine = ((!string.IsNullOrWhiteSpace(safeLocation)) ? ("<p style='margin:0 0 4px;font-size:12px;color:#aaaaaa;'>Seeking Talent From: <span style='color:#789eeb;'>" + safeLocation + "</span></p>") : "");
		string unionLine = ((!string.IsNullOrWhiteSpace(safeUnion)) ? ("<p style='margin:0 0 4px;font-size:12px;color:#aaaaaa;'>Union Status: <span style='color:#FFFFFF;'>" + safeUnion + "</span></p>") : "");
		string payLine = ((!string.IsNullOrWhiteSpace(safePay)) ? ("<p style='margin:0 0 4px;font-size:12px;color:#aaaaaa;'>Pay: <span style='color:#FFFFFF;'>" + safePay + "</span></p>") : "");
		string descBoxOne = ((!string.IsNullOrWhiteSpace(formattedDesc)) ? ("<div style='background:#ffffff;padding:14px 0;margin:0 0 16px;font-size:13px;color:#333333;line-height:1.6;'>" + formattedDesc + "</div>") : "");
		string descBox = ((!string.IsNullOrWhiteSpace(formattedDescOne)) ? ("<div style='background:#f9f9f9;border:1px solid #dddddd;padding:14px;margin:0 0 16px;font-size:13px;color:#333333;line-height:1.6;'>\r\n                  <p style='margin:0 0 10px;font-weight:bold;font-size:13px;color:#333333;'>Casting / Shoot Info</p>\r\n                  " + formattedDescOne + "\r\n                </div>") : "");
		string reasonBox = ((!string.IsNullOrWhiteSpace(safeReason)) ? ("<div style='background:#fdecea;border:1px solid #f5c6cb;border-left:5px solid #e57373;\r\n                  padding:14px;margin:0 0 16px;font-size:13px;color:#333333;line-height:1.6;'>\r\n              <p style='margin:0 0 6px;font-weight:bold;font-size:13px;color:#c62828;'>Reason for Rejection</p>\r\n              " + safeReason + "\r\n            </div>") : "");
		string greetingName = (string.IsNullOrWhiteSpace(safeDirector) ? "" : (" " + safeDirector));
		return $"<!DOCTYPE html>\r\n    <html>\r\n    <head>\r\n      <meta charset='UTF-8'/>\r\n      <meta name='viewport' content='width=device-width, initial-scale=1.0'/>\r\n    </head>\r\n    <body style='margin:0;padding:0;background:#f4f4f4;font-family:Arial,sans-serif;font-size:14px;color:#333333;'>\r\n    <table width='100%' cellpadding='0' cellspacing='0' border='0' style='background:#f4f4f4;'>\r\n    <tr><td align='center' style='padding:20px 0;'>\r\n      <table width='600' cellpadding='0' cellspacing='0' border='0'\r\n             style='background:#ffffff;border:1px solid #dddddd;'>\r\n\r\n        <!-- HEADER: Logo -->\r\n        <tr>\r\n          <td style='background:#2D2D2D;padding:0;text-align:center;'>\r\n            <img src='https://files.constantcontact.com/82886fe2301/302cc6b2-5b9a-4673-8b39-143010d6b262.jpg?rdr=true'\r\n                 alt='DirectSubmit' width='600'\r\n                 style='display:block;width:100%;max-width:600px;border:0;'/>\r\n          </td>\r\n        </tr>\r\n\r\n        <!-- RED BANNER -->\r\n        <tr>\r\n          <td style='background:#e57373;padding:10px 20px;'>\r\n            <span style='color:#ffffff;font-size:16px;font-weight:bold;'>Casting Notice Not Approved</span>\r\n          </td>\r\n        </tr>\r\n\r\n        <!-- DARK NOTICE HEADER — same as approved, title NOT linked -->\r\n        <tr>\r\n          <td style='background:#2D2D2D;padding:16px 20px;'>\r\n            <p style='margin:0 0 8px;font-size:17px;font-weight:bold;line-height:1.4;color:#ffffff;'>\r\n              {safeTitle}\r\n            </p>\r\n            {seekingLine}\r\n            {unionLine}\r\n            {payLine}\r\n            <p style='margin:0;font-size:12px;color:#aaaaaa;'>\r\n              Posted: <span style='color:#789eeb;'>{noticePosted}</span>\r\n              &nbsp;\r\n              Deadline for Submissions: <span style='color:#e53935;'>{noticeEndDate}</span>\r\n            </p>\r\n          </td>\r\n        </tr>\r\n\r\n        <!-- WHITE BODY -->\r\n        <tr>\r\n          <td style='background:#ffffff;padding:20px;font-family:Arial,sans-serif;\r\n                     font-size:14px;color:#333333;line-height:1.6;'>\r\n\r\n            <p style='margin:0 0 16px;'>Hi{greetingName},</p>\r\n\r\n            <p style='margin:0 0 16px;'>\r\n              Thank you for submitting your casting notice\r\n              <strong>&ldquo;{safeTitle}&rdquo;</strong> to DirectSubmit.\r\n            </p>\r\n\r\n            <p style='margin:0 0 16px;'>\r\n              After review, we&apos;re unable to approve this notice for publication at this time.\r\n              This can happen when a notice is missing required details, doesn&apos;t meet our\r\n              posting guidelines, or needs clarification.\r\n            </p>\r\n\r\n            {reasonBox}\r\n\r\n            <p style='margin:0 0 16px;'>\r\n              For your reference, here are the details of the notice as submitted:\r\n            </p>\r\n\r\n            {descBoxOne}\r\n            {descBox}\r\n\r\n            <p style='margin:0 0 10px;font-weight:bold;font-size:14px;color:#333333;'>\r\n              Roles in this notice:\r\n            </p>\r\n            {rolesHtml}\r\n\r\n            <p style='margin:16px 0 16px;'>\r\n              You can log in to review and update your notice, then resubmit it for approval.\r\n              If you have questions or believe this was in error, please reply to this email\r\n              and our team will be happy to help.\r\n            </p>\r\n\r\n            <p style='margin:0 0 20px;'>\r\n              <a href='{loginUrl}'\r\n                 style='background:#789eeb;color:#ffffff;padding:11px 24px;\r\n                        text-decoration:none;border-radius:4px;font-size:13px;\r\n                        font-weight:bold;display:inline-block;'>\r\n                Login to Review Your Notice\r\n              </a>\r\n            </p>\r\n\r\n            <p style='margin:0;'>\r\n              Best regards,<br/>\r\n              The DirectSubmit Team\r\n            </p>\r\n          </td>\r\n        </tr>\r\n\r\n        <!-- DIVIDER -->\r\n        <tr>\r\n          <td style='padding:0 20px;background:#ffffff;'>\r\n            <hr style='border:none;border-top:1px solid #dddddd;margin:0;'/>\r\n          </td>\r\n        </tr>\r\n\r\n        <!-- FOOTER -->\r\n        <tr>\r\n          <td style='background:#ffffff;padding:14px 20px;text-align:center;\r\n                     font-family:Arial,sans-serif;font-size:12px;color:#888888;line-height:1.6;'>\r\n            <p style='margin:0 0 4px;'>\r\n              By using the DirectSubmit platform, you agree to the terms and conditions of our\r\n              <a href='https://directsubmit.nycastings.com/terms-and-condition'\r\n                 target='_blank' style='color:#789eeb;text-decoration:underline;'>Terms of Service</a>\r\n              and\r\n              <a href='https://directsubmit.nycastings.com/privacy-policy'\r\n                 target='_blank' style='color:#789eeb;text-decoration:underline;'>Privacy Policy</a>.\r\n              &copy; DirectSubmit\r\n            </p>\r\n            <p style='margin:0;text-align:center;background:#333333;padding:10px;font-size:13px;color:#ffffff;'>\r\n              <a href='https://directsubmit.nycastings.com'\r\n                 style='color:#789eeb;text-decoration:none;'>DirectSubmit.com</a>\r\n              &nbsp;|&nbsp; 1480 Vine St. &nbsp;|&nbsp; Los Angeles, CA 90028\r\n            </p>\r\n          </td>\r\n        </tr>\r\n      </table>\r\n    </td></tr>\r\n    </table>\r\n    </body>\r\n    </html>";
		static string Enc(string s)
		{
			return WebUtility.HtmlEncode(s ?? "");
		}
		static string FormatDescription(string s)
		{
			if (string.IsNullOrWhiteSpace(s))
			{
				return "";
			}
			return Enc(s).Replace("\r\n", "<br/>").Replace("\n", "<br/>").Replace("\r", "<br/>");
		}
	}

	private string BuildNoticeApprovedEmailBody(string directorName, string noticeTitle, string noticeUnion, string noticePay, string locationDisplay, string noticePosted, string noticeEndDate, string noticeDescOne, string noticeDesc, DataTable allRoles, int noticeId)
	{
		string baseUrl = "https://directsubmit.nycastings.com";
		string dashboardUrl = baseUrl + "/dashboard";
		string returnPath = Uri.EscapeDataString($"/notice/{noticeId}");
		string noticeUrl = baseUrl + "/login?returnTo=" + returnPath;
		string safeTitle = Enc(noticeTitle);
		string safeDirector = Enc(directorName);
		string safeUnion = Enc(noticeUnion);
		string safePay = Enc(noticePay);
		string safeLocation = Enc(locationDisplay);
		string formattedDescOne = FormatDescription(noticeDescOne);
		string formattedDesc = FormatDescription(noticeDesc);
		StringBuilder rolesHtml = new StringBuilder();
		if (allRoles != null)
		{
			foreach (DataRow row in allRoles.Rows)
			{
				string roleName = Enc(row["RoleName"]?.ToString());
				string roleSex = Enc(row["RoleSex"]?.ToString());
				string roleEth = Enc(row["RoleEthnicity"]?.ToString());
				string ageStart = row["RoleAgeStart"]?.ToString() ?? "0";
				string ageEnd = row["RoleAgeEnd"]?.ToString() ?? "0";
				string roleType = Enc(row["RoleType"]?.ToString());
				string roleUnion = Enc(row["RoleUnion"]?.ToString());
				string roleDetails = Enc(row["RoleDetails"]?.ToString());
				string ageDisplay = ((ageStart != "0" && ageEnd != "0") ? ("Age: " + ageStart + "-" + ageEnd) : "");
				List<string> attrParts = new List<string>();
				if (!string.IsNullOrWhiteSpace(roleSex))
				{
					attrParts.Add(roleSex);
				}
				if (!string.IsNullOrWhiteSpace(roleEth))
				{
					attrParts.Add(roleEth);
				}
				if (!string.IsNullOrWhiteSpace(ageDisplay))
				{
					attrParts.Add(ageDisplay);
				}
				if (!string.IsNullOrWhiteSpace(roleType))
				{
					attrParts.Add("Role Type: " + roleType);
				}
				if (!string.IsNullOrWhiteSpace(roleUnion))
				{
					attrParts.Add(roleUnion);
				}
				string attrsLine = string.Join("&nbsp;&nbsp;|&nbsp;&nbsp;", attrParts);
				StringBuilder stringBuilder = rolesHtml;
				StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(783, 4, stringBuilder);
				handler.AppendLiteral("\r\n                    <table width='100%' cellpadding='0' cellspacing='0' border='0'\r\n                           style='margin:0 0 12px;border:1px solid #dddddd;'>\r\n                      <tr>\r\n                        <td style='padding:12px 14px;vertical-align:top;'>\r\n                          <p style='margin:0 0 4px;'>\r\n                            <a href='");
				handler.AppendFormatted(noticeUrl);
				handler.AppendLiteral("'\r\n                               style='color:#789eeb;text-decoration:none;font-weight:bold;font-size:14px;'>");
				handler.AppendFormatted(roleName);
				handler.AppendLiteral("</a>\r\n                          </p>\r\n                          <p style='margin:0 0 8px;font-size:12px;color:#666666;'>");
				handler.AppendFormatted(attrsLine);
				handler.AppendLiteral("</p>\r\n                          <p style='margin:0;font-size:13px;color:#333333;line-height:1.5;'>");
				handler.AppendFormatted(roleDetails);
				handler.AppendLiteral("</p>\r\n                        </td>\r\n                      </tr>\r\n                    </table>");
				stringBuilder.Append(ref handler);
			}
		}
		string seekingLine = ((!string.IsNullOrWhiteSpace(safeLocation)) ? ("<p style='margin:0 0 4px;font-size:12px;color:#aaaaaa;'>Seeking Talent From: <span style='color:#789eeb;'>" + safeLocation + "</span></p>") : "");
		string unionLine = ((!string.IsNullOrWhiteSpace(safeUnion)) ? ("<p style='margin:0 0 4px;font-size:12px;color:#aaaaaa;'>Union Status: <span style='color:#FFFFFF;'>" + safeUnion + "</span></p>") : "");
		string payLine = ((!string.IsNullOrWhiteSpace(safePay)) ? ("<p style='margin:0 0 4px;font-size:12px;color:#aaaaaa;'>Pay: <span style='color:#FFFFFF;'>" + safePay + "</span></p>") : "");
		string descBoxOne = ((!string.IsNullOrWhiteSpace(formattedDesc)) ? ("<div style='background:#ffffff;padding:14px 0;margin:0 0 16px;font-size:13px;color:#333333;line-height:1.6;'>" + formattedDesc + "</div>") : "");
		string descBox = ((!string.IsNullOrWhiteSpace(formattedDescOne)) ? ("<div style='background:#f9f9f9;border:1px solid #dddddd;padding:14px;margin:0 0 16px;font-size:13px;color:#333333;line-height:1.6;'>\r\n                  <p style='margin:0 0 10px;font-weight:bold;font-size:13px;color:#333333;'>Casting / Shoot Info</p>\r\n                  " + formattedDescOne + "\r\n                </div>") : "");
		string greetingName = (string.IsNullOrWhiteSpace(safeDirector) ? "" : (" " + safeDirector));
		return $"<!DOCTYPE html>\r\n            <html>\r\n            <head>\r\n              <meta charset='UTF-8'/>\r\n              <meta name='viewport' content='width=device-width, initial-scale=1.0'/>\r\n            </head>\r\n            <body style='margin:0;padding:0;background:#f4f4f4;font-family:Arial,sans-serif;font-size:14px;color:#333333;'>\r\n            <table width='100%' cellpadding='0' cellspacing='0' border='0' style='background:#f4f4f4;'>\r\n            <tr><td align='center' style='padding:20px 0;'>\r\n              <table width='600' cellpadding='0' cellspacing='0' border='0'\r\n                     style='background:#ffffff;border:1px solid #dddddd;'>\r\n\r\n                <!-- HEADER: Logo -->\r\n                <tr>\r\n                  <td style='background:#2D2D2D;padding:0;text-align:center;'>\r\n                    <img src='https://files.constantcontact.com/82886fe2301/302cc6b2-5b9a-4673-8b39-143010d6b262.jpg?rdr=true'\r\n                         alt='DirectSubmit' width='600'\r\n                         style='display:block;width:100%;max-width:600px;border:0;'/>\r\n                  </td>\r\n                </tr>\r\n\r\n                <!-- BLUE BANNER -->\r\n                <tr>\r\n                  <td style='background:#789eeb;padding:10px 20px;'>\r\n                    <span style='color:#ffffff;font-size:16px;font-weight:bold;'>Casting Call Approved</span>\r\n                  </td>\r\n                </tr>\r\n\r\n                <!-- DARK NOTICE HEADER -->\r\n                <tr>\r\n                  <td style='background:#2D2D2D;padding:16px 20px;'>\r\n                    <p style='margin:0 0 8px;font-size:17px;font-weight:bold;line-height:1.4;'>\r\n                      <a href='{noticeUrl}'\r\n                         style='color:#ffffff !important;text-decoration:none !important;font-size:17px;font-weight:bold;'>\r\n                        <span style='color:#ffffff;text-decoration:none;'>{safeTitle}</span>\r\n                      </a>\r\n                    </p>\r\n                    {seekingLine}\r\n                    {unionLine}\r\n                    {payLine}\r\n                    <p style='margin:0;font-size:12px;color:#aaaaaa;'>\r\n                      Posted: <span style='color:#789eeb;'>{noticePosted}</span>\r\n                      &nbsp;\r\n                      Deadline for Submissions: <span style='color:#e53935;'>{noticeEndDate}</span>\r\n                    </p>\r\n                  </td>\r\n                </tr>\r\n\r\n                <!-- WHITE BODY -->\r\n                <tr>\r\n                  <td style='background:#ffffff;padding:20px;font-family:Arial,sans-serif;\r\n                             font-size:14px;color:#333333;line-height:1.6;'>\r\n\r\n                    <p style='margin:0 0 16px;'>Hi{greetingName},</p>\r\n\r\n                    <p style='margin:0 0 16px;'>\r\n                      Great news! Your casting notice titled <strong>&ldquo;{safeTitle}&rdquo;</strong>\r\n                      has been approved and is now live on\r\n                      <a href='{baseUrl}' style='color:#789eeb;'>DirectSubmit.com</a>.\r\n                      Feel free to share this notice.\r\n                    </p>\r\n\r\n                    <p style='margin:0 0 16px;'>\r\n                      Talent can now view and submit to your project. You can review submissions\r\n                      and communicate directly with talent through your dashboard.\r\n                    </p>\r\n\r\n                    {descBoxOne}\r\n                    {descBox}\r\n\r\n                    <p style='margin:0 0 10px;font-weight:bold;font-size:14px;color:#333333;'>\r\n                      Roles in this notice:\r\n                    </p>\r\n                    {rolesHtml}\r\n\r\n                    <p style='margin:16px 0 20px;'>\r\n                      <a href='{dashboardUrl}'\r\n                         style='background:#789eeb;color:#ffffff;padding:11px 24px;\r\n                                text-decoration:none;border-radius:4px;font-size:13px;\r\n                                font-weight:bold;display:inline-block;'>\r\n                        Login to Your Dashboard\r\n                      </a>\r\n                    </p>\r\n\r\n                    <p style='margin:0;'>\r\n                      Thank you for using DirectSubmit &mdash; we&apos;re excited to help you find the right talent!<br/><br/>\r\n                      Best regards,<br/>\r\n                      The DirectSubmit Team\r\n                    </p>\r\n                  </td>\r\n                </tr>\r\n\r\n                <!-- DIVIDER -->\r\n                <tr>\r\n                  <td style='padding:0 20px;background:#ffffff;'>\r\n                    <hr style='border:none;border-top:1px solid #dddddd;margin:0;'/>\r\n                  </td>\r\n                </tr>\r\n\r\n                <!-- FOOTER -->\r\n                <tr>\r\n                  <td style='background:#ffffff;padding:14px 20px;text-align:center;\r\n                             font-family:Arial,sans-serif;font-size:12px;color:#888888;line-height:1.6;'>\r\n                    <p style='margin:0 0 4px;'>\r\n                      By using the DirectSubmit platform, you agree to the terms and conditions of our\r\n                      <a href='https://directsubmit.nycastings.com/terms-and-condition'\r\n                         target='_blank' style='color:#789eeb;text-decoration:underline;'>Terms of Service</a>\r\n                      and\r\n                      <a href='https://directsubmit.nycastings.com/privacy-policy'\r\n                         target='_blank' style='color:#789eeb;text-decoration:underline;'>Privacy Policy</a>.\r\n                      &copy; DirectSubmit\r\n                    </p>\r\n                    <p style='margin:0;text-align:center;background:#333333;padding:10px;font-size:13px;color:#ffffff;'>\r\n                      <a href='https://directsubmit.nycastings.com'\r\n                         style='color:#789eeb;text-decoration:none;'>DirectSubmit.com</a>\r\n                      &nbsp;|&nbsp; 1480 Vine St. &nbsp;|&nbsp; Los Angeles, CA 90028\r\n                    </p>\r\n                  </td>\r\n                </tr>\r\n              </table>\r\n            </td></tr>\r\n            </table>\r\n            </body>\r\n            </html>";
		static string Enc(string s)
		{
			return WebUtility.HtmlEncode(s ?? "");
		}
		static string FormatDescription(string s)
		{
			if (string.IsNullOrWhiteSpace(s))
			{
				return "";
			}
			return Enc(s).Replace("\r\n", "<br/>").Replace("\n", "<br/>").Replace("\r", "<br/>");
		}
	}

	public async Task<List<ChargebeeNewPlanModel>> GetChargebeePlansfromSite()
	{
		List<ChargebeeNewPlanModel> plans = new List<ChargebeeNewPlanModel>();
		HttpClient client = _httpFactory.CreateClient();
		string authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes(_apiKey + ":"));
		client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
		string url = "https://" + _site + ".chargebee.com/api/v2/plans?limit=100&status[is]=active";
		string offset = null;
		do
		{
			string requestUrl = ((offset == null) ? url : (url + "&offset=" + Uri.EscapeDataString(offset)));
			HttpResponseMessage obj = await client.GetAsync(requestUrl);
			obj.EnsureSuccessStatusCode();
			using JsonDocument doc = JsonDocument.Parse(await obj.Content.ReadAsStringAsync());
			foreach (JsonElement item2 in doc.RootElement.GetProperty("list").EnumerateArray())
			{
				JsonElement p = item2.GetProperty("plan");
				plans.Add(new ChargebeeNewPlanModel
				{
					PlanId = p.GetProperty("id").GetString(),
					Name = p.GetProperty("name").GetString(),
					Price = (p.TryGetProperty("price", out var pr) ? ((decimal)pr.GetInt64() / 100m) : 0m),
					CurrencyCode = (p.TryGetProperty("currency_code", out var cc) ? cc.GetString() : "USD"),
					Period = ((!p.TryGetProperty("period", out var pd)) ? 1 : pd.GetInt32()),
					PeriodUnit = (p.TryGetProperty("period_unit", out var pu) ? pu.GetString() : "month"),
					Description = (p.TryGetProperty("invoice_name", out var inv) ? inv.GetString() : "")
				});
			}
			offset = (doc.RootElement.TryGetProperty("next_offset", out var no) ? no.GetString() : null);
		}
		while (!string.IsNullOrEmpty(offset));
		return plans;
	}

	public IEnumerable<AudienceTypeModel> GetAudienceTypes()
	{
		DataTable dt = _dbManager.ReadData("USP_GET_EVENT_MAIL_AUDIENCE_TYPES", CommandType.StoredProcedure, new Dictionary<string, object>());
		List<AudienceTypeModel> list = new List<AudienceTypeModel>();
		if (dt == null)
		{
			return list;
		}
		foreach (DataRow r in dt.Rows)
		{
			list.Add(new AudienceTypeModel
			{
				AudienceTypeId = Convert.ToInt32(r["AudienceTypeId"]),
				AudienceName = r["AudienceName"].ToString(),
				Description = (r["Description"]?.ToString() ?? "")
			});
		}
		return list;
	}

	public int GetAudienceCount(int audienceTypeId)
	{
		DataTable dt = _dbManager.ReadData("USP_GET_EVENT_MAIL_AUDIENCE_COUNT", CommandType.StoredProcedure, new Dictionary<string, object> { { "@AudienceTypeId", audienceTypeId } });
		if (dt == null || dt.Rows.Count <= 0)
		{
			return 0;
		}
		return Convert.ToInt32(dt.Rows[0]["RecipientCount"]);
	}

	public int AddEvent(EventRequestModel model)
	{
		DataTable dt = _dbManager.ReadData("USP_ADD_EVENT_MAIL_EVENT", CommandType.StoredProcedure, new Dictionary<string, object>
		{
			{ "@EventName", model.EventName },
			{
				"@EventDescription",
				((object)model.EventDescription) ?? ((object)DBNull.Value)
			},
			{ "@CreatedBy", model.UserId }
		});
		if (dt == null || dt.Rows.Count <= 0)
		{
			return 0;
		}
		return Convert.ToInt32(dt.Rows[0]["EventId"]);
	}

	public bool UpdateEvent(EventRequestModel model)
	{
		DataTable dt = _dbManager.ReadData("USP_UPDATE_EVENT_MAIL_EVENT", CommandType.StoredProcedure, new Dictionary<string, object>
		{
			{ "@EventId", model.EventId },
			{ "@EventName", model.EventName },
			{
				"@EventDescription",
				((object)model.EventDescription) ?? ((object)DBNull.Value)
			}
		});
		if (dt != null && dt.Rows.Count > 0)
		{
			return Convert.ToInt32(dt.Rows[0]["RowsAffected"]) > 0;
		}
		return false;
	}

	public IEnumerable<EventModel> GetEvents()
	{
		DataTable dt = _dbManager.ReadData("USP_GET_EVENT_MAIL_EVENTS", CommandType.StoredProcedure, new Dictionary<string, object>());
		List<EventModel> list = new List<EventModel>();
		if (dt == null)
		{
			return list;
		}
		foreach (DataRow r in dt.Rows)
		{
			list.Add(new EventModel
			{
				EventId = Convert.ToInt32(r["EventId"]),
				EventName = r["EventName"].ToString(),
				EventDescription = (r["EventDescription"]?.ToString() ?? ""),
				CreatedAt = Convert.ToDateTime(r["CreatedAt"]),
				UpdatedAt = ((r["UpdatedAt"] != DBNull.Value) ? new DateTime?(Convert.ToDateTime(r["UpdatedAt"])) : ((DateTime?)null))
			});
		}
		return list;
	}

	public bool DeleteEvent(int eventId)
	{
		DataTable dt = _dbManager.ReadData("USP_DELETE_EVENT_MAIL_EVENT", CommandType.StoredProcedure, new Dictionary<string, object> { { "@EventId", eventId } });
		if (dt != null)
		{
			return dt.Rows.Count > 0;
		}
		return false;
	}

	public int AddEventMessage(EventMessageRequestModel model)
	{
		DataTable dt = _dbManager.ReadData("USP_ADD_EVENT_MAIL_MESSAGE", CommandType.StoredProcedure, new Dictionary<string, object>
		{
			{ "@EventId", model.EventId },
			{ "@AudienceTypeId", model.AudienceTypeId },
			{ "@Subject", model.Subject },
			{ "@MessageBody", model.MessageBody },
			{ "@CreatedBy", model.UserId }
		});
		if (dt == null || dt.Rows.Count <= 0)
		{
			return 0;
		}
		return Convert.ToInt32(dt.Rows[0]["EventMessageId"]);
	}

	public bool UpdateEventMessage(EventMessageRequestModel model)
	{
		DataTable dt = _dbManager.ReadData("USP_UPDATE_EVENT_MAIL_MESSAGE", CommandType.StoredProcedure, new Dictionary<string, object>
		{
			{ "@EventMessageId", model.EventMessageId },
			{ "@EventId", model.EventId },
			{ "@AudienceTypeId", model.AudienceTypeId },
			{ "@Subject", model.Subject },
			{ "@MessageBody", model.MessageBody }
		});
		if (dt != null && dt.Rows.Count > 0)
		{
			return Convert.ToInt32(dt.Rows[0]["RowsAffected"]) > 0;
		}
		return false;
	}

	public IEnumerable<EventMessageModel> GetEventMessages(int? eventId, int? audienceTypeId)
	{
		DataTable dt = _dbManager.ReadData("USP_GET_EVENT_MAIL_MESSAGES", CommandType.StoredProcedure, new Dictionary<string, object>
		{
			{
				"@EventId",
				((object)eventId) ?? DBNull.Value
			},
			{
				"@AudienceTypeId",
				((object)audienceTypeId) ?? DBNull.Value
			}
		});
		List<EventMessageModel> list = new List<EventMessageModel>();
		if (dt == null)
		{
			return list;
		}
		foreach (DataRow r in dt.Rows)
		{
			list.Add(new EventMessageModel
			{
				EventMessageId = Convert.ToInt32(r["EventMessageId"]),
				EventId = Convert.ToInt32(r["EventId"]),
				EventName = r["EventName"].ToString(),
				AudienceTypeId = Convert.ToInt32(r["AudienceTypeId"]),
				AudienceName = r["AudienceName"].ToString(),
				Subject = r["Subject"].ToString(),
				MessageBody = r["MessageBody"].ToString(),
				CreatedAt = Convert.ToDateTime(r["CreatedAt"]),
				UpdatedAt = ((r["UpdatedAt"] != DBNull.Value) ? new DateTime?(Convert.ToDateTime(r["UpdatedAt"])) : ((DateTime?)null)),
				LastSentAt = ((r["LastSentAt"] != DBNull.Value) ? new DateTime?(Convert.ToDateTime(r["LastSentAt"])) : ((DateTime?)null))
			});
		}
		return list;
	}

	public bool DeleteEventMessage(int eventMessageId)
	{
		DataTable dt = _dbManager.ReadData("USP_DELETE_EVENT_MAIL_MESSAGE", CommandType.StoredProcedure, new Dictionary<string, object> { { "@EventMessageId", eventMessageId } });
		if (dt != null)
		{
			return dt.Rows.Count > 0;
		}
		return false;
	}

	public (int QueuedCount, string Message) SendEventMail(SendEventMailRequestModel request)
	{
		string htmlBody = BuildEventEmailBody(request.Subject, request.MessageBody);
		DataTable dt = _dbManager.ReadData("USP_QUEUE_EVENT_MAIL_EMAILS", CommandType.StoredProcedure, new Dictionary<string, object>
		{
			{ "@AudienceTypeId", request.AudienceTypeId },
			{
				"@EventMessageId",
				(request.EventMessageId > 0) ? ((object)request.EventMessageId) : DBNull.Value
			},
			{ "@Subject", request.Subject },
			{ "@HtmlBody", htmlBody }
		});
		if (dt == null || dt.Rows.Count == 0)
		{
			return (QueuedCount: 0, Message: "Failed to queue emails.");
		}
		return (QueuedCount: Convert.ToInt32(dt.Rows[0]["QueuedCount"]), Message: dt.Rows[0]["Message"].ToString());
	}

	private string BuildEventEmailBody(string subject, string messageBody)
	{
		string safeSubject = WebUtility.HtmlEncode(subject ?? "");
		string paragraphs = string.Join("", from l in (messageBody ?? "").Replace("\r\n", "\n").Split('\n')
			where !string.IsNullOrWhiteSpace(l)
			select "<p style='margin:0 0 16px;'>" + l + "</p>");
		return $"<!DOCTYPE html>\r\n            <html>\r\n            <head>\r\n              <meta charset='UTF-8'/>\r\n              <meta name='viewport' content='width=device-width, initial-scale=1.0'/>\r\n            </head>\r\n            <body style='margin:0;padding:0;background:#f4f4f4;font-family:Arial,sans-serif;font-size:14px;color:#333333;'>\r\n\r\n            <table width='100%' cellpadding='0' cellspacing='0' border='0' style='background:#f4f4f4;'>\r\n            <tr><td align='center' style='padding:20px 0;'>\r\n\r\n              <table width='600' cellpadding='0' cellspacing='0' border='0'\r\n                     style='background:#ffffff;border:1px solid #dddddd;'>\r\n\r\n                <!-- HEADER -->\r\n                <tr>\r\n                  <td style='background:#333333;padding:0;text-align:center;'>\r\n                    <img src='https://files.constantcontact.com/82886fe2301/302cc6b2-5b9a-4673-8b39-143010d6b262.jpg?rdr=true'\r\n                         alt='DirectSubmit' width='600'\r\n                         style='display:block;width:100%;max-width:600px;border:0;'/>\r\n                  </td>\r\n                </tr>\r\n\r\n                <!-- BLUE BANNER -->\r\n                <tr>\r\n                  <td style='background:#789eeb;padding:14px 30px;'>\r\n                    <span style='color:#ffffff;font-size:18px;font-weight:bold;'>\r\n                      {safeSubject}\r\n                    </span>\r\n                  </td>\r\n                </tr>\r\n\r\n                <!-- BODY — plain paragraphs only -->\r\n                <tr>\r\n                  <td style='background:#ffffff;padding:28px 30px;\r\n                             font-family:Arial,sans-serif;font-size:14px;\r\n                             color:#333333;line-height:1.6;'>\r\n\r\n                    <p style='margin:0 0 16px;'></p>\r\n\r\n                    {paragraphs}\r\n\r\n                    <p style='margin:24px 0 24px;'>\r\n                      <a href='https://directsubmit.nycastings.com/login'\r\n                         style='background:#789eeb;color:#ffffff;\r\n                                padding:12px 28px;text-decoration:none;\r\n                                border-radius:4px;font-size:14px;\r\n                                font-weight:bold;display:inline-block;'>\r\n                        Login to Your Account\r\n                      </a>\r\n                    </p>\r\n\r\n                    <p style='margin:0;'>\r\n                      Thank you,<br/>\r\n                      The DirectSubmit Team\r\n                    </p>\r\n\r\n                  </td>\r\n                </tr>\r\n\r\n                <!-- DIVIDER -->\r\n                <tr>\r\n                  <td style='padding:0 30px;background:#ffffff;'>\r\n                    <hr style='border:none;border-top:1px solid #dddddd;margin:0;'/>\r\n                  </td>\r\n                </tr>\r\n\r\n                <!-- FOOTER -->\r\n                <tr>\r\n                  <td style='background:#ffffff;padding:14px 20px;text-align:center;\r\n                             font-family:Arial,sans-serif;font-size:12px;color:#888888;line-height:1.6;'>\r\n                    <p style='margin:0 0 4px;'>\r\n                      By using the DirectSubmit platform, you agree to the terms and conditions of our\r\n                      <a href='https://directsubmit.nycastings.com/terms-and-condition'\r\n                         target='_blank' style='color:#4d90fe;text-decoration:underline;'>Terms of Service</a>\r\n                      and\r\n                      <a href='https://directsubmit.nycastings.com/privacy-policy'\r\n                         target='_blank' style='color:#4d90fe;text-decoration:underline;'>Privacy Policy</a>.\r\n                      &copy; DirectSubmit\r\n                    </p>\r\n                    <p style='margin:0;text-align:center;background:#333333;padding:10px;font-size:13px;color:#ffffff;'>\r\n                      <a href='https://directsubmit.nycastings.com'\r\n                         style='color:#4d90fe;text-decoration:none;'>DirectSubmit.com</a>\r\n                      &nbsp;|&nbsp; 1480 Vine St. &nbsp;|&nbsp; Los Angeles, CA 90028\r\n                    </p>\r\n                  </td>\r\n                </tr>\r\n\r\n              </table>\r\n\r\n            </td></tr>\r\n            </table>\r\n            </body>\r\n            </html>";
	}

	public IEnumerable<BulkEmailBatchModel> GetBulkEmailBatches(DateTime? fromDate, DateTime? toDate)
	{
		DataTable dt = _dbManager.ReadData("USP_GET_BULK_EMAIL_BATCHES", CommandType.StoredProcedure, new Dictionary<string, object>
		{
			{
				"@FromDate",
				((object)fromDate) ?? DBNull.Value
			},
			{
				"@ToDate",
				((object)toDate) ?? DBNull.Value
			}
		});
		List<BulkEmailBatchModel> list = new List<BulkEmailBatchModel>();
		if (dt == null || dt.Rows.Count == 0)
		{
			return list;
		}
		foreach (DataRow r in dt.Rows)
		{
			list.Add(new BulkEmailBatchModel
			{
				BatchType = r["BatchType"].ToString(),
				NoticeId = ((r["NoticeId"] != DBNull.Value) ? new int?(Convert.ToInt32(r["NoticeId"])) : ((int?)null)),
				NoticeTitle = ((r["NoticeTitle"] != DBNull.Value) ? r["NoticeTitle"].ToString() : null),
				Subject = ((r["Subject"] != DBNull.Value) ? r["Subject"].ToString() : null),
				QueuedAt = Convert.ToDateTime(r["QueuedAt"]),
				TotalEmails = Convert.ToInt32(r["TotalEmails"]),
				SentCount = Convert.ToInt32(r["SentCount"]),
				PendingCount = Convert.ToInt32(r["PendingCount"]),
				FailedCount = Convert.ToInt32(r["FailedCount"]),
				LastSentAt = ((r["LastSentAt"] != DBNull.Value) ? new DateTime?(Convert.ToDateTime(r["LastSentAt"])) : ((DateTime?)null))
			});
		}
		return list;
	}

	public BulkEmailDetailResponse GetBulkEmailDetails(int? noticeId, bool eventOnly, string subject, string status, string searchEmail, DateTime? fromDate, DateTime? toDate, int page, int pageSize)
	{
		DataTable dt = _dbManager.ReadData("USP_GET_BULK_EMAIL_DETAILS", CommandType.StoredProcedure, new Dictionary<string, object>
		{
			{
				"@NoticeId",
				((object)noticeId) ?? DBNull.Value
			},
			{
				"@EventOnly",
				eventOnly ? 1 : 0
			},
			{
				"@Subject",
				string.IsNullOrWhiteSpace(subject) ? ((IConvertible)DBNull.Value) : ((IConvertible)subject.Trim())
			},
			{
				"@Status",
				string.IsNullOrWhiteSpace(status) ? ((IConvertible)DBNull.Value) : ((IConvertible)status.Trim())
			},
			{
				"@SearchEmail",
				string.IsNullOrWhiteSpace(searchEmail) ? ((IConvertible)DBNull.Value) : ((IConvertible)searchEmail.Trim())
			},
			{
				"@FromDate",
				((object)fromDate) ?? DBNull.Value
			},
			{
				"@ToDate",
				((object)toDate) ?? DBNull.Value
			},
			{ "@Page", page },
			{ "@PageSize", pageSize }
		});
		BulkEmailDetailResponse response = new BulkEmailDetailResponse
		{
			Page = page,
			PageSize = pageSize
		};
		if (dt == null || dt.Rows.Count == 0)
		{
			return response;
		}
		foreach (DataRow r in dt.Rows)
		{
			response.TotalCount = Convert.ToInt32(r["TotalCount"]);
			response.Items.Add(new BulkEmailDetailModel
			{
				Id = Convert.ToInt64(r["Id"]),
				NoticeId = ((r["NoticeId"] != DBNull.Value) ? new int?(Convert.ToInt32(r["NoticeId"])) : ((int?)null)),
				RoleId = ((r["RoleId"] != DBNull.Value) ? new int?(Convert.ToInt32(r["RoleId"])) : ((int?)null)),
				ToEmail = r["ToEmail"].ToString(),
				Subject = r["Subject"].ToString(),
				Status = r["Status"].ToString(),
				RetryCount = ((r["RetryCount"] != DBNull.Value) ? Convert.ToInt32(r["RetryCount"]) : 0),
				CreatedAt = Convert.ToDateTime(r["CreatedAt"]),
				ProcessedAt = ((r["ProcessedAt"] != DBNull.Value) ? new DateTime?(Convert.ToDateTime(r["ProcessedAt"])) : ((DateTime?)null)),
				SentAt = ((r["SentAt"] != DBNull.Value) ? new DateTime?(Convert.ToDateTime(r["SentAt"])) : ((DateTime?)null)),
				ErrorMessage = ((r["ErrorMessage"] != DBNull.Value) ? r["ErrorMessage"].ToString() : null)
			});
		}
		return response;
	}
}

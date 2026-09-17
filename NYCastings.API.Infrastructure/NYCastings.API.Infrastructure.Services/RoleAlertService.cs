using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NYCasting.Core.Models;
using NYCasting.Infrastructure.DataAccess;

namespace NYCastings.API.Infrastructure.Services;

public class RoleAlertService
{
	private readonly DbManager _dbManager;

	private readonly EmailService _emailService;

	private readonly IHttpContextAccessor _httpContextAccessor;

	public RoleAlertService(IOptions<ConnectionString> dbConfig, IHttpContextAccessor httpContextAccessor)
	{
		_dbManager = new DbManager(dbConfig.Value.NYCasting);
		_emailService = new EmailService(dbConfig);
		_httpContextAccessor = httpContextAccessor;
	}

	public int QueueEmailsForNotice(int noticeId)
	{
		_ = _httpContextAccessor.HttpContext?.Request;
		string baseUrl = "https://directsubmit.nycastings.com";
		string apiUrl = "https://api-dev.directsubmit.com";
		DataTable dt = _dbManager.ReadData("USP_GET_ROLE_ALERT_MATCHED_USERS", CommandType.StoredProcedure, new Dictionary<string, object> { { "@NoticeId", noticeId } });
		if (dt == null || dt.Rows.Count == 0)
		{
			return 0;
		}
		DataTable allRolesDt = _dbManager.ReadData("USP_GET_ALL_ROLES_FOR_NOTICE", CommandType.StoredProcedure, new Dictionary<string, object> { { "@NoticeId", noticeId } });
		DataRow firstRow = dt.Rows[0];
		string noticeTitle = firstRow["NoticeTitle"]?.ToString() ?? "";
		string directorName = firstRow["DirectorName"]?.ToString() ?? "";
		string noticeUnion = firstRow["NoticeUnion"]?.ToString() ?? "";
		noticeUnion = string.Join(", ", from u in noticeUnion.Split(',')
			select u.Trim());
		string noticePay = firstRow["NoticePay"]?.ToString() ?? "";
		string noticeDesc = firstRow["NoticeDescription"]?.ToString() ?? "";
		string noticeDescOne = firstRow["NoticeDescriptionOne"]?.ToString() ?? "";
		string locationDisplay = firstRow["NoticeLocationDisplay"]?.ToString() ?? "";
		string noticePosted = ((firstRow["NoticePosted"] != DBNull.Value) ? Convert.ToDateTime(firstRow["NoticePosted"]).ToString("MM/dd/yyyy") : "");
		string noticeEndDate = ((firstRow["NoticeEndDate"] != DBNull.Value) ? Convert.ToDateTime(firstRow["NoticeEndDate"]).ToString("MM/dd/yyyy") : "TBD");
		string subject = "Role Alert: " + noticeTitle;
		Dictionary<string, HashSet<int>> emailMatchedRoles = new Dictionary<string, HashSet<int>>(StringComparer.OrdinalIgnoreCase);
		Dictionary<string, int> emailToUserId = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
		foreach (DataRow row in dt.Rows)
		{
			string primary = row["UserEmail"]?.ToString()?.Trim() ?? "";
			string secondary = row["UserEmail2"]?.ToString()?.Trim() ?? "";
			int roleId = ((row["RoleId"] != DBNull.Value) ? Convert.ToInt32(row["RoleId"]) : 0);
			int roleAlert = ((row["RoleAlert"] != DBNull.Value) ? Convert.ToInt32(row["RoleAlert"]) : 0);
			int userId = ((row["UserId"] != DBNull.Value) ? Convert.ToInt32(row["UserId"]) : 0);
			if (!string.IsNullOrWhiteSpace(primary))
			{
				if (!emailMatchedRoles.ContainsKey(primary))
				{
					emailMatchedRoles[primary] = new HashSet<int>();
				}
				if (!emailToUserId.ContainsKey(primary))
				{
					emailToUserId[primary] = userId;
				}
				if (roleId > 0 && roleAlert == 1)
				{
					emailMatchedRoles[primary].Add(roleId);
				}
			}
			if (!string.IsNullOrWhiteSpace(secondary) && !secondary.Equals(primary, StringComparison.OrdinalIgnoreCase))
			{
				if (!emailMatchedRoles.ContainsKey(secondary))
				{
					emailMatchedRoles[secondary] = new HashSet<int>();
				}
				if (!emailToUserId.ContainsKey(secondary))
				{
					emailToUserId[secondary] = userId;
				}
				if (roleId > 0 && roleAlert == 1)
				{
					emailMatchedRoles[secondary].Add(roleId);
				}
			}
		}
		if (emailMatchedRoles.Count == 0)
		{
			return 0;
		}
		int queued = 0;
		foreach (KeyValuePair<string, HashSet<int>> kvp in emailMatchedRoles)
		{
			string toEmail = kvp.Key;
			HashSet<int> matchedRoles = kvp.Value;
			int userId2 = (emailToUserId.TryGetValue(toEmail, out var uid) ? uid : 0);
			string returnPath = Uri.EscapeDataString($"/notice/{noticeId}");
			string noticeUrl = baseUrl + "/login?returnTo=" + returnPath;
			string unsubscribeUrl = $"{apiUrl}/api/User/UnsubscribeUser?userId={userId2}&email={Uri.EscapeDataString(toEmail)}";
			string body = BuildRoleAlertEmailBody(noticeUrl, noticeTitle, directorName, locationDisplay, noticeUnion, noticePay, noticePosted, noticeEndDate, noticeDesc, noticeDescOne, allRolesDt, matchedRoles, baseUrl, unsubscribeUrl);
			if (_dbManager.InsertOrUpdateData("USP_EMAILQUEUE_INSERT", CommandType.StoredProcedure, new Dictionary<string, object>
			{
				{ "@NoticeId", noticeId },
				{
					"@RoleId",
					DBNull.Value
				},
				{ "@ToEmail", toEmail },
				{ "@Subject", subject },
				{ "@Body", body }
			}))
			{
				queued++;
			}
		}
		return queued;
	}

	private string BuildRoleAlertEmailBody(string noticeUrl, string noticeTitle, string directorName, string locationDisplay, string noticeUnion, string noticePay, string noticePosted, string noticeEndDate, string noticeDesc, string noticeDescOne, DataTable allRoles, HashSet<int> matchedRoleIds, string baseUrl, string unsubscribeUrl)
	{
		StringBuilder rolesHtml = new StringBuilder();
		if (allRoles != null)
		{
			foreach (DataRow r in allRoles.Rows)
			{
				int roleId = ((r["RoleId"] != DBNull.Value) ? Convert.ToInt32(r["RoleId"]) : 0);
				string roleName = r["RoleName"]?.ToString() ?? "";
				string roleSex = r["RoleSex"]?.ToString() ?? "";
				string roleEth = r["RoleEthnicity"]?.ToString() ?? "";
				string ageStart = r["RoleAgeStart"]?.ToString() ?? "0";
				string ageEnd = r["RoleAgeEnd"]?.ToString() ?? "0";
				string roleType = r["RoleType"]?.ToString() ?? "";
				string roleUnion = r["RoleUnion"]?.ToString() ?? "";
				string roleDetails = r["RoleDetails"]?.ToString() ?? "";
				string ageDisplay = ((ageStart != "0" && ageEnd != "0") ? ("Age: " + ageStart + "-" + ageEnd) : "");
				bool num = matchedRoleIds.Contains(roleId);
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
				if (num)
				{
					StringBuilder stringBuilder = rolesHtml;
					StringBuilder stringBuilder2 = stringBuilder;
					StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(1365, 4, stringBuilder);
					handler.AppendLiteral("\r\n                        <table width='100%' cellpadding='0' cellspacing='0' border='0'\r\n                               style='margin:0 0 12px;border:1px solid #dddddd;border-left:5px solid #4CAF50;'>\r\n                          <tr>\r\n                            <td style='padding:12px 14px;vertical-align:top;'>\r\n                              <p style='margin:0 0 4px;'>\r\n                                <a href='");
					handler.AppendFormatted(noticeUrl);
					handler.AppendLiteral("'\r\n                                   style='color:#789eeb;text-decoration:none;font-weight:bold;font-size:14px;'>\r\n                                  ");
					handler.AppendFormatted(roleName);
					handler.AppendLiteral("\r\n                                </a>\r\n                                &nbsp;\r\n                                <span style='display:inline-block;background:#4CAF50;color:#ffffff;\r\n                                             font-size:9px;font-weight:bold;padding:2px 6px;\r\n                                             border-radius:3px;vertical-align:middle;letter-spacing:1px;'>\r\n                                  ROLE ALERT\r\n                                </span>\r\n                              </p>\r\n                              <p style='margin:0 0 8px;font-size:12px;color:#666666;'>");
					handler.AppendFormatted(attrsLine);
					handler.AppendLiteral("</p>\r\n                              <p style='margin:0;font-size:13px;color:#333333;line-height:1.5;'>");
					handler.AppendFormatted(roleDetails);
					handler.AppendLiteral("</p>\r\n                            </td>\r\n                          </tr>\r\n                        </table>");
					stringBuilder2.Append(ref handler);
				}
				else
				{
					StringBuilder stringBuilder = rolesHtml;
					StringBuilder stringBuilder3 = stringBuilder;
					StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(754, 3, stringBuilder);
					handler.AppendLiteral("\r\n                        <table width='100%' cellpadding='0' cellspacing='0' border='0'\r\n                               style='margin:0 0 12px;border:1px solid #dddddd;'>\r\n                          <tr>\r\n                            <td style='padding:12px 14px;vertical-align:top;'>\r\n                              <p style='margin:0 0 4px;font-weight:bold;font-size:14px;color:#333333;'>\r\n                                ");
					handler.AppendFormatted(roleName);
					handler.AppendLiteral("\r\n                              </p>\r\n                              <p style='margin:0 0 8px;font-size:12px;color:#666666;'>");
					handler.AppendFormatted(attrsLine);
					handler.AppendLiteral("</p>\r\n                              <p style='margin:0;font-size:13px;color:#333333;line-height:1.5;'>");
					handler.AppendFormatted(roleDetails);
					handler.AppendLiteral("</p>\r\n                            </td>\r\n                          </tr>\r\n                        </table>");
					stringBuilder3.Append(ref handler);
				}
			}
		}
		string seekingLine = ((!string.IsNullOrWhiteSpace(locationDisplay)) ? ("<p style='margin:0 0 4px;font-size:12px;color:#aaaaaa;'>Seeking Talent From: <span style='color:#789eeb;'>" + locationDisplay + "</span></p>") : "");
		string unionLine = ((!string.IsNullOrWhiteSpace(noticeUnion)) ? ("<p style='margin:0 0 4px;font-size:12px;color:#aaaaaa;'>Union Status: <span style='color:#FFFFFF;'>" + noticeUnion + "</span></p>") : "");
		string payLine = ((!string.IsNullOrWhiteSpace(noticePay)) ? ("<p style='margin:0 0 4px;font-size:12px;color:#aaaaaa;'>Pay: <span style='color:#FFFFFF;'>" + noticePay + "</span></p>") : "");
		string formattedDescOne = FormatDescription(noticeDescOne);
		string formattedDesc = FormatDescription(noticeDesc);
		string descBoxOne = ((!string.IsNullOrWhiteSpace(formattedDescOne)) ? ("<div style='background:#ffffff;\r\n                   padding:14px 0;margin:0 0 16px;\r\n                   font-size:13px;color:#333333;line-height:1.6;'>\r\n                   " + formattedDescOne + "\r\n                 </div>") : "");
		string descBox = ((!string.IsNullOrWhiteSpace(formattedDesc)) ? ("<div style='background:#f9f9f9;border:1px solid #dddddd;\r\n               padding:14px;margin:0 0 16px;\r\n               font-size:13px;color:#333333;line-height:1.6;'>\r\n               <p style='margin:0 0 10px;font-weight:bold;font-size:13px;color:#333333;'>\r\n                   Casting / Shoot Info\r\n               </p>\r\n               " + formattedDesc + "\r\n                </div>") : "");
		return $"<!DOCTYPE html>\r\n            <html>\r\n            <head>\r\n              <meta charset='UTF-8'/>\r\n              <meta name='viewport' content='width=device-width, initial-scale=1.0'/>\r\n            </head>\r\n            <body style='margin:0;padding:0;background:#f4f4f4;font-family:Arial,sans-serif;font-size:14px;color:#333333;'>\r\n            <table width='100%' cellpadding='0' cellspacing='0' border='0' style='background:#f4f4f4;'>\r\n            <tr><td align='center' style='padding:20px 0;'>\r\n              <table width='600' cellpadding='0' cellspacing='0' border='0'\r\n                     style='background:#ffffff;border:1px solid #dddddd;'>\r\n\r\n                <!-- HEADER: Logo -->\r\n                <tr>\r\n                  <td style='background:#333333;padding:0;text-align:center;'>\r\n                    <img src='https://files.constantcontact.com/82886fe2301/302cc6b2-5b9a-4673-8b39-143010d6b262.jpg?rdr=true'\r\n                         alt='DirectSubmit' width='600'\r\n                         style='display:block;width:100%;max-width:600px;border:0;'/>\r\n                  </td>\r\n                </tr>\r\n\r\n                <!-- BLUE BANNER -->\r\n                <tr>\r\n                  <td style='background:#789eeb;padding:10px 20px;'>\r\n                    <span style='color:#ffffff;font-size:16px;font-weight:bold;'>Role Alert!</span>\r\n                  </td>\r\n                </tr>\r\n\r\n                <!-- DARK NOTICE HEADER -->\r\n                <tr>\r\n                  <td style='background:#2D2D2D;padding:16px 20px;'>\r\n                    <p style='margin:0 0 8px;font-size:17px;font-weight:bold;line-height:1.4;'>\r\n                      <a href='{noticeUrl}' style='color:#ffffff;text-decoration:none;'>{noticeTitle}</a>\r\n                    </p>\r\n                    {seekingLine}\r\n                    {unionLine}\r\n                    {payLine}        \r\n                    <p style='margin:0;font-size:12px;color:#aaaaaa;'>\r\n                      Posted: <span style='color:#789eeb;'>{noticePosted}</span>\r\n                      &nbsp;\r\n                      Deadline for Submissions: <span style='color:#e53935;'>{noticeEndDate}</span>\r\n                    </p>\r\n                  </td>\r\n                </tr>\r\n\r\n                <!-- WHITE BODY -->\r\n                <tr>\r\n                  <td style='background:#ffffff;padding:20px;font-family:Arial,sans-serif;\r\n                             font-size:14px;color:#333333;line-height:1.6;'>\r\n                    {descBoxOne}\r\n                    {descBox}\r\n                    {rolesHtml}\r\n                  </td>\r\n                </tr>\r\n\r\n                <!-- DIVIDER -->\r\n                <tr>\r\n                  <td style='padding:0 20px;background:#ffffff;'>\r\n                    <hr style='border:none;border-top:1px solid #dddddd;margin:0;'/>\r\n                  </td>\r\n                </tr>\r\n\r\n                <!-- FOOTER TEXT -->\r\n                <tr>\r\n                  <td style='background:#ffffff;padding:16px 20px;font-family:Arial,sans-serif;\r\n                             font-size:13px;color:#555555;line-height:1.6;'>\r\n                    <p style='margin:0 0 10px;'>\r\n                      You are receiving this Role Alert because it matches the preferences you selected in your\r\n                      <a href='{baseUrl}/castingcalls' style='color:#789eeb;'>Role Alerts settings</a>\r\n                      on DirectSubmit.\r\n                    </p>\r\n                    <p style='margin:0 0 10px;'>\r\n                      Not every alert will be a perfect fit, as casting roles often include many specific\r\n                      details beyond basic criteria like age, gender, location, and ethnicity.\r\n                    </p>\r\n                    <p style='margin:0 0 16px;'>\r\n                      You can edit or cancel your Role Alerts preferences on the\r\n                      <a href='{baseUrl}/castingcalls'\r\n                         target='_blank' style='color:#789eeb;'>Casting Calls</a>\r\n                      page under the <strong>Get Role Alerts By Email</strong> section.\r\n                    </p>\r\n                    <p style='margin:0 0 20px;'>\r\n                      <a href='{noticeUrl}'\r\n                       style='background:#789eeb;color:#ffffff;padding:11px 24px;\r\n                              text-decoration:none;border-radius:4px;font-size:13px;\r\n                              font-weight:bold;display:inline-block;'>\r\n                      View Full Notice &amp; Submit\r\n                    </a>\r\n                    </p>\r\n                  </td>\r\n                </tr>\r\n\r\n                <!-- DIVIDER -->\r\n                <tr>\r\n                  <td style='padding:0 20px;background:#ffffff;'>\r\n                    <hr style='border:none;border-top:1px solid #dddddd;margin:0;'/>\r\n                  </td>\r\n                </tr>\r\n\r\n               <!-- FOOTER -->\r\n                    <tr>\r\n                      <td style='background:#ffffff;padding:14px 20px;text-align:center;\r\n                                 font-family:Arial,sans-serif;font-size:12px;color:#888888;line-height:1.6;'>\r\n                        <p style='margin:0 0 4px;'>\r\n                          By using the DirectSubmit platform, you agree to the terms and conditions of our\r\n                          <a href='https://directsubmit.nycastings.com/terms-and-condition'\r\n                             target='_blank' style='color:#789eeb;text-decoration:underline;'>Terms of Service</a>\r\n                          and\r\n                          <a href='https://directsubmit.nycastings.com/privacy-policy'\r\n                             target='_blank' style='color:#789eeb;text-decoration:underline;'>Privacy Policy</a>.\r\n                          &copy; DirectSubmit\r\n                        </p>\r\n                        <p style='margin:0;text-align:center;background:#333333;padding:10px;font-size:13px;color:#ffffff;'>\r\n                          <a href='https://directsubmit.nycastings.com'\r\n                             style='color:#789eeb;text-decoration:none;'>DirectSubmit.com</a>\r\n                          &nbsp;|&nbsp; 1480 Vine St. &nbsp;|&nbsp; Los Angeles, CA 90028\r\n                        </p>\r\n                        <p style='margin:8px 0 0;text-align:center;font-size:11px;color:#aaaaaa;'>\r\n                          <a href='{unsubscribeUrl}'\r\n                             style='color:#aaaaaa;text-decoration:underline;'>Unsubscribe from Role Alert emails</a>\r\n                        </p>\r\n                      </td>\r\n                    </tr>\r\n\r\n              </table>\r\n            </td></tr>\r\n            </table>\r\n            </body>\r\n            </html>";
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
}

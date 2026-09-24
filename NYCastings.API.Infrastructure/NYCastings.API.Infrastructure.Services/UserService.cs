using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NYCasting.Core.Models;
using NYCasting.Infrastructure.DataAccess;
using NYCastings.API.Core.Contracts.JobCategoryModel;
using NYCastings.API.Core.Contracts.UserInterface;
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

namespace NYCastings.API.Infrastructure.Services;

public class UserService : BaseApiService, IUserService
{
	private readonly DbManager _dbManager;

	private readonly string _jwtSecret;

	private readonly EmailService _emailService;

	public UserService(IOptions<ConnectionString> dbConfig, IConfiguration configuration)
		: base(dbConfig)
	{
		if (dbConfig == null || string.IsNullOrWhiteSpace(dbConfig.Value.NYCasting))
		{
			throw new ArgumentNullException("Connection string for NYCasting is missing.");
		}
		_dbManager = new DbManager(dbConfig.Value.NYCasting);
		_jwtSecret = configuration["Jwt:Key"] ?? throw new ArgumentNullException("Jwt:Key is missing in configuration.");
		_emailService = new EmailService(dbConfig, configuration);
	}

	public bool AddUserDetails(AddUserDetails userDetails)
	{
		if (string.IsNullOrWhiteSpace(userDetails.Password))
		{
			userDetails.Password = GenerateRandomPassword();
		}
		string procedureName = "ADD_NEWUSER";
		Dictionary<string, object> parameters = TakeUserDataFields(userDetails);
		bool num = _dbManager.InsertOrUpdateData(procedureName, CommandType.StoredProcedure, parameters);
		if (num)
		{
			SendWelcomeEmail(userDetails);
		}
		return num;
	}

	public void SendWelcomeEmail(AddUserDetails userDetails)
	{
		if (userDetails.AccountType == 2)
		{
			string subject = "Welcome to DirectSubmit - Booking more work just became a lot easier!";
			string body = $"<!DOCTYPE html>\r\n        <html>\r\n        <head>\r\n          <meta charset='UTF-8'/>\r\n          <meta name='viewport' content='width=device-width, initial-scale=1.0'/>\r\n        </head>\r\n        <body style='margin:0;padding:0;background:#f4f4f4;font-family:Arial,sans-serif;font-size:14px;color:#333333;'>\r\n\r\n        <table width='100%' cellpadding='0' cellspacing='0' border='0' style='background:#f4f4f4;'>\r\n        <tr><td align='center' style='padding:20px 0;'>\r\n\r\n          <table width='600' cellpadding='0' cellspacing='0' border='0'\r\n                 style='background:#ffffff;border:1px solid #dddddd;'>\r\n\r\n            <!-- HEADER -->\r\n            <tr>\r\n              <td style='background:#333333;padding:0;text-align:center;'>\r\n                <img src='https://files.constantcontact.com/82886fe2301/302cc6b2-5b9a-4673-8b39-143010d6b262.jpg?rdr=true'\r\n                     alt='DirectSubmit' width='600'\r\n                     style='display:block;width:100%;max-width:600px;border:0;'/>\r\n              </td>\r\n            </tr>\r\n\r\n            <!-- BLUE BANNER -->\r\n            <tr>\r\n              <td style='background:#789eeb;padding:14px 30px;'>\r\n                <span style='color:#ffffff;font-size:18px;font-weight:bold;'>\r\n                  Welcome to DirectSubmit!\r\n                </span>\r\n              </td>\r\n            </tr>\r\n\r\n            <!-- BODY -->\r\n            <tr>\r\n              <td style='background:#ffffff;padding:28px 30px;\r\n                         font-family:Arial,sans-serif;font-size:14px;\r\n                         color:#333333;line-height:1.6;'>\r\n\r\n                <p style='margin:0 0 16px;'>\r\n                  Congratulations {userDetails.FirstName} {userDetails.LastName},\r\n                </p>\r\n\r\n                <p style='margin:0 0 16px;'>\r\n                  We want to welcome you to <strong>DirectSubmit</strong> and we\r\n                  sincerely hope that it will help you book more work!\r\n                </p>\r\n\r\n                <p style='margin:0 0 8px;'>Your talent info was received:</p>\r\n\r\n                <p style='margin:0 0 16px;'>\r\n                  <strong>Talent Name:</strong> {userDetails.FirstName} {userDetails.LastName}<br/>\r\n                  <strong>User:</strong> {userDetails.UserName}<br/>\r\n                  <strong>Password:</strong> {userDetails.Password}<br/>\r\n                  <strong>Location Code:</strong> {userDetails.State}\r\n                </p>\r\n\r\n                <p style='margin:0 0 16px;'>\r\n                  Make sure that you fill out your profile right away and\r\n                  start submitting!\r\n                </p>\r\n\r\n                <p style='margin:0 0 24px;'>\r\n                  <a href='https://directsubmit.nycastings.com/login'\r\n                     style='background:#789eeb;color:#ffffff;\r\n                            padding:12px 28px;text-decoration:none;\r\n                            border-radius:4px;font-size:14px;\r\n                            font-weight:bold;display:inline-block;'>\r\n                    Login to Your Account\r\n                  </a>\r\n                </p>\r\n\r\n                <p style='margin:0;'>\r\n                  Good luck with your career!<br/>\r\n                  Your Team @ DirectSubmit\r\n                </p>\r\n\r\n              </td>\r\n            </tr>\r\n\r\n            <!-- DIVIDER -->\r\n            <tr>\r\n              <td style='padding:0 30px;background:#ffffff;'>\r\n                <hr style='border:none;border-top:1px solid #dddddd;margin:0;'/>\r\n              </td>\r\n            </tr>\r\n\r\n            <!-- FOOTER -->\r\n            <tr>\r\n              <td style='background:#ffffff;padding:14px 20px;text-align:center;\r\n                         font-family:Arial,sans-serif;font-size:12px;color:#888888;line-height:1.6;'>\r\n                <p style='margin:0 0 4px;'>\r\n                  By using the DirectSubmit platform, you agree to the terms and conditions of our\r\n                  <a href='https://directsubmit.nycastings.com/terms-and-condition'\r\n                     target='_blank' style='color:#789eeb;text-decoration:underline;'>Terms of Service</a>\r\n                  and\r\n                  <a href='https://directsubmit.nycastings.com/privacy-policy'\r\n                     target='_blank' style='color:#789eeb;text-decoration:underline;'>Privacy Policy</a>.\r\n                  &copy; DirectSubmit\r\n                </p>\r\n                <p style='margin:0;text-align:center;background:#333333;padding:10px;font-size:13px;color:#ffffff;'>\r\n                  <a href='https://directsubmit.nycastings.com'\r\n                     style='color:#789eeb;text-decoration:none;'>DirectSubmit.com</a>\r\n                  &nbsp;|&nbsp; 1480 Vine St. &nbsp;|&nbsp; Los Angeles, CA 90028\r\n                </p>\r\n              </td>\r\n            </tr>\r\n\r\n          </table>\r\n\r\n        </td></tr>\r\n        </table>\r\n        </body>\r\n        </html>";
			_emailService.SendTalentWelcomeEmail(userDetails?.Email ?? string.Empty, subject, body);
		}
		else if (userDetails.AccountType == 9)
		{
			string subject2 = "Welcome to DirectSubmit – Your Journey as a Director Begins!";
			string body2 = $"<!DOCTYPE html>\r\n        <html>\r\n        <head>\r\n          <meta charset='UTF-8'/>\r\n          <meta name='viewport' content='width=device-width, initial-scale=1.0'/>\r\n        </head>\r\n        <body style='margin:0;padding:0;background:#f4f4f4;font-family:Arial,sans-serif;font-size:14px;color:#333333;'>\r\n\r\n        <table width='100%' cellpadding='0' cellspacing='0' border='0' style='background:#f4f4f4;'>\r\n        <tr><td align='center' style='padding:20px 0;'>\r\n\r\n          <table width='600' cellpadding='0' cellspacing='0' border='0'\r\n                 style='background:#ffffff;border:1px solid #dddddd;'>\r\n\r\n            <!-- HEADER -->\r\n            <tr>\r\n              <td style='background:#333333;padding:0;text-align:center;'>\r\n                <img src='https://files.constantcontact.com/82886fe2301/302cc6b2-5b9a-4673-8b39-143010d6b262.jpg?rdr=true'\r\n                     alt='DirectSubmit' width='600'\r\n                     style='display:block;width:100%;max-width:600px;border:0;'/>\r\n              </td>\r\n            </tr>\r\n\r\n            <!-- BLUE BANNER -->\r\n            <tr>\r\n              <td style='background:#789eeb;padding:14px 30px;'>\r\n                <span style='color:#ffffff;font-size:18px;font-weight:bold;'>\r\n                  Welcome to DirectSubmit – Your Journey as a Director Begins!\r\n                </span>\r\n              </td>\r\n            </tr>\r\n\r\n            <!-- BODY -->\r\n            <tr>\r\n              <td style='background:#ffffff;padding:28px 30px;\r\n                         font-family:Arial,sans-serif;font-size:14px;\r\n                         color:#333333;line-height:1.6;'>\r\n\r\n                <p style='margin:0 0 16px;'>\r\n                  Dear {userDetails.FirstName} {userDetails.LastName},\r\n                </p>\r\n\r\n                <p style='margin:0 0 16px;'>\r\n                  We are excited to welcome you to <strong>DirectSubmit</strong> as a Director!\r\n                  We believe that your new role will open up a world of exciting opportunities,\r\n                  and we are thrilled to have you as part of our community.\r\n                </p>\r\n\r\n                <p style='margin:0 0 8px;'>Your Director account details are as follows:</p>\r\n\r\n                <p style='margin:0 0 16px;'>\r\n                  <strong>Talent Name:</strong> {userDetails.FirstName} {userDetails.LastName}<br/>\r\n                  <strong>User:</strong> {userDetails.UserName}<br/>\r\n                  <strong>Password:</strong> {userDetails.Password}<br/>\r\n                  <strong>Location Code:</strong> {userDetails.State}\r\n                </p>\r\n\r\n                <p style='margin:0 0 16px;'>\r\n                  As a Director, you now have access to a range of tools and features\r\n                  that will help you find the perfect talent for your projects.\r\n                  We&apos;re excited to see how you&apos;ll take advantage of these\r\n                  resources to grow and succeed.\r\n                </p>\r\n\r\n                <p style='margin:0 0 24px;'>\r\n                  We wish you the best of luck on your journey with DirectSubmit,\r\n                  and we are here to support you every step of the way.\r\n                  Your next big opportunity is just around the corner!\r\n                </p>\r\n\r\n                <p style='margin:0 0 24px;'>\r\n                  <a href='https://directsubmit.nycastings.com/login'\r\n                     style='background:#789eeb;color:#ffffff;\r\n                            padding:12px 28px;text-decoration:none;\r\n                            border-radius:4px;font-size:14px;\r\n                            font-weight:bold;display:inline-block;'>\r\n                    Login to Your Account\r\n                  </a>\r\n                </p>\r\n\r\n                <p style='margin:0;'>\r\n                  Good luck with your career!<br/>\r\n                  Your Team @ DirectSubmit\r\n                </p>\r\n\r\n              </td>\r\n            </tr>\r\n\r\n            <!-- DIVIDER -->\r\n            <tr>\r\n              <td style='padding:0 30px;background:#ffffff;'>\r\n                <hr style='border:none;border-top:1px solid #dddddd;margin:0;'/>\r\n              </td>\r\n            </tr>\r\n\r\n            <!-- FOOTER -->\r\n            <tr>\r\n              <td style='background:#ffffff;padding:14px 20px;text-align:center;\r\n                         font-family:Arial,sans-serif;font-size:12px;color:#888888;line-height:1.6;'>\r\n                <p style='margin:0 0 4px;'>\r\n                  By using the DirectSubmit platform, you agree to the terms and conditions of our\r\n                  <a href='https://directsubmit.nycastings.com/terms-and-condition'\r\n                     target='_blank' style='color:#789eeb;text-decoration:underline;'>Terms of Service</a>\r\n                  and\r\n                  <a href='https://directsubmit.nycastings.com/privacy-policy'\r\n                     target='_blank' style='color:#789eeb;text-decoration:underline;'>Privacy Policy</a>.\r\n                  &copy; DirectSubmit\r\n                </p>\r\n                <p style='margin:0;text-align:center;background:#333333;padding:10px;font-size:13px;color:#ffffff;'>\r\n                  <a href='https://directsubmit.nycastings.com'\r\n                     style='color:#789eeb;text-decoration:none;'>DirectSubmit.com</a>\r\n                  &nbsp;|&nbsp; 1480 Vine St. &nbsp;|&nbsp; Los Angeles, CA 90028\r\n                </p>\r\n              </td>\r\n            </tr>\r\n\r\n          </table>\r\n\r\n        </td></tr>\r\n        </table>\r\n        </body>\r\n        </html>";
			_emailService.SendDirectorWelcomeEmail(userDetails?.Email ?? string.Empty, subject2, body2);
		}
		else
		{
			string subject3 = "Welcome to DirectSubmit – Your Journey Begins!";
			string body3 = $"<!DOCTYPE html>\r\n        <html>\r\n        <head>\r\n          <meta charset='UTF-8'/>\r\n          <meta name='viewport' content='width=device-width, initial-scale=1.0'/>\r\n        </head>\r\n        <body style='margin:0;padding:0;background:#f4f4f4;font-family:Arial,sans-serif;font-size:14px;color:#333333;'>\r\n\r\n        <table width='100%' cellpadding='0' cellspacing='0' border='0' style='background:#f4f4f4;'>\r\n        <tr><td align='center' style='padding:20px 0;'>\r\n\r\n          <table width='600' cellpadding='0' cellspacing='0' border='0'\r\n                 style='background:#ffffff;border:1px solid #dddddd;'>\r\n\r\n            <!-- HEADER -->\r\n            <tr>\r\n              <td style='background:#333333;padding:0;text-align:center;'>\r\n                <img src='https://files.constantcontact.com/82886fe2301/302cc6b2-5b9a-4673-8b39-143010d6b262.jpg?rdr=true'\r\n                     alt='DirectSubmit' width='600'\r\n                     style='display:block;width:100%;max-width:600px;border:0;'/>\r\n              </td>\r\n            </tr>\r\n\r\n            <!-- BLUE BANNER -->\r\n            <tr>\r\n              <td style='background:#789eeb;padding:14px 30px;'>\r\n                <span style='color:#ffffff;font-size:18px;font-weight:bold;'>\r\n                  Welcome to DirectSubmit – Your Journey Begins!\r\n                </span>\r\n              </td>\r\n            </tr>\r\n\r\n            <!-- BODY -->\r\n            <tr>\r\n              <td style='background:#ffffff;padding:28px 30px;\r\n                         font-family:Arial,sans-serif;font-size:14px;\r\n                         color:#333333;line-height:1.6;'>\r\n\r\n                <p style='margin:0 0 16px;'>\r\n                  Dear {userDetails.FirstName} {userDetails.LastName},\r\n                </p>\r\n\r\n                <p style='margin:0 0 16px;'>\r\n                  We are excited to welcome you to <strong>DirectSubmit</strong>!\r\n                  We believe that your new role will open up a world of exciting\r\n                  opportunities, and we are thrilled to have you as part of our community.\r\n                </p>\r\n\r\n                <p style='margin:0 0 8px;'>Your account details are as follows:</p>\r\n\r\n                <p style='margin:0 0 16px;'>\r\n                  <strong>User Name:</strong> {userDetails.FirstName} {userDetails.LastName}<br/>\r\n                  <strong>User Id:</strong> {userDetails.UserName}<br/>\r\n                  <strong>Password:</strong> {userDetails.Password}<br/>\r\n                  <strong>Location Code:</strong> {userDetails.State}\r\n                </p>\r\n\r\n                <p style='margin:0 0 16px;'>\r\n                  As a User, you now have access to a range of tools and features\r\n                  that will help you find the perfect talent for your projects.\r\n                  We&apos;re excited to see how you&apos;ll take advantage of these\r\n                  resources to grow and succeed.\r\n                </p>\r\n\r\n                <p style='margin:0 0 24px;'>\r\n                  We wish you the best of luck on your journey with DirectSubmit,\r\n                  and we are here to support you every step of the way.\r\n                  Your next big opportunity is just around the corner!\r\n                </p>\r\n\r\n                <p style='margin:0 0 24px;'>\r\n                  <a href='https://directsubmit.nycastings.com/login'\r\n                     style='background:#789eeb;color:#ffffff;\r\n                            padding:12px 28px;text-decoration:none;\r\n                            border-radius:4px;font-size:14px;\r\n                            font-weight:bold;display:inline-block;'>\r\n                    Login to Your Account\r\n                  </a>\r\n                </p>\r\n\r\n                <p style='margin:0;'>\r\n                  Good luck with your career!<br/>\r\n                  Your Team @ DirectSubmit\r\n                </p>\r\n\r\n              </td>\r\n            </tr>\r\n\r\n            <!-- DIVIDER -->\r\n            <tr>\r\n              <td style='padding:0 30px;background:#ffffff;'>\r\n                <hr style='border:none;border-top:1px solid #dddddd;margin:0;'/>\r\n              </td>\r\n            </tr>\r\n\r\n            <!-- FOOTER -->\r\n            <tr>\r\n              <td style='background:#ffffff;padding:14px 20px;text-align:center;\r\n                         font-family:Arial,sans-serif;font-size:12px;color:#888888;line-height:1.6;'>\r\n                <p style='margin:0 0 4px;'>\r\n                  By using the DirectSubmit platform, you agree to the terms and conditions of our\r\n                  <a href='https://directsubmit.nycastings.com/terms-and-condition'\r\n                     target='_blank' style='color:#789eeb;text-decoration:underline;'>Terms of Service</a>\r\n                  and\r\n                  <a href='https://directsubmit.nycastings.com/privacy-policy'\r\n                     target='_blank' style='color:#789eeb;text-decoration:underline;'>Privacy Policy</a>.\r\n                  &copy; DirectSubmit\r\n                </p>\r\n                <p style='margin:0;text-align:center;background:#333333;padding:10px;font-size:13px;color:#ffffff;'>\r\n                  <a href='https://directsubmit.nycastings.com'\r\n                     style='color:#789eeb;text-decoration:none;'>DirectSubmit.com</a>\r\n                  &nbsp;|&nbsp; 1480 Vine St. &nbsp;|&nbsp; Los Angeles, CA 90028\r\n                </p>\r\n              </td>\r\n            </tr>\r\n\r\n          </table>\r\n\r\n        </td></tr>\r\n        </table>\r\n        </body>\r\n        </html>";
			_emailService.SendTalentWelcomeEmail(userDetails?.Email ?? string.Empty, subject3, body3);
		}
	}

	private static Dictionary<string, object> TakeUserDataFields(AddUserDetails usersDetails)
	{
		Dictionary<string, object> obj = new Dictionary<string, object>
		{
			{
				"@LastName",
				usersDetails?.LastName ?? string.Empty
			},
			{
				"@FirstName",
				usersDetails?.FirstName ?? string.Empty
			},
			{
				"@UserName",
				usersDetails?.UserName ?? string.Empty
			},
			{
				"@Password",
				usersDetails?.Password ?? string.Empty
			},
			{
				"@Email",
				usersDetails?.Email ?? string.Empty
			}
		};
		int? num = usersDetails?.AccountType;
		obj.Add("@AccountType", num.HasValue ? ((object)num.GetValueOrDefault()) : DBNull.Value);
		obj.Add("@State", usersDetails?.State ?? string.Empty);
		obj.Add("@Phone", usersDetails?.Phone ?? string.Empty);
		obj.Add("@CompanyName", usersDetails?.CompanyName ?? string.Empty);
		obj.Add("@AffiliationIds", string.Join(",", usersDetails.AffiliationName));
		return obj;
	}

	public UserDetails GetUserDetails(string username, string password)
	{
		string procedureName = "USP_NEW_VALIDATE_USER";
		Dictionary<string, object> parameters = new Dictionary<string, object>
		{
			{ "@login", username },
			{ "@pwd", password }
		};
		DataTable dataTable = _dbManager.ReadData(procedureName, CommandType.StoredProcedure, parameters);
		if (dataTable.Rows.Count == 0)
		{
			return null;
		}
		DataRow row = dataTable.Rows[0];
		return new UserDetails
		{
			UserFirstName = row["NAM_FIRST_USER"].ToString(),
			UserLastName = row["NAM_LAST_USER"].ToString(),
			UserRole = row["NAM_ROLE"].ToString(),
			RoleId = Convert.ToInt32(row["IDN_ROLE"]),
			UserId = Convert.ToInt32(row["IDN_USER"]),
			RecordCreatedDate = ((row["DTE_RECORD_CRTD"] == DBNull.Value) ? ((DateTime?)null) : new DateTime?(Convert.ToDateTime(row["DTE_RECORD_CRTD"]))),
			City = row["affcity"].ToString(),
			State = row["affstate"].ToString(),
			Email = row["EMAIL"].ToString(),
			ActiveFlag = (row["CDE_ACTIVE_FLAG"] != DBNull.Value && Convert.ToBoolean(row["CDE_ACTIVE_FLAG"])),
			Login = row["CDE_LOGIN"].ToString(),
			CompanyName = row["CompanyName"].ToString(),
			PhoneNumber = row["PhoneNumber"].ToString(),
			VerifiedDirector = Convert.ToBoolean(row["VerifiedDirector"]),
			TotalCredits = ((row["TotalCredits"] != DBNull.Value) ? Convert.ToInt32(row["TotalCredits"]) : 0)
		};
	}

	public object Authenticate(string username, string password)
	{
		UserDetails user = GetUserDetails(username, password);
		if (user == null)
		{
			return new
			{
				status = "Error",
				message = "Invalid username or password.",
				data = (object)null,
				count = 0,
				httpStatusCode = 401
			};
		}
		try
		{
			JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
			byte[] key = Encoding.UTF8.GetBytes(_jwtSecret);
			SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
			{
				Expires = DateTime.UtcNow.AddHours(1.0),
				SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), "http://www.w3.org/2001/04/xmldsig-more#hmac-sha256")
			};
			SecurityToken token = jwtSecurityTokenHandler.CreateToken(tokenDescriptor);
			string jwtToken = jwtSecurityTokenHandler.WriteToken(token);
			return new
			{
				status = "Ok",
				message = (string)null,
				data = new
				{
					UserFirstName = user.UserFirstName,
					UserLastName = user.UserLastName,
					UserRole = user.UserRole,
					UserId = user.UserId,
					RoleId = user.RoleId,
					City = user.City,
					State = user.State,
					RecordCreatedDate = user.RecordCreatedDate,
					Email = user.Email,
					Login = user.Login,
					ActiveFlag = user.ActiveFlag,
					CompanyName = user.CompanyName,
					PhoneNumber = user.PhoneNumber,
					VerifiedDirector = user.VerifiedDirector,
					TotalCredits = user.TotalCredits,
					token = jwtToken
				},
				count = 1,
				httpStatusCode = 200
			};
		}
		catch (Exception ex)
		{
			Console.WriteLine("Error generating JWT Token: " + ex.Message);
			return new
			{
				status = "Error",
				message = "Error generating token.",
				data = (object)null,
				count = 0,
				httpStatusCode = 500
			};
		}
	}

	public IEnumerable<LocationModel> GetLocations()
	{
		string procedureName = "USP_GET_LOCATIONS";
		DataTable dataTable = _dbManager.ReadData(procedureName, CommandType.StoredProcedure);
		foreach (DataRow row in dataTable.Rows)
		{
			yield return new LocationModel
			{
				LocationCode = row.Field<string>("T_LocationCode"),
				LocationState = row.Field<string>("T_LocationState"),
				LocationCity = row.Field<string>("T_LocationCity"),
				LocationStateAbbreviatoion = row.Field<string>("T_LocationStateAbbr")
			};
		}
	}

	public IEnumerable<EthnicityModel> GetAllEthnicities()
	{
		string procedureName = "USP_GET_ETHNICITIES";
		DataTable dataTable = _dbManager.ReadData(procedureName, CommandType.StoredProcedure);
		foreach (DataRow row in dataTable.Rows)
		{
			yield return new EthnicityModel
			{
				EthnicityId = row.Field<int>("EthnicityId"),
				EthnicityName = row.Field<string>("EthnicityName"),
				ActiveFlag = row.Field<bool>("ActiveFlag")
			};
		}
	}

	public IEnumerable<VocalRangeModel> GetAllVocalRanges()
	{
		string procedureName = "USP_GET_VOCAL_RANGES";
		DataTable dataTable = _dbManager.ReadData(procedureName, CommandType.StoredProcedure);
		foreach (DataRow row in dataTable.Rows)
		{
			yield return new VocalRangeModel
			{
				VocalRangeId = row.Field<int>("VocalRangeId"),
				VocalRangeName = row.Field<string>("VocalRangeName"),
				ActiveFlag = row.Field<bool>("ActiveFlag")
			};
		}
	}

	public IEnumerable<EyeColorModel> GetAllEyeColors()
	{
		string procedureName = "USP_GET_EYE_COLOR";
		DataTable dataTable = _dbManager.ReadData(procedureName, CommandType.StoredProcedure);
		foreach (DataRow row in dataTable.Rows)
		{
			yield return new EyeColorModel
			{
				EyeColorId = row.Field<int>("EyeColorId"),
				EyeColorName = row.Field<string>("EyeColorName"),
				ActiveFlag = row.Field<bool>("ActiveFlag")
			};
		}
	}

	public IEnumerable<HairColorModel> GetAllHairColors()
	{
		string procedureName = "USP_GET_HAIR_COLOR";
		DataTable dataTable = _dbManager.ReadData(procedureName, CommandType.StoredProcedure);
		foreach (DataRow row in dataTable.Rows)
		{
			yield return new HairColorModel
			{
				HairColorId = row.Field<int>("HairColorId"),
				HairColorName = row.Field<string>("HairColorName"),
				ActiveFlag = row.Field<bool>("ActiveFlag")
			};
		}
	}

	public IEnumerable<TalentModel> GetActiveTalents()
	{
		string procedureName = "USP_GET_ACTIVE_TALENTS";
		DataTable dataTable = _dbManager.ReadData(procedureName, CommandType.StoredProcedure);
		foreach (DataRow row in dataTable.Rows)
		{
			yield return new TalentModel
			{
				TalentId = row.Field<int>("TalentId"),
				TalentName = row.Field<string>("TalentName"),
				ActiveFlag = row.Field<bool>("ActiveFlag")
			};
		}
	}

	public IEnumerable<AffiliationModel> GetAllAffiliations()
	{
		string procedureName = "USP_GET_ALL_AFFILIATIONS";
		DataTable dt = _dbManager.ReadData(procedureName, CommandType.StoredProcedure);
		foreach (DataRow row in dt.Rows)
		{
			yield return new AffiliationModel
			{
				AffiliationId = row.Field<int>("AffiliationId"),
				AffiliationName = row.Field<string>("AffiliationName")
			};
		}
	}

	public IEnumerable<VocalTypeModel> GetVocalTypes()
	{
		string procedureName = "USP_GET_VOCAL_TYPES";
		DataTable dataTable = _dbManager.ReadData(procedureName, CommandType.StoredProcedure);
		foreach (DataRow row in dataTable.Rows)
		{
			yield return new VocalTypeModel
			{
				VocalTypeId = row.Field<int>("VocalTypeId"),
				VocalTypeName = row.Field<string>("VocalTypeName")
			};
		}
	}

	public UserNavigationStageResponse GetUserNavigationStage(int userId)
	{
		string procedureName = "USP_GET_USER_NAVIGATION_STAGE";
		Dictionary<string, object> parameters = new Dictionary<string, object> { { "@UserId", userId } };
		DataTable dataTable = _dbManager.ReadData(procedureName, CommandType.StoredProcedure, parameters);
		if (dataTable.Rows.Count == 0)
		{
			return null;
		}
		DataRow row = dataTable.Rows[0];
		return new UserNavigationStageResponse
		{
			Stage = Convert.ToInt32(row["Stage"])
		};
	}

	public IEnumerable<PayTypeModel> GetAllPayTypes()
	{
		string procedureName = "USP_GET_PAY_TYPES";
		DataTable dataTable = _dbManager.ReadData(procedureName, CommandType.StoredProcedure);
		foreach (DataRow row in dataTable.Rows)
		{
			yield return new PayTypeModel
			{
				PayId = row.Field<int>("PayId"),
				PayType = row.Field<string>("PayType")
			};
		}
	}

	public IEnumerable<JobCategory> GetAllJobCategories()
	{
		string procedureName = "USP_GET_ACTIVE_JOB_CATEGORIES";
		DataTable dataTable = _dbManager.ReadData(procedureName, CommandType.StoredProcedure);
		foreach (DataRow row in dataTable.Rows)
		{
			yield return new JobCategory
			{
				JobCategoryId = row.Field<int>("JobCategoryId"),
				JobCategoryName = row.Field<string>("JobCategoryName"),
				HasDailySheet = row.Field<byte>("HasDailySheet"),
				DailySheetOrder = row.Field<short>("DailySheetOrder"),
				ActiveFlag = row.Field<bool>("ActiveFlag")
			};
		}
	}

	public bool ChangeUserPassword(ChangePasswordRequestModel request)
	{
		string procedureName = "USP_ChangeUserPassword";
		Dictionary<string, object> parameters = TakeChangePasswordParams(request);
		return _dbManager.ExecuteScalarResult(procedureName, CommandType.StoredProcedure, parameters) == 1;
	}

	private Dictionary<string, object> TakeChangePasswordParams(ChangePasswordRequestModel request)
	{
		return new Dictionary<string, object>
		{
			{ "@Email", request.Email },
			{
				"@OldPassword",
				request.OldPassword ?? string.Empty
			},
			{
				"@NewPassword",
				request.NewPassword ?? string.Empty
			}
		};
	}

	public IEnumerable<NotificationMessageModel> GetActiveNotificationsByUser(int userId, string userRole)
	{
		string procedure = "USP_GetActiveNotificationsByUser";
		List<NotificationMessageModel> result = new List<NotificationMessageModel>();
		Dictionary<string, object> parameters = new Dictionary<string, object>
		{
			{ "@UserId", userId },
			{ "@UserRole", userRole }
		};
		foreach (DataRow row in _dbManager.ReadData(procedure, CommandType.StoredProcedure, parameters).Rows)
		{
			result.Add(new NotificationMessageModel
			{
				ID = row.Field<int>("ID"),
				Title = row.Field<string>("Title"),
				Message = row.Field<string>("Message"),
				CreatedAt = row.Field<DateTime>("CreatedAt")
			});
		}
		return result;
	}

	public bool ForgotPassword(ForgotPasswordRequestModel request, out string newPassword)
	{
		newPassword = GenerateRandomPassword();
		Dictionary<string, object> parameters = new Dictionary<string, object>
		{
			{ "@Email", request.Email },
			{ "@NewPassword", newPassword }
		};
		DataTable dt = _dbManager.ReadData("USP_New_ForgotPassword", CommandType.StoredProcedure, parameters);
		int num;
		if (dt != null && dt.Rows.Count > 0)
		{
			num = ((Convert.ToInt32(dt.Rows[0]["StatusCode"]) == 1) ? 1 : 0);
			if (num != 0)
			{
				string subject = "Your Password Reset Request";
				string body = "<!DOCTYPE html>\r\n            <html>\r\n            <head>\r\n              <meta charset='UTF-8'/>\r\n              <meta name='viewport' content='width=device-width, initial-scale=1.0'/>\r\n            </head>\r\n            <body style='margin:0;padding:0;background:#f4f4f4;font-family:Arial,sans-serif;font-size:14px;color:#333333;'>\r\n\r\n            <table width='100%' cellpadding='0' cellspacing='0' border='0' style='background:#f4f4f4;'>\r\n            <tr><td align='center' style='padding:20px 0;'>\r\n\r\n              <table width='600' cellpadding='0' cellspacing='0' border='0'\r\n                     style='background:#ffffff;border:1px solid #dddddd;'>\r\n\r\n                <!-- HEADER -->\r\n                <tr>\r\n                  <td style='background:#333333;padding:0;text-align:center;'>\r\n                    <img src='https://files.constantcontact.com/82886fe2301/302cc6b2-5b9a-4673-8b39-143010d6b262.jpg?rdr=true'\r\n                         alt='DirectSubmit' width='600'\r\n                         style='display:block;width:100%;max-width:600px;border:0;'/>\r\n                  </td>\r\n                </tr>\r\n\r\n                <!-- BLUE BANNER -->\r\n                <tr>\r\n                  <td style='background:#789eeb;padding:14px 30px;'>\r\n                    <span style='color:#ffffff;font-size:18px;font-weight:bold;'>\r\n                      Your Password Reset Request\r\n                    </span>\r\n                  </td>\r\n                </tr>\r\n\r\n                <!-- BODY — plain paragraphs only -->\r\n                <tr>\r\n                  <td style='background:#ffffff;padding:28px 30px;\r\n                             font-family:Arial,sans-serif;font-size:14px;\r\n                             color:#333333;line-height:1.6;'>\r\n\r\n                    <p style='margin:0 0 16px;'>Hi,</p>\r\n\r\n                    <p style='margin:0 0 16px;'>\r\n                      We&apos;ve received a request to reset the password for your\r\n                      account associated with this email address.\r\n                    </p>\r\n\r\n                    <p style='margin:0 0 8px;'>\r\n                      <strong>Your new temporary password is:</strong>\r\n                    </p>\r\n\r\n                    <p style='margin:0 0 24px;font-size:22px;\r\n                              color:#2b2b2b;letter-spacing:2px;'>\r\n                      <strong>" + newPassword + "</strong>\r\n                    </p>\r\n\r\n                    <p style='margin:0 0 16px;'>\r\n                      For your security, please log in and change this password\r\n                      as soon as possible.\r\n                    </p>\r\n\r\n                    <p style='margin:0 0 24px;'>\r\n                      If you didn&apos;t request this change or need help, feel free\r\n                      to reach out to our support team.\r\n                    </p>\r\n\r\n                    <p style='margin:0 0 24px;'>\r\n                      <a href='https://directsubmit.nycastings.com/login'\r\n                         style='background:#789eeb;color:#ffffff;\r\n                                padding:12px 28px;text-decoration:none;\r\n                                border-radius:4px;font-size:14px;\r\n                                font-weight:bold;display:inline-block;'>\r\n                        Login to Your Account\r\n                      </a>\r\n                    </p>\r\n\r\n                    <p style='margin:0;'>\r\n                      Thank you,<br/>\r\n                      The DirectSubmit Team\r\n                    </p>\r\n\r\n                  </td>\r\n                </tr>\r\n\r\n                <!-- DIVIDER -->\r\n                <tr>\r\n                  <td style='padding:0 30px;background:#ffffff;'>\r\n                    <hr style='border:none;border-top:1px solid #dddddd;margin:0;'/>\r\n                  </td>\r\n                </tr>\r\n\r\n                <!-- FOOTER -->\r\n               <tr>\r\n                  <td style='background:#ffffff;padding:14px 20px;text-align:center;\r\n                             font-family:Arial,sans-serif;font-size:12px;color:#888888;line-height:1.6;'>\r\n                    <p style='margin:0 0 4px;'>\r\n                      By using the DirectSubmit platform, you agree to the terms and conditions of our\r\n                      <a href='https://directsubmit.nycastings.com/terms-and-condition'\r\n                         target='_blank' style='color:#4d90fe;text-decoration:underline;'>Terms of Service</a>\r\n                      and\r\n                      <a href='https://directsubmit.nycastings.com/privacy-policy'\r\n                         target='_blank' style='color:#4d90fe;text-decoration:underline;'>Privacy Policy</a>.\r\n                      &copy; DirectSubmit\r\n                    </p>\r\n                    <p style='margin:0;text-align:center;background:#333333;padding:10px;font-size:13px;color:#ffffff;'>\r\n                      <a href='https://directsubmit.nycastings.com'\r\n                         style='color:#4d90fe;text-decoration:none;'>DirectSubmit.com</a>\r\n                      &nbsp;|&nbsp; 1480 Vine St. &nbsp;|&nbsp; Los Angeles, CA 90028\r\n                    </p>\r\n                  </td>\r\n                </tr>\r\n\r\n              </table>\r\n\r\n            </td></tr>\r\n            </table>\r\n            </body>\r\n            </html>";
				_emailService.SendEmail(request.Email, subject, body);
			}
		}
		else
		{
			num = 0;
		}
		return (byte)num != 0;
	}

	private string GenerateRandomPassword(int length = 10)
	{
		Random random = new Random();
		return new string((from s in Enumerable.Repeat("ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789@#$!", length)
			select s[random.Next(s.Length)]).ToArray());
	}

	public IEnumerable<RepresentationHeadingModel> GetRepresentationHeadings()
	{
		string procedureName = "USP_Get_RepresentationHeadings";
		DataTable dataTable = _dbManager.ReadData(procedureName, CommandType.StoredProcedure);
		foreach (DataRow row in dataTable.Rows)
		{
			yield return new RepresentationHeadingModel
			{
				ID = row.Field<int>("ID"),
				HeadingName = row.Field<string>("HeadingName"),
				Active = row.Field<bool>("Active")
			};
		}
	}

	public IEnumerable<Affiliation2Model> GetCustomOrderedAffiliations()
	{
		string procedureName = "USP_GetAffiliations_CustomOrder";
		List<Affiliation2Model> affiliations = new List<Affiliation2Model>();
		foreach (DataRow row in _dbManager.ReadData(procedureName, CommandType.StoredProcedure).Rows)
		{
			affiliations.Add(new Affiliation2Model
			{
				AffiliationId = row.Field<int>("AffiliationId"),
				AffiliationName = row["AffiliationName"].ToString()
			});
		}
		return affiliations;
	}

	public bool SendContactUsEmail(string name, string email, string department, string message)
	{
		string subject = "New Contact Us Submission - " + department;
		string body = $"<!DOCTYPE html>\r\n            <html>\r\n            <head>\r\n              <meta charset='UTF-8'/>\r\n              <meta name='viewport' content='width=device-width, initial-scale=1.0'/>\r\n            </head>\r\n            <body style='margin:0;padding:0;background:#f4f4f4;font-family:Arial,sans-serif;font-size:14px;color:#333333;'>\r\n\r\n            <table width='100%' cellpadding='0' cellspacing='0' border='0' style='background:#f4f4f4;'>\r\n            <tr><td align='center' style='padding:20px 0;'>\r\n\r\n              <table width='600' cellpadding='0' cellspacing='0' border='0'\r\n                     style='background:#ffffff;border:1px solid #dddddd;'>\r\n\r\n                <!-- HEADER -->\r\n                <tr>\r\n                  <td style='background:#333333;padding:0;text-align:center;'>\r\n                    <img src='https://files.constantcontact.com/82886fe2301/302cc6b2-5b9a-4673-8b39-143010d6b262.jpg?rdr=true'\r\n                         alt='DirectSubmit' width='600'\r\n                         style='display:block;width:100%;max-width:600px;border:0;'/>\r\n                  </td>\r\n                </tr>\r\n\r\n                <!-- BLUE BANNER -->\r\n                <tr>\r\n                  <td style='background:#4d90fe;padding:14px 30px;'>\r\n                    <span style='color:#ffffff;font-size:18px;font-weight:bold;'>\r\n                      New Contact Us Submission — {department}\r\n                    </span>\r\n                  </td>\r\n                </tr>\r\n\r\n                <!-- BODY — plain paragraphs only -->\r\n                <tr>\r\n                  <td style='background:#ffffff;padding:28px 30px;\r\n                             font-family:Arial,sans-serif;font-size:14px;\r\n                             color:#333333;line-height:1.6;'>\r\n\r\n                    <p style='margin:0 0 16px;'>\r\n                      <strong>New message submitted via the Contact Us form:</strong>\r\n                    </p>\r\n\r\n                    <p style='margin:0 0 10px;'>\r\n                      <strong>Name:</strong> {name}\r\n                    </p>\r\n\r\n                    <p style='margin:0 0 10px;'>\r\n                      <strong>Email:</strong> {email}\r\n                    </p>\r\n\r\n                    <p style='margin:0 0 10px;'>\r\n                      <strong>Department:</strong> {department}\r\n                    </p>\r\n\r\n                    <p style='margin:0 0 8px;'>\r\n                      <strong>Message:</strong>\r\n                    </p>\r\n\r\n                    <!-- Message in light box — stands out clearly -->\r\n                    <p style='margin:0 0 24px;padding:14px;\r\n                              background:#f9f9f9;border:1px solid #dddddd;\r\n                              border-radius:4px;line-height:1.6;'>\r\n                      {message}\r\n                    </p>\r\n\r\n                    <p style='margin:0;'>\r\n                      Best regards,<br/>\r\n                      DirectSubmit.com\r\n                    </p>\r\n\r\n                  </td>\r\n                </tr>\r\n\r\n                <!-- DIVIDER -->\r\n                <tr>\r\n                  <td style='padding:0 30px;background:#ffffff;'>\r\n                    <hr style='border:none;border-top:1px solid #dddddd;margin:0;'/>\r\n                  </td>\r\n                </tr>\r\n\r\n                <!-- FOOTER -->\r\n                <tr>\r\n                  <td style='background:#ffffff;padding:14px 20px;text-align:center;\r\n                             font-family:Arial,sans-serif;font-size:12px;color:#888888;line-height:1.6;'>\r\n                    <p style='margin:0 0 4px;'>\r\n                      By using the DirectSubmit platform, you agree to the terms and conditions of our\r\n                      <a href='https://directsubmit.nycastings.com/terms-and-condition'\r\n                         target='_blank' style='color:#4d90fe;text-decoration:underline;'>Terms of Service</a>\r\n                      and\r\n                      <a href='https://directsubmit.nycastings.com/privacy-policy'\r\n                         target='_blank' style='color:#4d90fe;text-decoration:underline;'>Privacy Policy</a>.\r\n                      &copy; DirectSubmit\r\n                    </p>\r\n                    <p style='margin:0;text-align:center;background:#333333;padding:10px;font-size:13px;color:#ffffff;'>\r\n                      <a href='https://directsubmit.nycastings.com'\r\n                         style='color:#4d90fe;text-decoration:none;'>DirectSubmit.com</a>\r\n                      &nbsp;|&nbsp; 1480 Vine St. &nbsp;|&nbsp; Los Angeles, CA 90028\r\n                    </p>\r\n                  </td>\r\n                </tr>\r\n\r\n              </table>\r\n\r\n            </td></tr>\r\n            </table>\r\n            </body>\r\n            </html>";
		return _emailService.SendEmail("admin@nycastings.com", subject, body);
	}

	public string GetUserName(int userId)
	{
		string procedureName = "USP_GET_USERNAME";
		Dictionary<string, object> parameters = new Dictionary<string, object> { { "@UserId", userId } };
		DataTable dataTable = _dbManager.ReadData(procedureName, CommandType.StoredProcedure, parameters);
		string userName = "";
		IEnumerator enumerator = dataTable.Rows.GetEnumerator();
		try
		{
			if (enumerator.MoveNext())
			{
				userName = Convert.ToString(((DataRow)enumerator.Current)["UserName"]);
			}
		}
		finally
		{
			IDisposable disposable = enumerator as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}
		return userName;
	}

	public int GetUserId(string userName)
	{
		string procedureName = "USP_GET_USERID";
		Dictionary<string, object> parameters = new Dictionary<string, object> { { "@Name", userName } };
		DataTable dataTable = _dbManager.ReadData(procedureName, CommandType.StoredProcedure, parameters);
		int userId = 0;
		IEnumerator enumerator = dataTable.Rows.GetEnumerator();
		try
		{
			if (enumerator.MoveNext())
			{
				userId = Convert.ToInt32(((DataRow)enumerator.Current)["UserId"]);
			}
		}
		finally
		{
			IDisposable disposable = enumerator as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}
		return userId;
	}

	public bool UpdateResumeName(int userId, string name)
	{
		string procedureName = "USP_UPDATE_RESUMENAME";
		Dictionary<string, object> parameters = new Dictionary<string, object>
		{
			{ "@UserId", userId },
			{ "@Name", name }
		};
		return _dbManager.InsertOrUpdateData(procedureName, CommandType.StoredProcedure, parameters);
	}

	public bool SaveUserPrivacySettings(SaveUserPrivacySettingsRequestModel model)
	{
		string procedureName = "USP_SaveUserPrivacySettings";
		Dictionary<string, object> parameters = new Dictionary<string, object>
		{
			{ "@UserId", model.UserId },
			{
				"@IsPublicSearchOptOut",
				((object)model.IsPublicSearchOptOut) ?? DBNull.Value
			},
			{
				"@IsSearchEmailOptOut",
				((object)model.IsSearchEmailOptOut) ?? DBNull.Value
			},
			{
				"@IsResumeViewedEmailOptOut",
				((object)model.IsResumeViewedEmailOptOut) ?? DBNull.Value
			}
		};
		return _dbManager.InsertOrUpdateData(procedureName, CommandType.StoredProcedure, parameters);
	}

	public UserPrivacySettingsResponseModel GetUserPrivacySettings(int userId)
	{
		string procedureName = "USP_GetUserPrivacySettings";
		Dictionary<string, object> parameters = new Dictionary<string, object> { { "@UserId", userId } };
		DataTable dataTable = _dbManager.ReadData(procedureName, CommandType.StoredProcedure, parameters);
		if (dataTable.Rows.Count == 0)
		{
			return null;
		}
		DataRow row = dataTable.Rows[0];
		return new UserPrivacySettingsResponseModel
		{
			UserId = Convert.ToInt32(row["UserId"]),
			IsPublicSearchOptOut = Convert.ToBoolean(row["IsPublicSearchOptOut"]),
			IsSearchEmailOptOut = Convert.ToBoolean(row["IsSearchEmailOptOut"]),
			IsResumeViewedEmailOptOut = Convert.ToBoolean(row["IsResumeViewedEmailOptOut"])
		};
	}

	public bool ForgotUsername(ForgotUsernameRequestModel request)
	{
		Dictionary<string, object> parameters = new Dictionary<string, object> { { "@Email", request.Email } };
		DataTable dt = _dbManager.ReadData("USP_GET_USERNAME_BY_EMAIL", CommandType.StoredProcedure, parameters);
		if (dt == null || dt.Rows.Count == 0)
		{
			return false;
		}
		string userName = dt.Rows[0]["UserName"]?.ToString() ?? "";
		if (string.IsNullOrWhiteSpace(userName))
		{
			return false;
		}
		string subject = "Your DirectSubmit Username";
		string body = "<!DOCTYPE html>\r\n            <html>\r\n            <head>\r\n              <meta charset='UTF-8'/>\r\n              <meta name='viewport' content='width=device-width, initial-scale=1.0'/>\r\n            </head>\r\n            <body style='margin:0;padding:0;background:#f4f4f4;font-family:Arial,sans-serif;font-size:14px;color:#333333;'>\r\n\r\n            <table width='100%' cellpadding='0' cellspacing='0' border='0' style='background:#f4f4f4;'>\r\n            <tr><td align='center' style='padding:20px 0;'>\r\n\r\n              <table width='600' cellpadding='0' cellspacing='0' border='0'\r\n                     style='background:#ffffff;border:1px solid #dddddd;'>\r\n\r\n                <!-- HEADER -->\r\n                <tr>\r\n                  <td style='background:#333333;padding:0;text-align:center;'>\r\n                    <img src='https://files.constantcontact.com/82886fe2301/302cc6b2-5b9a-4673-8b39-143010d6b262.jpg?rdr=true'\r\n                         alt='DirectSubmit' width='600'\r\n                         style='display:block;width:100%;max-width:600px;border:0;'/>\r\n                  </td>\r\n                </tr>\r\n\r\n                <!-- BLUE BANNER -->\r\n                <tr>\r\n                  <td style='background:#789eeb;padding:14px 30px;'>\r\n                    <span style='color:#ffffff;font-size:18px;font-weight:bold;'>\r\n                      Your Username\r\n                    </span>\r\n                  </td>\r\n                </tr>\r\n\r\n                <!-- BODY -->\r\n                <tr>\r\n                  <td style='background:#ffffff;padding:28px 30px;\r\n                             font-family:Arial,sans-serif;font-size:14px;\r\n                             color:#333333;line-height:1.6;'>\r\n\r\n                    <p style='margin:0 0 16px;'>Hi,</p>\r\n\r\n                    <p style='margin:0 0 16px;'>\r\n                      We&apos;ve received a request to recover the username for your\r\n                      account associated with this email address.\r\n                    </p>\r\n\r\n                    <p style='margin:0 0 8px;'>\r\n                      <strong>Your username is:</strong>\r\n                    </p>\r\n\r\n                    <p style='margin:0 0 24px;font-size:22px;\r\n                              color:#2b2b2b;letter-spacing:1px;'>\r\n                      <strong>" + userName + "</strong>\r\n                    </p>\r\n\r\n                    <p style='margin:0 0 24px;'>\r\n                      <a href='https://directsubmit.nycastings.com/login'\r\n                         style='background:#789eeb;color:#ffffff;\r\n                                padding:12px 28px;text-decoration:none;\r\n                                border-radius:4px;font-size:14px;\r\n                                font-weight:bold;display:inline-block;'>\r\n                        Login to Your Account\r\n                      </a>\r\n                    </p>\r\n\r\n                    <p style='margin:0 0 16px;'>\r\n                      If you didn&apos;t request this, you can safely ignore this email\r\n                      or reach out to our support team.\r\n                    </p>\r\n\r\n                    <p style='margin:0;'>\r\n                      Thank you,<br/>\r\n                      The DirectSubmit Team\r\n                    </p>\r\n\r\n                  </td>\r\n                </tr>\r\n\r\n                <!-- DIVIDER -->\r\n                <tr>\r\n                  <td style='padding:0 30px;background:#ffffff;'>\r\n                    <hr style='border:none;border-top:1px solid #dddddd;margin:0;'/>\r\n                  </td>\r\n                </tr>\r\n\r\n                <!-- FOOTER -->\r\n                <tr>\r\n                  <td style='background:#ffffff;padding:14px 20px;text-align:center;\r\n                             font-family:Arial,sans-serif;font-size:12px;color:#888888;line-height:1.6;'>\r\n                    <p style='margin:0 0 4px;'>\r\n                      By using the DirectSubmit platform, you agree to the terms and conditions of our\r\n                      <a href='https://directsubmit.nycastings.com/terms-and-condition'\r\n                         target='_blank' style='color:#4d90fe;text-decoration:underline;'>Terms of Service</a>\r\n                      and\r\n                      <a href='https://directsubmit.nycastings.com/privacy-policy'\r\n                         target='_blank' style='color:#4d90fe;text-decoration:underline;'>Privacy Policy</a>.\r\n                      &copy; DirectSubmit\r\n                    </p>\r\n                    <p style='margin:0;text-align:center;background:#333333;padding:10px;font-size:13px;color:#ffffff;'>\r\n                      <a href='https://directsubmit.nycastings.com'\r\n                         style='color:#4d90fe;text-decoration:none;'>DirectSubmit.com</a>\r\n                      &nbsp;|&nbsp; 1480 Vine St. &nbsp;|&nbsp; Los Angeles, CA 90028\r\n                    </p>\r\n                  </td>\r\n                </tr>\r\n\r\n              </table>\r\n\r\n            </td></tr>\r\n            </table>\r\n            </body>\r\n            </html>";
		_emailService.SendEmail(request.Email, subject, body);
		return true;
	}

	public bool IsVerifiedDirector(int userId)
	{
		string procedureName = "USP_IS_VERIFIED_DIRECTOR";
		Dictionary<string, object> parameters = new Dictionary<string, object> { { "@UserId", userId } };
		DataTable dataTable = _dbManager.ReadData(procedureName, CommandType.StoredProcedure, parameters);
		if (dataTable.Rows.Count == 0)
		{
			return false;
		}
		DataRow row = dataTable.Rows[0];
		if (row["VerifiedDirector"] != DBNull.Value)
		{
			return Convert.ToBoolean(row["VerifiedDirector"]);
		}
		return false;
	}

	public IEnumerable<CountryCodeModel> GetCountryCodes()
	{
		string procedureName = "USP_GET_COUNTRY_CODES";
		DataTable dt = _dbManager.ReadData(procedureName, CommandType.StoredProcedure, new Dictionary<string, object>());
		List<CountryCodeModel> countries = new List<CountryCodeModel>();
		if (dt == null || dt.Rows.Count == 0)
		{
			return countries;
		}
		foreach (DataRow row in dt.Rows)
		{
			countries.Add(new CountryCodeModel
			{
				CountryId = ((row["CountryId"] != DBNull.Value) ? Convert.ToInt32(row["CountryId"]) : 0),
				CountryName = (row["CountryName"]?.ToString() ?? ""),
				ISO2 = (row["ISO2"]?.ToString() ?? ""),
				ISO3 = (row["ISO3"]?.ToString() ?? ""),
				DialCode = (row["DialCode"]?.ToString() ?? "")
			});
		}
		return countries;
	}

	public bool UnsubscribeUser(int userId, string email)
	{
		return _dbManager.InsertOrUpdateData("USP_UNSUBSCRIBE_USER", CommandType.StoredProcedure, new Dictionary<string, object>
		{
			{ "@UserId", userId },
			{
				"@Email",
				((object)email) ?? ((object)DBNull.Value)
			}
		});
	}

	public bool UnsubscribeEventMail(int userId, string email)
	{
		return _dbManager.InsertOrUpdateData("USP_UNSUBSCRIBE_EVENT_MAIL", CommandType.StoredProcedure, new Dictionary<string, object>
		{
			{ "@UserId", userId },
			{
				"@Email",
				((object)email) ?? ((object)DBNull.Value)
			}
		});
	}
}

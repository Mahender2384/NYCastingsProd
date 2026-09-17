using System;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using ChargeBee.Api;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NYCasting.Core.Models;
using NYCastings.API.Core.Contracts.AdminInterface;
using NYCastings.API.Core.Contracts.NewCastingNoticeInterface;
using NYCastings.API.Core.Contracts.ProfileSearchInterface;
using NYCastings.API.Core.Contracts.SearchNoticeInterface;
using NYCastings.API.Core.Contracts.TalentProfileInterface;
using NYCastings.API.Core.Contracts.UserInterface;
using NYCastings.API.Hubs;
using NYCastings.API.Infrastructure.Services;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using Swashbuckle.AspNetCore.SwaggerGen;

[CompilerGenerated]
internal class Program
{
	private static void Main(string[] args)
	{
		WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
		ApiConfig.Configure(builder.Configuration["Chargebee:Site"], builder.Configuration["Chargebee:ApiKey"]);
		string logBasePath = "Logs";
		string yearFolder = Path.Combine(logBasePath, DateTime.UtcNow.Year.ToString());
		string monthFolder = Path.Combine(yearFolder, DateTime.UtcNow.ToString("MMMM"));
		Directory.CreateDirectory(monthFolder);
		string logFilePath = Path.Combine(monthFolder, "logs.txt");
		Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).Enrich.FromLogContext().Enrich.WithMachineName().Enrich.WithThreadId().WriteTo.Console().WriteTo.File(logFilePath, LogEventLevel.Verbose, "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}", null, 1073741824L, null, buffered: false, shared: true, null, RollingInterval.Day, rollOnFileSizeLimit: false, 31).CreateLogger();
		builder.Host.UseSerilog();
		builder.Services.AddControllers();
		builder.Services.Configure(delegate(FormOptions options)
		{
			options.MultipartBodyLengthLimit = 524288000L;
		});
		builder.Services.AddEndpointsApiExplorer();
		builder.Services.AddSwaggerGen(delegate(SwaggerGenOptions c)
		{
			c.SwaggerDoc("v1", new OpenApiInfo
			{
				Title = "NYCasting API",
				Version = "v1"
			});
			c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
			{
				Name = "Authorization",
				Type = SecuritySchemeType.ApiKey,
				Scheme = "Bearer",
				BearerFormat = "JWT",
				In = ParameterLocation.Header,
				Description = "Enter 'Bearer' followed by your JWT token."
			});
			c.AddSecurityRequirement(new OpenApiSecurityRequirement { 
			{
				new OpenApiSecurityScheme
				{
					Reference = new OpenApiReference
					{
						Type = ReferenceType.SecurityScheme,
						Id = "Bearer"
					}
				},
				new string[0]
			} });
		});
		builder.Services.AddCors(delegate(CorsOptions options)
		{
			options.AddPolicy("NYCastingPolicy", delegate(CorsPolicyBuilder corsPolicyBuilder)
			{
				corsPolicyBuilder.WithOrigins("https://app.directsubmit.com", "https://directsubmit.nycastings.com", "https://directsubmit.com", "https://www.directsubmit.com", "https://casting-dev.directsubmit.com", "http://localhost:3000", "https://50.6.201.196:444").AllowAnyHeader().AllowAnyMethod()
					.AllowCredentials();
			});
		});
		builder.Services.Configure<ConnectionString>(builder.Configuration.GetSection("ConnectionStrings"));
		builder.Services.AddScoped<IUserService, UserService>();
		builder.Services.AddScoped<INewCastingNoticeService, NewCastingNoticeService>();
		builder.Services.AddScoped<IProfileSearchInterface, ProfileSearchService>();
		builder.Services.AddScoped<ITalentProfileService, TalentProfileService>();
		builder.Services.AddScoped<ISearchNoticeInterface, SearchNoticeService>();
		builder.Services.AddScoped<IAdminService, AdminService>();
		builder.Services.AddScoped<EmailService>();
		builder.Services.AddScoped<RoleAlertService>();
		builder.Services.AddSignalR();
		builder.Services.AddHangfire(delegate(IGlobalConfiguration config)
		{
			config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170).UseSimpleAssemblyNameTypeSerializer().UseRecommendedSerializerSettings()
				.UseSqlServerStorage(builder.Configuration.GetConnectionString("NYCasting"), new SqlServerStorageOptions
				{
					CommandBatchMaxTimeout = TimeSpan.FromMinutes(5.0),
					SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5.0),
					QueuePollInterval = TimeSpan.Zero,
					UseRecommendedIsolationLevel = true,
					DisableGlobalLocks = true
				});
		});
		builder.Services.AddHangfireServer(delegate(BackgroundJobServerOptions options)
		{
			options.WorkerCount = 2;
		});
		string jwtKey = builder.Configuration["Jwt:Key"];
		if (string.IsNullOrEmpty(jwtKey))
		{
			throw new ArgumentNullException("Jwt:Key", "JWT key is missing in configuration.");
		}
		builder.Services.AddAuthentication("Bearer").AddJwtBearer("Bearer", delegate(JwtBearerOptions options)
		{
			options.TokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuer = false,
				ValidateAudience = false,
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
			};
		});
		builder.Services.AddHttpContextAccessor();
		builder.Services.AddHttpClient();
		builder.WebHost.ConfigureKestrel(delegate(KestrelServerOptions options)
		{
			options.Limits.MaxRequestBodySize = 524288000L;
		});
		builder.Services.Configure(delegate(IISServerOptions options)
		{
			options.MaxRequestBodySize = 524288000L;
		});
		builder.Services.Configure(delegate(FormOptions options)
		{
			options.MultipartBodyLengthLimit = 524288000L;
			options.ValueLengthLimit = int.MaxValue;
			options.MultipartHeadersLengthLimit = int.MaxValue;
		});
		builder.Services.Configure(delegate(FormOptions options)
		{
			options.MultipartBodyLengthLimit = 524288000L;
			options.ValueLengthLimit = int.MaxValue;
			options.MultipartHeadersLengthLimit = int.MaxValue;
			options.MemoryBufferThreshold = 81920;
		});
		WebApplication app = builder.Build();
		app.UseHttpsRedirection();
		app.UseRouting();
		app.UseCors("NYCastingPolicy");
		app.UseAuthentication();
		app.UseAuthorization();
		app.UseStaticFiles();
		string uploadsPath = Path.Combine(builder.Environment.ContentRootPath, "Uploads");
		if (!Directory.Exists(uploadsPath))
		{
			Directory.CreateDirectory(uploadsPath);
		}
		app.UseStaticFiles(new StaticFileOptions
		{
			FileProvider = new PhysicalFileProvider(uploadsPath),
			RequestPath = "/Uploads"
		});
		app.MapControllers();
		app.MapHub<ChatHub>("/chatHub");
		app.UseSwagger();
		app.UseSwaggerUI();
		app.UseHangfireDashboard();
		RecurringJob.AddOrUpdate("process-email-queue", (EmailService svc) => svc.SendQueuedEmailsAsync(), "*/30 * * * * *", new RecurringJobOptions
		{
			TimeZone = TimeZoneInfo.Utc
		});
		RecurringJob.AddOrUpdate("purge-email-queue", (EmailService svc) => svc.PurgeOldQueuedEmailsAsync(), Cron.Daily(0), new RecurringJobOptions
		{
			TimeZone = TimeZoneInfo.Utc
		});
		app.Run();
		Log.CloseAndFlush();
	}
}

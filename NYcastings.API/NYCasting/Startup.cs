using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NYCasting;

public class Startup
{
	public IConfiguration configuration { get; }

	private IWebHostEnvironment _currentEnvironment { get; set; }

	public Startup(IConfiguration configuration, IWebHostEnvironment env)
	{
		this.configuration = configuration;
		_currentEnvironment = env;
	}

	public void ConfigureServices(IServiceCollection services)
	{
		string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "NYCatingApplicationLogs", "ApplicationLogs.txt");
	}
}

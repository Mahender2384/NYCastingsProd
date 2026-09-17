using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NYCasting.Core.Models;
using NYCasting.Infrastructure.DataAccess;

namespace NYCastings.API.Infrastructure.Services;

public class BaseApiService : Controller
{
	private readonly DbManager _dbManager;

	public BaseApiService(IOptions<ConnectionString> dbConfig)
	{
		_dbManager = new DbManager(dbConfig.Value.NYCasting);
	}
}

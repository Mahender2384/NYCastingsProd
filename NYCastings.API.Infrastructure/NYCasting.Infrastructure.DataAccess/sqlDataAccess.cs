using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace NYCasting.Infrastructure.DataAccess;

public class sqlDataAccess
{
	public interface IDbManager
	{
		Task<List<T>> GetDataAsync<T>(string procedureName, CommandType commandType, IEnumerable<SqlParameter> parameters);
	}

	private static string cstring = string.Empty;

	public sqlDataAccess(string ConnectionString = null)
	{
		cstring = ConnectionString;
	}

	public static SqlConnection CreateConnection()
	{
		SqlConnection sqlConnection = new SqlConnection(cstring);
		sqlConnection.Open();
		return sqlConnection;
	}

	public static SqlConnection CreateConnection(string connectionString)
	{
		SqlConnection sqlConnection = new SqlConnection(connectionString);
		sqlConnection.Open();
		return sqlConnection;
	}

	public void CloseConnection(SqlConnection connection)
	{
		connection.Close();
		connection.Dispose();
	}

	public SqlTransaction CreateTransaction(SqlConnection connection, string conName = null)
	{
		if (string.IsNullOrEmpty(conName))
		{
			return connection.BeginTransaction(IsolationLevel.Serializable);
		}
		return connection.BeginTransaction();
	}
}

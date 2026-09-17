using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

namespace NYCasting.Infrastructure.DataAccess;

public class DbManager : sqlDataAccess
{
	public DbManager(string ConnectionString = null)
		: base(ConnectionString)
	{
	}

	public void ExecuteQuery(string sqlCommand, CommandType commandType, SqlConnection connection, Dictionary<string, object> paramCollection = null, SqlTransaction transaction = null)
	{
		try
		{
			SqlCommand _sqlCommand = new SqlCommand(sqlCommand, connection);
			if (transaction != null)
			{
				_sqlCommand.Transaction = transaction;
			}
			_sqlCommand.CommandType = commandType;
			_sqlCommand.CommandTimeout = 120;
			if (paramCollection != null)
			{
				foreach (KeyValuePair<string, object> param in paramCollection)
				{
					_sqlCommand.Parameters.AddWithValue(param.Key, param.Value);
				}
			}
			_sqlCommand.ExecuteNonQuery();
		}
		catch (Exception ex)
		{
			throw new Exception(ex.Message);
		}
	}

	public void ExecuteQuery(string sqlcommand, CommandType commandType, Dictionary<string, object> paramCollection = null, List<SqlParameter> outputParams = null)
	{
		try
		{
			using SqlConnection connection = sqlDataAccess.CreateConnection();
			SqlCommand _sqlCommand = new SqlCommand(sqlcommand, connection);
			_sqlCommand.CommandType = commandType;
			if (paramCollection != null)
			{
				foreach (KeyValuePair<string, object> param in paramCollection)
				{
					_sqlCommand.Parameters.AddWithValue(param.Key, param.Value);
				}
			}
			if (outputParams != null)
			{
				foreach (SqlParameter sqlParam in outputParams)
				{
					_sqlCommand.Parameters.Add(sqlParam);
				}
			}
			_sqlCommand.ExecuteNonQuery();
		}
		catch (Exception ex)
		{
			throw new Exception(ex.Message);
		}
	}

	public int ExecuteScalarResult(string sqlcommand, CommandType commandType, Dictionary<string, object> paramCollection = null)
	{
		using SqlConnection connection = sqlDataAccess.CreateConnection();
		SqlCommand cmd = new SqlCommand(sqlcommand, connection);
		cmd.CommandType = commandType;
		if (paramCollection != null)
		{
			foreach (KeyValuePair<string, object> param in paramCollection)
			{
				cmd.Parameters.AddWithValue(param.Key, param.Value);
			}
		}
		return Convert.ToInt32(cmd.ExecuteScalar());
	}

	public DataTable ReadData(string sqlCommand, CommandType commandType, Dictionary<string, object> paramCollection = null)
	{
		try
		{
			using SqlConnection connection = sqlDataAccess.CreateConnection();
			SqlDataAdapter _adapter = new SqlDataAdapter(sqlCommand, connection);
			_adapter.SelectCommand.CommandType = commandType;
			_adapter.SelectCommand.CommandTimeout = 120;
			if (paramCollection != null)
			{
				foreach (KeyValuePair<string, object> param in paramCollection)
				{
					_adapter.SelectCommand.Parameters.AddWithValue(param.Key, param.Value);
				}
			}
			DataTable _dt = new DataTable();
			_adapter.Fill(_dt);
			return _dt;
		}
		catch (Exception ex)
		{
			throw new Exception(ex.Message);
		}
	}

	public DataSet ReadDataSet(string sqlCommand, CommandType commandType, Dictionary<string, object> paramCollection = null)
	{
		try
		{
			using SqlConnection connection = sqlDataAccess.CreateConnection();
			SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand, connection);
			adapter.SelectCommand.CommandType = commandType;
			adapter.SelectCommand.CommandTimeout = 120;
			if (paramCollection != null)
			{
				foreach (KeyValuePair<string, object> param in paramCollection)
				{
					adapter.SelectCommand.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
				}
			}
			DataSet ds = new DataSet();
			adapter.Fill(ds);
			return ds;
		}
		catch (Exception ex)
		{
			throw new Exception("ReadDataSet Error: " + ex.Message);
		}
	}

	public DataSet ReadDataDataSet(string sqlCommand, CommandType commandType, Dictionary<string, object> paramCollection = null)
	{
		try
		{
			using SqlConnection connection = sqlDataAccess.CreateConnection();
			SqlDataAdapter _adapter = new SqlDataAdapter(sqlCommand, connection);
			_adapter.SelectCommand.CommandType = commandType;
			_adapter.SelectCommand.CommandTimeout = 1200;
			if (paramCollection != null)
			{
				foreach (KeyValuePair<string, object> param in paramCollection)
				{
					_adapter.SelectCommand.Parameters.AddWithValue(param.Key, param.Value);
				}
			}
			DataSet ds = new DataSet();
			_adapter.Fill(ds);
			return ds;
		}
		catch (Exception ex)
		{
			throw new Exception(ex.Message);
		}
	}

	public bool InsertOrUpdateData(string sqlCommand, CommandType commandType, Dictionary<string, object> paramCollection)
	{
		try
		{
			using SqlConnection con = sqlDataAccess.CreateConnection();
			SqlCommand command = new SqlCommand(sqlCommand, con);
			command.CommandType = commandType;
			if (paramCollection != null)
			{
				foreach (KeyValuePair<string, object> param in paramCollection)
				{
					command.Parameters.AddWithValue(param.Key, param.Value);
				}
			}
			command.ExecuteNonQuery();
			return true;
		}
		catch (Exception ex)
		{
			throw new Exception(ex.Message);
		}
	}

	public bool DeleteData(string sqlCommand, CommandType commandType, Dictionary<string, object> paramCollection = null)
	{
		try
		{
			using SqlConnection connection = sqlDataAccess.CreateConnection();
			SqlCommand command = new SqlCommand(sqlCommand, connection);
			command.CommandType = commandType;
			if (paramCollection != null)
			{
				foreach (KeyValuePair<string, object> param in paramCollection)
				{
					command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
				}
			}
			command.ExecuteNonQuery();
			return true;
		}
		catch (Exception ex)
		{
			throw new Exception("Error occurred while deleting data: " + ex.Message, ex);
		}
	}
}
